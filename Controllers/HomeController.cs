using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using NutriPlan.Data;
using NutriPlan.Models;
using paw_np.Models;
using paw_np.Models.ViewModels;
using paw_np.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace paw_np.Controllers;

public class HomeController : Controller
{
    private readonly NutriPlanDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IFoodJournalService _foodJournalService;
    private readonly IMealPlannerService _mealPlannerService;
    private readonly IShoppingListService _shoppingListService;

    public HomeController(
        NutriPlanDbContext context,
        IIngredientService ingredientService,
        IRecipeService recipeService,
        IFoodJournalService foodJournalService,
        IMealPlannerService mealPlannerService,
        IShoppingListService shoppingListService)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _foodJournalService = foodJournalService;
        _mealPlannerService = mealPlannerService;
        _shoppingListService = shoppingListService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);

        var today = DateOnly.FromDateTime(DateTime.Today);

        // Get all data
        var ingredients = await _ingredientService.GetAllAsync(userId);
        var recipes = await _recipeService.GetAllAsync(userId);
        var journals = await _foodJournalService.GetAllAsync(userId);
        var planners = await _mealPlannerService.GetAllAsync(userId);
        var shoppingLists = await _shoppingListService.GetAllAsync(userId);

        // Today's journal
        var todayJournal = journals.FirstOrDefault(j => j.JournalDate == today);

        // Calculate macros from today's entries (based on ingredients)
        decimal totalProteins = 0, totalCarbs = 0, totalFats = 0;
        if (todayJournal != null)
        {
            foreach (var entry in todayJournal.JournalEntries)
            {
                if (entry.Ingredient != null)
                {
                    var factor = entry.Quantity / 100m;
                    totalProteins += entry.Ingredient.Proteins * factor;
                    totalCarbs += entry.Ingredient.Carbs * factor;
                    totalFats += entry.Ingredient.Fats * factor;
                }
            }
        }

        var model = new DashboardViewModel
        {
            Today = today,
            CaloriesConsumed = todayJournal?.TotalKcalConsumed ?? 0,
            CaloricTarget = user?.CaloricTarget ?? 2000,
            TotalProteins = Math.Round(totalProteins, 1),
            TotalCarbs = Math.Round(totalCarbs, 1),
            TotalFats = Math.Round(totalFats, 1),
            TotalIngredients = ingredients.Count,
            TotalRecipes = recipes.Count,
            TotalJournals = journals.Count,
            TotalMealPlans = planners.Count,
            TotalShoppingLists = shoppingLists.Count,
            RecentJournals = journals
                .OrderByDescending(j => j.JournalDate)
                .Take(5)
                .Select(j => new RecentJournalViewModel
                {
                    Id = j.Id,
                    JournalDate = j.JournalDate,
                    TotalKcalConsumed = j.TotalKcalConsumed,
                    EntriesCount = j.JournalEntries.Count
                })
                .ToList(),
            UpcomingPlans = planners
                .Where(p => p.PlanDate >= today)
                .OrderBy(p => p.PlanDate)
                .Take(5)
                .Select(p => new RecentMealPlanViewModel
                {
                    Id = p.Id,
                    PlanDate = p.PlanDate,
                    MealType = p.MealType,
                    ItemsCount = p.PlannerItems.Count
                })
                .ToList()
        };

        return View("dashboard", model);
    }

    public IActionResult Meals()
    {
        return RedirectToAction("Index", "FoodJournal");
    }

    public IActionResult Planner()
    {
        return RedirectToAction("Index", "MealPlanner");
    }

    public IActionResult Recipes()
    {
        return RedirectToAction("Index", "Recipes");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var ingredients = await _ingredientService.GetAllAsync(userId);
        var recipes = await _recipeService.GetAllAsync(userId);
        var journals = await _foodJournalService.GetAllAsync(userId);

        var model = new ProfileViewModel
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            CaloricTarget = user.CaloricTarget,
            CreatedAt = user.CreatedAt,
            TotalIngredients = ingredients.Count,
            TotalRecipes = recipes.Count,
            TotalJournals = journals.Count
        };

        return View("profile", model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("profile", model);
        }

        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        // Check email uniqueness
        var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != userId);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already used by another account.");
            return View("profile", model);
        }

        user.Name = model.Name.Trim();
        user.Email = model.Email.Trim();
        user.CaloricTarget = model.CaloricTarget;

        // Optional password change
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
        }

        await _context.SaveChangesAsync();

        // Update the auth cookie with new name/email
        await SignInAsync(user, isPersistent: false);

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Index));
        }

        return View("login", new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("login", model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View("login", model);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View("login", model);
        }

        await SignInAsync(user, model.RememberMe);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Index));
        }

        return View("register", new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("register", model);
        }

        var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(model.Email), "Email is already used.");
            return View("register", model);
        }

        var user = new User
        {
            Name = model.Name,
            Email = model.Email
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult ShoppingList()
    {
        return RedirectToAction("Index", "ShoppingList");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Index));
    }

    private async Task SignInAsync(User user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = isPersistent });
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : 0;
    }
}
