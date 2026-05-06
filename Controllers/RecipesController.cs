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

            var ingredients = await _ingredientService.GetAllAsync(GetCurrentUserId());
            ViewBag.AvailableIngredients = ingredients
                .OrderBy(i => i.Name)
                .Select(i => new paw_np.Models.ViewModels.IngredientOptionViewModel { Id = i.Id, Name = i.Name })
                .ToList();

            return View(recipe);
        }

        // GET: Recipes/Create
        public async Task<IActionResult> Create()
        {
            var model = new paw_np.Models.ViewModels.RecipeCreateViewModel();
            await PopulateAvailableIngredients(model);
            return View(model);
        }

        // POST: Recipes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(paw_np.Models.ViewModels.RecipeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAvailableIngredients(model);
                return View(model);
            }

            var recipe = new Recipe
            {
                Name = model.Name,
                Description = model.Description,
                Instructions = model.Instructions,
                Servings = model.Servings,
                PrepTimeMin = model.PrepTimeMin,
                CookTimeMin = model.CookTimeMin
            };

            var result = await _recipeService.CreateAsync(GetCurrentUserId(), recipe);
            if (!result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage);
                }
                await PopulateAvailableIngredients(model);
                return View(model);
            }

            // Now add each ingredient row
            var createdRecipe = result.Recipe!;
            foreach (var row in model.Ingredients)
            {
                int ingredientId = row.IngredientId;

                // If the user is creating a new ingredient inline
                if (ingredientId == 0 && !string.IsNullOrWhiteSpace(row.NewIngredientName))
                {
                    var newIngredient = new Ingredient
                    {
                        Name = row.NewIngredientName.Trim(),
                        CaloriesPer100g = row.NewCaloriesPer100g,
                        Proteins = row.NewProteins,
                        Carbs = row.NewCarbs,
                        Fats = row.NewFats,
                        Unit = "g"
                    };

                    var ingResult = await _ingredientService.CreateAsync(GetCurrentUserId(), newIngredient);
                    if (ingResult.Success && ingResult.Ingredient != null)
                    {
                        ingredientId = ingResult.Ingredient.Id;
                    }
                    else
                    {
                        continue; // skip if failed
                    }
                }

                if (ingredientId > 0)
                {
                    var ri = new RecipeIngredient
                    {
                        IngredientId = ingredientId,
                        Quantity = row.Quantity,
                        Unit = row.Unit ?? "g"
                    };
                    await _recipeService.AddIngredientAsync(GetCurrentUserId(), createdRecipe.Id, ri);
                }
            }

            return RedirectToAction(nameof(Details), new { id = createdRecipe.Id });
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Instructions,Servings,PrepTimeMin,CookTimeMin")] Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Recipe.User));
            ModelState.Remove(nameof(Recipe.UserId));
            ModelState.Remove(nameof(Recipe.RecipeIngredients));
            ModelState.Remove(nameof(Recipe.JournalEntries));
            ModelState.Remove(nameof(Recipe.PlannerItems));

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

        private async Task PopulateAvailableIngredients(paw_np.Models.ViewModels.RecipeCreateViewModel model)
        {
            var ingredients = await _ingredientService.GetAllAsync(GetCurrentUserId());
            model.AvailableIngredients = ingredients
                .OrderBy(i => i.Name)
                .Select(i => new paw_np.Models.ViewModels.IngredientOptionViewModel { Id = i.Id, Name = i.Name })
                .ToList();
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : 0;
        }
    }
}
