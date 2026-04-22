using NutriPlan.Models;

namespace paw_np.Repositories.Interfaces
{
    public interface IFoodJournalRepository : IRepositoryBase<FoodJournal>
    {
        Task<List<FoodJournal>> GetAllByUserAsync(int userId);
        Task<FoodJournal?> GetByIdWithEntriesAsync(int id, int userId);
        Task<FoodJournal?> GetByIdForUserAsync(int id, int userId);
        Task<FoodJournal?> GetByUserAndDateAsync(int userId, DateOnly journalDate);

        Task<JournalEntry?> GetEntryByIdAsync(int entryId);
        Task<JournalEntry> AddEntryAsync(JournalEntry entry);
        Task UpdateEntryAsync(JournalEntry entry);
        Task DeleteEntryAsync(JournalEntry entry);
    }
}
