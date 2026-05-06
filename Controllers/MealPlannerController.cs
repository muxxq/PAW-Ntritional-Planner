using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NutriPlan.Models;
using paw_np.Models.ViewModels;
using paw_np.Services.Interfaces;

namespace paw_np.Controllers
{
    [Authorize]
    public class MealPlannerController : Controller
    {
        private readonly IMealPlannerService _mealPlannerService;
        private readonly IIngredientService _ingredientService;
        private readonly IRecipeService _recipeService;

        public MealPlannerController(
            IMealPlannerService mealPlannerService,
            IIngredientService ingredientService,
            IRecipeService recipeService)
        {
            _mealPlannerService = mealPlannerService;
            _ingredientService = ingredientService;
            _recipeService = recipeService;
        }

        // GET: MealPlanner
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var planners = await _mealPlannerService.GetWeekAsync(GetCurrentUserId(), today);
            
            var model = new MealPlannerIndexViewModel
            {
                CreateForm = new MealPlannerFormViewModel
                {
                    PlanDate = today
                }
            };

            for (int i = 0; i < 7; i++)
            {
                model.Calendar.Days.Add(today.AddDays(i));
            }

            foreach (var mp in planners)
            {
                var key = $"{mp.PlanDate:yyyy-MM-dd}_{mp.MealType}";
                model.Calendar.CellData[key] = new MealPlannerListItemViewModel
                {
                    Id = mp.Id,
                    UserId = mp.UserId,
                    PlanDate = mp.PlanDate,
                    MealType = mp.MealType,
                    Notes = mp.Notes,
                    ItemsCount = mp.PlannerItems.Count
                };
            }

            return View(model);
        }

        // GET: MealPlanner/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var planner = await _mealPlannerService.GetByIdAsync(id, GetCurrentUserId());
            if (planner is null)
            {
                return NotFound();
            }

            return View(await BuildDetailsViewModelAsync(planner));
        }

        // GET: MealPlanner/Create
        public IActionResult Create()
        {
            return View(new MealPlannerFormViewModel
            {
                PlanDate = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        // POST: MealPlanner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MealPlannerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var planner = new MealPlanner
            {
                PlanDate = model.PlanDate,
                MealType = model.MealType,
                Notes = model.Notes
            };

            var result = await _mealPlannerService.CreateAsync(GetCurrentUserId(), planner);
            if (!result.Success)
            {
                AddModelError(result.ErrorMessage);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MealPlanner/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var planner = await _mealPlannerService.GetByIdAsync(id, GetCurrentUserId());
            if (planner is null)
            {
                return NotFound();
            }

            var model = new MealPlannerFormViewModel
            {
                Id = planner.Id,
                UserId = planner.UserId,
                PlanDate = planner.PlanDate,
                MealType = planner.MealType,
                Notes = planner.Notes
            };

            return View(model);
        }

        // POST: MealPlanner/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MealPlannerFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var planner = new MealPlanner
            {
                Id = model.Id,
                PlanDate = model.PlanDate,
                MealType = model.MealType,
                Notes = model.Notes
            };

            var result = await _mealPlannerService.UpdateAsync(id, GetCurrentUserId(), planner);
            if (!result.Success)
            {
                if (result.ErrorMessage == "Planificarea nu exista.")
                {
                    return NotFound();
                }

                AddModelError(result.ErrorMessage);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MealPlanner/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var planner = await _mealPlannerService.GetByIdAsync(id, GetCurrentUserId());
            if (planner is null)
            {
                return NotFound();
            }

            var model = new MealPlannerDeleteViewModel
            {
                Id = planner.Id,
                UserId = planner.UserId,
                PlanDate = planner.PlanDate,
                MealType = planner.MealType,
                ItemsCount = planner.PlannerItems.Count
            };

            return View(model);
        }

        // POST: MealPlanner/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _mealPlannerService.DeleteAsync(id, GetCurrentUserId());
            if (!result.Success && result.ErrorMessage == "Planificarea nu exista.")
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: MealPlanner/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([Bind(Prefix = "NewItemForm")] PlannerItemFormViewModel itemForm)
        {
            if (!ModelState.IsValid)
            {
                var planner = await _mealPlannerService.GetByIdAsync(itemForm.PlannerId, GetCurrentUserId());
                if (planner is null)
                {
                    return NotFound();
                }

                var detailsModel = await BuildDetailsViewModelAsync(planner);
                detailsModel.NewItemForm = itemForm;
                await PopulateOptionsAsync(detailsModel.NewItemForm);
                return View(nameof(Details), detailsModel);
            }

            var item = new PlannerItem
            {
                RecipeId = itemForm.RecipeId,
                IngredientId = itemForm.IngredientId,
                Quantity = itemForm.Quantity
            };

            var result = await _mealPlannerService.AddItemAsync(GetCurrentUserId(), itemForm.PlannerId, item);
            if (!result.Success)
            {
                AddModelError(result.ErrorMessage);
                var planner = await _mealPlannerService.GetByIdAsync(itemForm.PlannerId, GetCurrentUserId());
                if (planner is null)
                {
                    return NotFound();
                }

                var detailsModel = await BuildDetailsViewModelAsync(planner);
                detailsModel.NewItemForm = itemForm;
                await PopulateOptionsAsync(detailsModel.NewItemForm);
                return View(nameof(Details), detailsModel);
            }

            return RedirectToAction(nameof(Details), new { id = itemForm.PlannerId });
        }

        // POST: MealPlanner/DeleteItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id, int plannerId)
        {
            var result = await _mealPlannerService.DeleteItemAsync(GetCurrentUserId(), id);
            if (!result.Success && result.ErrorMessage == "Elementul din planificare nu exista.")
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = plannerId });
        }

        private async Task<MealPlannerDetailsViewModel> BuildDetailsViewModelAsync(MealPlanner planner)
        {
            var model = new MealPlannerDetailsViewModel
            {
                Planner = new MealPlannerFormViewModel
                {
                    Id = planner.Id,
                    UserId = planner.UserId,
                    PlanDate = planner.PlanDate,
                    MealType = planner.MealType,
                    Notes = planner.Notes
                },
                Items = planner.PlannerItems
                    .Select(pi => new PlannerItemRowViewModel
                    {
                        Id = pi.Id,
                        PlannerId = pi.PlannerId,
                        RecipeId = pi.RecipeId,
                        IngredientId = pi.IngredientId,
                        DisplayName = pi.Recipe?.Name ?? pi.Ingredient?.Name ?? "Item",
                        Quantity = pi.Quantity
                    })
                    .ToList()
            };

            model.NewItemForm.PlannerId = planner.Id;
            await PopulateOptionsAsync(model.NewItemForm);

            return model;
        }

        private async Task PopulateOptionsAsync(PlannerItemFormViewModel model)
        {
            var userId = GetCurrentUserId();
            var ingredients = await _ingredientService.GetAllAsync(userId);
            var recipes = await _recipeService.GetAllAsync(userId);

            model.AvailableIngredients = ingredients
                .OrderBy(i => i.Name)
                .Select(i => new IngredientOptionViewModel { Id = i.Id, Name = i.Name })
                .ToList();

            model.AvailableRecipes = recipes
                .OrderBy(r => r.Name)
                .Select(r => new RecipeOptionViewModel { Id = r.Id, Name = r.Name })
                .ToList();
        }

        private void AddModelError(string? errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
            }
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : 0;
        }
    }
}
