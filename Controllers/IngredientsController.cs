using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriPlan.Models;
using paw_np.Services.Interfaces;

namespace paw_np.Controllers
{
    [Authorize]
    public class IngredientsController : Controller
    {
        private readonly IIngredientService _ingredientService;

        public IngredientsController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            var ingredients = await _ingredientService.GetAllAsync(GetCurrentUserId());
            return View(ingredients);
        }

        // GET: Ingredients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _ingredientService.GetByIdAsync(id.Value, GetCurrentUserId());
            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // GET: Ingredients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ingredients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CaloriesPer100g,Proteins,Carbs,Fats,Sugars,Unit")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                var result = await _ingredientService.CreateAsync(GetCurrentUserId(), ingredient);
                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }

                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage);
                }
            }

            return View(ingredient);
        }

        // GET: Ingredients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _ingredientService.GetByIdAsync(id.Value, GetCurrentUserId());
            if (ingredient == null)
            {
                return NotFound();
            }
            return View(ingredient);
        }

        // POST: Ingredients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CaloriesPer100g,Proteins,Carbs,Fats,Sugars,Unit")] Ingredient ingredient)
        {
            if (id != ingredient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _ingredientService.UpdateAsync(id, GetCurrentUserId(), ingredient);
                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }

                if (result.ErrorMessage == "Ingredientul nu exista.")
                {
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage);
                }
            }
            return View(ingredient);
        }

        // GET: Ingredients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _ingredientService.GetByIdAsync(id.Value, GetCurrentUserId());
            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // POST: Ingredients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _ingredientService.DeleteAsync(id, GetCurrentUserId());
            if (!result.Success && result.ErrorMessage == "Ingredientul nu exista.")
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : 0;
        }
    }
}
