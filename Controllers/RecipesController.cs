using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NutriPlan.Models;
using paw_np.Services.Interfaces;

namespace paw_np.Controllers
{
    [Authorize]
    public class RecipesController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly IIngredientService _ingredientService;

        public RecipesController(IRecipeService recipeService, IIngredientService ingredientService)
        {
            _recipeService = recipeService;
            _ingredientService = ingredientService;
        }

        // GET: Recipes
        public async Task<IActionResult> Index()
        {
            var recipes = await _recipeService.GetAllAsync(GetCurrentUserId());
            return View(recipes);
        }

        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _recipeService.GetByIdWithIngredientsAsync(id.Value, GetCurrentUserId());
            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // GET: Recipes/Create
        public IActionResult Create()
        {
            return View(new Recipe());
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Servings,PrepTimeMin,CookTimeMin")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                var result = await _recipeService.CreateAsync(GetCurrentUserId(), recipe);
                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }

                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage);
                }
            }
            return View(recipe);
        }

        // GET: Recipes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _recipeService.GetByIdAsync(id.Value, GetCurrentUserId());
            if (recipe == null)
            {
                return NotFound();
            }
            return View(recipe);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Servings,PrepTimeMin,CookTimeMin")] Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _recipeService.UpdateAsync(id, GetCurrentUserId(), recipe);
                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }

                if (result.ErrorMessage == "Reteta nu exista.")
                {
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage);
                }
            }
            return View(recipe);
        }

        // GET: Recipes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _recipeService.GetByIdAsync(id.Value, GetCurrentUserId());
            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _recipeService.DeleteAsync(id, GetCurrentUserId());
            if (!result.Success && result.ErrorMessage == "Reteta nu exista.")
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIngredient(int recipeId, int ingredientId, decimal quantity, string? unit)
        {
            var recipeIngredient = new RecipeIngredient
            {
                IngredientId = ingredientId,
                Quantity = quantity,
                Unit = unit ?? "g"
            };

            var result = await _recipeService.AddIngredientAsync(GetCurrentUserId(), recipeId, recipeIngredient);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Details), new { id = recipeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIngredient(int id, int recipeId)
        {
            var result = await _recipeService.DeleteIngredientAsync(GetCurrentUserId(), id);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Details), new { id = recipeId });
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : 0;
        }
    }
}
