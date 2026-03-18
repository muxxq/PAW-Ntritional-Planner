using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriPlan.Data;
using NutriPlan.Models;

namespace NutriPlan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly NutriPlanDbContext _context;

        public IngredientsController(NutriPlanDbContext context)
        {
            _context = context;
        }

        // ── GET /api/ingredients ─────────────────────────────────────────────
        // Returneaza toate ingredientele (cu optiune de cautare dupa nume)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ingredient>>> GetAll([FromQuery] string? search)
        {
            var query = _context.Ingredients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(i => i.Name.Contains(search));

            return await query.OrderBy(i => i.Name).ToListAsync();
        }

        // ── GET /api/ingredients/{id} ────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<ActionResult<Ingredient>> GetById(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);

            if (ingredient == null)
                return NotFound(new { message = $"Ingredientul cu id={id} nu a fost gasit." });

            return ingredient;
        }

        // ── POST /api/ingredients ────────────────────────────────────────────
        // Adauga un ingredient nou in repository
        [HttpPost]
        public async Task<ActionResult<Ingredient>> Create([FromBody] Ingredient ingredient)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificam daca exista deja un ingredient cu acelasi nume
            bool exists = await _context.Ingredients.AnyAsync(i => i.Name == ingredient.Name);
            if (exists)
                return Conflict(new { message = $"Ingredientul '{ingredient.Name}' exista deja." });

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ingredient.Id }, ingredient);
        }

        // ── PUT /api/ingredients/{id} ────────────────────────────────────────
        // Actualizeaza un ingredient existent
        [HttpPut("{id}")]
        public async Task<ActionResult<Ingredient>> Update(int id, [FromBody] Ingredient updated)
        {
            if (id != updated.Id)
                return BadRequest(new { message = "Id-ul din URL nu corespunde cu cel din body." });

            var existing = await _context.Ingredients.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = $"Ingredientul cu id={id} nu a fost gasit." });

            // Actualizam campurile
            existing.Name           = updated.Name;
            existing.CaloriesPer100g = updated.CaloriesPer100g;
            existing.Proteins        = updated.Proteins;
            existing.Carbs           = updated.Carbs;
            existing.Fats            = updated.Fats;
            existing.Sugars          = updated.Sugars;
            existing.Unit            = updated.Unit;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // ── DELETE /api/ingredients/{id} ─────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
                return NotFound(new { message = $"Ingredientul cu id={id} nu a fost gasit." });

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ── GET /api/ingredients/test-connection ─────────────────────────────
        // Endpoint de test pentru a verifica daca conexiunea cu DB functioneaza
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool canConnect = await _context.Database.CanConnectAsync();
                int count = await _context.Ingredients.CountAsync();
                return Ok(new
                {
                    status = "Conexiunea cu baza de date functioneaza!",
                    canConnect,
                    ingredientsCount = count,
                    dbProvider = _context.Database.ProviderName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Eroare la conectare", error = ex.Message });
            }
        }
    }
}
