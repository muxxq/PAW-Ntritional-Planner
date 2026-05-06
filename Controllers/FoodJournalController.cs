using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NutriPlan.Models;
using paw_np.Models.ViewModels;
using paw_np.Services.Interfaces;

namespace paw_np.Controllers
{
    [Authorize]
    public class FoodJournalController : Controller
    {
        private readonly IFoodJournalService _foodJournalService;
        private readonly IIngredientService _ingredientService;
        private readonly IRecipeService _recipeService;

        public FoodJournalController(
            IFoodJournalService foodJournalService,
            IIngredientService ingredientService,
            IRecipeService recipeService)
        {
            _foodJournalService = foodJournalService;
            _ingredientService = ingredientService;
            _recipeService = recipeService;
        }

        public async Task<IActionResult> Index()
        {
            var journals = await _foodJournalService.GetAllAsync(GetCurrentUserId());
            var model = new FoodJournalIndexViewModel
            {
                Journals = journals
                    .Select(journal => new FoodJournalListItemViewModel
                    {
                        Id = journal.Id,
                        UserId = journal.UserId,
                        JournalDate = journal.JournalDate,
                        Notes = journal.Notes,
                        TotalKcalConsumed = journal.TotalKcalConsumed,
                        EntriesCount = journal.JournalEntries.Count
                    })
                    .ToList(),
                CreateForm = new FoodJournalFormViewModel
                {
                    JournalDate = DateOnly.FromDateTime(DateTime.Today)
                }
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var journal = await _foodJournalService.GetByIdAsync(id, GetCurrentUserId());
            if (journal is null)
            {
                return NotFound();
            }

            return View(await BuildDetailsViewModelAsync(journal));
        }

        public IActionResult Create()
        {
            return View(new FoodJournalFormViewModel
            {
                JournalDate = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodJournalFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var journal = new FoodJournal
            {
                UserId = GetCurrentUserId(),
                JournalDate = model.JournalDate,
                Notes = model.Notes,
                TotalKcalConsumed = 0
            };

            var result = await _foodJournalService.CreateAsync(GetCurrentUserId(), journal);
            if (!result.Success)
            {
                AddModelError(result.ErrorMessage);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var journal = await _foodJournalService.GetByIdAsync(id, GetCurrentUserId());
            if (journal is null)
            {
                return NotFound();
            }

            var model = new FoodJournalFormViewModel
            {
                Id = journal.Id,
                UserId = journal.UserId,
                JournalDate = journal.JournalDate,
                Notes = journal.Notes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodJournalFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var journal = new FoodJournal
            {
                Id = model.Id,
                UserId = GetCurrentUserId(),
                JournalDate = model.JournalDate,
                Notes = model.Notes
            };

            var result = await _foodJournalService.UpdateAsync(id, GetCurrentUserId(), journal);
            if (!result.Success)
            {
                if (result.ErrorMessage == "Jurnalul nu exista.")
                {
                    return NotFound();
                }

                AddModelError(result.ErrorMessage);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var journal = await _foodJournalService.GetByIdAsync(id, GetCurrentUserId());
            if (journal is null)
            {
                return NotFound();
            }

            var model = new FoodJournalDeleteViewModel
            {
                Id = journal.Id,
                UserId = journal.UserId,
                JournalDate = journal.JournalDate,
                TotalKcalConsumed = journal.TotalKcalConsumed,
                EntriesCount = journal.JournalEntries.Count
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _foodJournalService.DeleteAsync(id, GetCurrentUserId());
            if (!result.Success && result.ErrorMessage == "Jurnalul nu exista.")
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEntry(FoodJournalDetailsViewModel model)
        {
            var entryForm = model.NewEntryForm;
            if (!ModelState.IsValid)
            {
                var journal = await _foodJournalService.GetByIdAsync(entryForm.JournalId, GetCurrentUserId());
                if (journal is null)
                {
                    return NotFound();
                }

                var detailsModel = await BuildDetailsViewModelAsync(journal);
                detailsModel.NewEntryForm = entryForm;
                return View(nameof(Details), detailsModel);
            }

            // Auto-calculate KcalConsumed
            decimal kcal = 0;
            if (entryForm.RecipeId.HasValue && entryForm.RecipeId > 0)
            {
                // Clear ingredient if recipe was selected
                entryForm.IngredientId = null;

                var recipe = await _recipeService.GetByIdAsync(entryForm.RecipeId.Value, GetCurrentUserId());
                if (recipe != null)
                {
                    // kcal per serving * quantity (servings)
                    var kcalPerServing = recipe.Servings > 0 ? recipe.TotalKcal / recipe.Servings : recipe.TotalKcal;
                    kcal = kcalPerServing * entryForm.Quantity;
                }
            }
            else if (entryForm.IngredientId.HasValue && entryForm.IngredientId > 0)
            {
                // Clear recipe if ingredient was selected
                entryForm.RecipeId = null;

                var ingredient = await _ingredientService.GetByIdAsync(entryForm.IngredientId.Value, GetCurrentUserId());
                if (ingredient != null)
                {
                    // CaloriesPer100g * quantity(g) / 100
                    kcal = ingredient.CaloriesPer100g * entryForm.Quantity / 100m;
                }
            }

            var entry = new JournalEntry
            {
                RecipeId = entryForm.RecipeId,
                IngredientId = entryForm.IngredientId,
                Quantity = entryForm.Quantity,
                MealType = entryForm.MealType,
                KcalConsumed = kcal,
                LoggedAt = entryForm.LoggedAt
            };

            var result = await _foodJournalService.AddEntryAsync(GetCurrentUserId(), entryForm.JournalId, entry);
            if (!result.Success)
            {
                AddModelError(result.ErrorMessage);
                var journal = await _foodJournalService.GetByIdAsync(entryForm.JournalId, GetCurrentUserId());
                if (journal is null)
                {
                    return NotFound();
                }

                var detailsModel = await BuildDetailsViewModelAsync(journal);
                detailsModel.NewEntryForm = entryForm;
                return View(nameof(Details), detailsModel);
            }

            return RedirectToAction(nameof(Details), new { id = entryForm.JournalId });
        }

        public async Task<IActionResult> EditEntry(int id)
        {
            var journals = await _foodJournalService.GetAllAsync(GetCurrentUserId());
            var journal = journals.FirstOrDefault(current => current.JournalEntries.Any(entry => entry.Id == id));
            var entry = journal?.JournalEntries.FirstOrDefault(current => current.Id == id);

            if (journal is null || entry is null)
            {
                return NotFound();
            }

            var model = new FoodJournalEntryFormViewModel
            {
                Id = entry.Id,
                JournalId = entry.JournalId,
                RecipeId = entry.RecipeId,
                IngredientId = entry.IngredientId,
                Quantity = entry.Quantity,
                MealType = entry.MealType,
                KcalConsumed = entry.KcalConsumed,
                LoggedAt = entry.LoggedAt
            };

            await PopulateEntryFormOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEntry(int id, FoodJournalEntryFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateEntryFormOptionsAsync(model);
                return View(model);
            }

            // Auto-calculate KcalConsumed
            decimal kcal = 0;
            if (model.RecipeId.HasValue && model.RecipeId > 0)
            {
                model.IngredientId = null;
                var recipe = await _recipeService.GetByIdAsync(model.RecipeId.Value, GetCurrentUserId());
                if (recipe != null)
                {
                    var kcalPerServing = recipe.Servings > 0 ? recipe.TotalKcal / recipe.Servings : recipe.TotalKcal;
                    kcal = kcalPerServing * model.Quantity;
                }
            }
            else if (model.IngredientId.HasValue && model.IngredientId > 0)
            {
                model.RecipeId = null;
                var ingredient = await _ingredientService.GetByIdAsync(model.IngredientId.Value, GetCurrentUserId());
                if (ingredient != null)
                {
                    kcal = ingredient.CaloriesPer100g * model.Quantity / 100m;
                }
            }

            var entry = new JournalEntry
            {
                RecipeId = model.RecipeId,
                IngredientId = model.IngredientId,
                Quantity = model.Quantity,
                MealType = model.MealType,
                KcalConsumed = kcal,
                LoggedAt = model.LoggedAt
            };

            var result = await _foodJournalService.UpdateEntryAsync(GetCurrentUserId(), id, entry);
            if (!result.Success)
            {
                if (result.ErrorMessage == "Intrarea nu exista.")
                {
                    return NotFound();
                }

                AddModelError(result.ErrorMessage);
                await PopulateEntryFormOptionsAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = model.JournalId });
        }

        private async Task PopulateEntryFormOptionsAsync(FoodJournalEntryFormViewModel model)
        {
            var userId = GetCurrentUserId();
            var ingredients = await _ingredientService.GetAllAsync(userId);
            var recipes = await _recipeService.GetAllAsync(userId);

            model.AvailableRecipes = recipes
                .OrderBy(r => r.Name)
                .Select(r => new RecipeOptionViewModel { Id = r.Id, Name = r.Name, TotalKcal = r.TotalKcal, Servings = r.Servings })
                .ToList();

            model.AvailableIngredients = ingredients
                .OrderBy(i => i.Name)
                .Select(i => new IngredientOptionViewModel { Id = i.Id, Name = i.Name, CaloriesPer100g = i.CaloriesPer100g })
                .ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntry(int id, int journalId)
        {
            var result = await _foodJournalService.DeleteEntryAsync(GetCurrentUserId(), id);
            if (!result.Success && result.ErrorMessage == "Intrarea nu exista.")
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = journalId });
        }

        private async Task<FoodJournalDetailsViewModel> BuildDetailsViewModelAsync(FoodJournal journal)
        {
            var userId = GetCurrentUserId();
            var ingredients = await _ingredientService.GetAllAsync(userId);
            var recipes = await _recipeService.GetAllAsync(userId);

            return new FoodJournalDetailsViewModel
            {
                Journal = new FoodJournalFormViewModel
                {
                    Id = journal.Id,
                    UserId = journal.UserId,
                    JournalDate = journal.JournalDate,
                    Notes = journal.Notes
                },
                TotalKcalConsumed = journal.TotalKcalConsumed,
                Entries = journal.JournalEntries
                    .OrderBy(entry => entry.LoggedAt)
                    .Select(entry => new FoodJournalEntryRowViewModel
                    {
                        Id = entry.Id,
                        JournalId = entry.JournalId,
                        RecipeId = entry.RecipeId,
                        IngredientId = entry.IngredientId,
                        EntryName = entry.Recipe?.Name ?? entry.Ingredient?.Name ?? "Entry",
                        Quantity = entry.Quantity,
                        MealType = entry.MealType,
                        KcalConsumed = entry.KcalConsumed,
                        LoggedAt = entry.LoggedAt
                    })
                    .ToList(),
                NewEntryForm = new FoodJournalEntryFormViewModel
                {
                    JournalId = journal.Id,
                    LoggedAt = DateTime.UtcNow
                },
                AvailableRecipes = recipes
                    .OrderBy(r => r.Name)
                    .Select(r => new RecipeOptionViewModel { Id = r.Id, Name = r.Name, TotalKcal = r.TotalKcal, Servings = r.Servings })
                    .ToList(),
                AvailableIngredients = ingredients
                    .OrderBy(i => i.Name)
                    .Select(i => new IngredientOptionViewModel { Id = i.Id, Name = i.Name, CaloriesPer100g = i.CaloriesPer100g })
                    .ToList()
            };
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
