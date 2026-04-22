using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NutriPlan.Models;
using paw_np.Models.ViewModels;
using paw_np.Services.Interfaces;

namespace paw_np.Controllers
{
    [Authorize]
    public class ShoppingListController : Controller
    {
        private readonly IShoppingListService _shoppingListService;
        private readonly IIngredientService _ingredientService;

        public ShoppingListController(
            IShoppingListService shoppingListService,
            IIngredientService ingredientService)
        {
            _shoppingListService = shoppingListService;
            _ingredientService = ingredientService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var shoppingLists = await _shoppingListService.GetAllAsync(userId);
            var viewModel = new ShoppingListIndexViewModel
            {
                ShoppingLists = shoppingLists
                    .Select(sl => new ShoppingListListItemViewModel
                    {
                        Id = sl.Id,
                        Name = sl.Name,
                        UserId = sl.UserId,
                        CreatedAt = sl.CreatedAt,
                        ItemCount = sl.Items.Count,
                        CheckedItemCount = sl.Items.Count(item => item.IsChecked)
                    })
                    .OrderByDescending(sl => sl.CreatedAt)
                    .ToList(),
                CreateForm = new ShoppingListFormViewModel()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var shoppingList = await _shoppingListService.GetByIdAsync(id, GetCurrentUserId());
            if (shoppingList is null)
            {
                return NotFound();
            }

            return View(await BuildDetailsViewModelAsync(shoppingList));
        }

        public IActionResult Create()
        {
            return View(new ShoppingListFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShoppingListFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var shoppingList = new ShoppingList
            {
                Name = model.Name,
                UserId = GetCurrentUserId()
            };

            var result = await _shoppingListService.CreateAsync(GetCurrentUserId(), shoppingList);
            if (!result.Success)
            {
                AddModelError(result.ErrorMessage);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var shoppingList = await _shoppingListService.GetByIdAsync(id, GetCurrentUserId());
            if (shoppingList is null)
            {
                return NotFound();
            }

            var viewModel = new ShoppingListFormViewModel
            {
                Id = shoppingList.Id,
                Name = shoppingList.Name,
                UserId = shoppingList.UserId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShoppingListFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var shoppingList = new ShoppingList
            {
                Id = model.Id,
                Name = model.Name,
                UserId = GetCurrentUserId()
            };

            var result = await _shoppingListService.UpdateAsync(id, GetCurrentUserId(), shoppingList);
            if (!result.Success)
            {
                if (result.ErrorMessage == "Lista de cumparaturi nu exista.")
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
            var shoppingList = await _shoppingListService.GetByIdAsync(id, GetCurrentUserId());
            if (shoppingList is null)
            {
                return NotFound();
            }

            var viewModel = new ShoppingListDeleteViewModel
            {
                Id = shoppingList.Id,
                Name = shoppingList.Name,
                UserId = shoppingList.UserId,
                CreatedAt = shoppingList.CreatedAt,
                ItemCount = shoppingList.Items.Count
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _shoppingListService.DeleteAsync(id, GetCurrentUserId());
            if (!result.Success && result.ErrorMessage == "Lista de cumparaturi nu exista.")
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([Bind(Prefix = "NewItemForm")] ShoppingListItemFormViewModel itemForm)
        {
            if (!ModelState.IsValid)
            {
                var shoppingList = await _shoppingListService.GetByIdAsync(itemForm.ShoppingListId, GetCurrentUserId());
                if (shoppingList is null)
                {
                    return NotFound();
                }

                var detailsViewModel = await BuildDetailsViewModelAsync(shoppingList);
                detailsViewModel.NewItemForm = itemForm;
                await PopulateIngredientOptionsAsync(detailsViewModel.NewItemForm);
                return View(nameof(Details), detailsViewModel);
            }

            var item = new ShoppingListItem
            {
                IngredientId = itemForm.IngredientId,
                CustomItemName = itemForm.CustomItemName,
                Quantity = itemForm.Quantity,
                Unit = itemForm.Unit,
                IsChecked = itemForm.IsChecked
            };

            var result = await _shoppingListService.AddItemAsync(GetCurrentUserId(), itemForm.ShoppingListId, item);
            if (!result.Success)
            {
                AddModelError(result.ErrorMessage);
                var shoppingList = await _shoppingListService.GetByIdAsync(itemForm.ShoppingListId, GetCurrentUserId());
                if (shoppingList is null)
                {
                    return NotFound();
                }

                var detailsViewModel = await BuildDetailsViewModelAsync(shoppingList);
                detailsViewModel.NewItemForm = itemForm;
                await PopulateIngredientOptionsAsync(detailsViewModel.NewItemForm);
                return View(nameof(Details), detailsViewModel);
            }

            return RedirectToAction(nameof(Details), new { id = itemForm.ShoppingListId });
        }

        public async Task<IActionResult> EditItem(int id)
        {
            var shoppingLists = await _shoppingListService.GetAllAsync(GetCurrentUserId());
            var parentList = shoppingLists.FirstOrDefault(list => list.Items.Any(item => item.Id == id));
            var item = parentList?.Items.FirstOrDefault(listItem => listItem.Id == id);

            if (item is null || parentList is null)
            {
                return NotFound();
            }

            var model = new ShoppingListItemFormViewModel
            {
                Id = item.Id,
                ShoppingListId = item.ShoppingListId,
                IngredientId = item.IngredientId,
                CustomItemName = item.CustomItemName,
                Quantity = item.Quantity,
                Unit = item.Unit,
                IsChecked = item.IsChecked
            };
            await PopulateIngredientOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(int id, ShoppingListItemFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateIngredientOptionsAsync(model);
                return View(model);
            }

            var item = new ShoppingListItem
            {
                IngredientId = model.IngredientId,
                CustomItemName = model.CustomItemName,
                Quantity = model.Quantity,
                Unit = model.Unit,
                IsChecked = model.IsChecked
            };

            var result = await _shoppingListService.UpdateItemAsync(GetCurrentUserId(), id, item);
            if (!result.Success)
            {
                AddModelError(result.ErrorMessage);
                await PopulateIngredientOptionsAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = model.ShoppingListId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id, int shoppingListId)
        {
            var result = await _shoppingListService.DeleteItemAsync(GetCurrentUserId(), id);
            if (!result.Success && result.ErrorMessage == "Item-ul nu exista.")
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = shoppingListId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleItemChecked(int id, int shoppingListId)
        {
            var result = await _shoppingListService.ToggleItemCheckedAsync(GetCurrentUserId(), id);
            if (!result.Success && result.ErrorMessage == "Item-ul nu exista.")
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = shoppingListId });
        }

        private async Task<ShoppingListDetailsViewModel> BuildDetailsViewModelAsync(ShoppingList shoppingList)
        {
            var model = new ShoppingListDetailsViewModel
            {
                ShoppingList = new ShoppingListFormViewModel
                {
                    Id = shoppingList.Id,
                    Name = shoppingList.Name,
                    UserId = shoppingList.UserId
                },
                Items = shoppingList.Items
                    .OrderBy(item => item.IsChecked)
                    .ThenBy(item => item.Ingredient != null ? item.Ingredient.Name : item.CustomItemName)
                    .Select(item => new ShoppingListItemRowViewModel
                    {
                        Id = item.Id,
                        ShoppingListId = item.ShoppingListId,
                        IngredientId = item.IngredientId,
                        DisplayName = item.Ingredient?.Name ?? item.CustomItemName ?? "Item",
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        IsChecked = item.IsChecked
                    })
                    .ToList()
            };

            await PopulateIngredientOptionsAsync(model.NewItemForm);
            model.NewItemForm.ShoppingListId = shoppingList.Id;

            return model;
        }

        private async Task PopulateIngredientOptionsAsync(ShoppingListItemFormViewModel model)
        {
            var ingredients = await _ingredientService.GetAllAsync(GetCurrentUserId());
            model.AvailableIngredients = ingredients
                .OrderBy(ingredient => ingredient.Name)
                .Select(ingredient => new IngredientOptionViewModel
                {
                    Id = ingredient.Id,
                    Name = ingredient.Name
                })
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
