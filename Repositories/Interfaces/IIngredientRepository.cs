using NutriPlan.Models;

namespace paw_np.Repositories.Interfaces
{
    public interface IIngredientRepository : IRepositoryBase<Ingredient>
    {
        Task<List<Ingredient>> GetAllByUserAsync(int userId);
        Task<Ingredient?> GetByIdForUserAsync(int id, int userId);
        Task<Ingredient?> GetByNameAsync(string name, int userId);
        Task<bool> ExistsByNameAsync(string name, int userId, int? excludeId = null);
    }
}
