using NutriPlan.Models;

namespace paw_np.Services.Interfaces
{
    public interface IFoodJournalService
    {
        Task<List<FoodJournal>> GetAllAsync(int userId);
        Task<FoodJournal?> GetByIdAsync(int id, int userId);
        Task<(bool Success, string? ErrorMessage, FoodJournal? FoodJournal)> CreateAsync(int userId, FoodJournal foodJournal);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, FoodJournal foodJournal);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId);

        Task<(bool Success, string? ErrorMessage, JournalEntry? Entry)> AddEntryAsync(int userId, int journalId, JournalEntry entry);
        Task<(bool Success, string? ErrorMessage)> UpdateEntryAsync(int userId, int entryId, JournalEntry entry);
        Task<(bool Success, string? ErrorMessage)> DeleteEntryAsync(int userId, int entryId);
    }
}
