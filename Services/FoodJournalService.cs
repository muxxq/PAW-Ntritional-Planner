using NutriPlan.Models;
using paw_np.Repositories.Interfaces;
using paw_np.Services.Interfaces;

namespace paw_np.Services
{
    public class FoodJournalService : IFoodJournalService
    {
        private readonly IFoodJournalRepository _foodJournalRepository;

        public FoodJournalService(IFoodJournalRepository foodJournalRepository)
        {
            _foodJournalRepository = foodJournalRepository;
        }

        public Task<List<FoodJournal>> GetAllAsync(int userId)
        {
            return _foodJournalRepository.GetAllByUserAsync(userId);
        }

        public Task<FoodJournal?> GetByIdAsync(int id, int userId)
        {
            return _foodJournalRepository.GetByIdWithEntriesAsync(id, userId);
        }

        public async Task<(bool Success, string? ErrorMessage, FoodJournal? FoodJournal)> CreateAsync(int userId, FoodJournal foodJournal)
        {
            foodJournal.UserId = userId;
            var validationError = ValidateJournal(foodJournal);
            if (validationError is not null)
            {
                return (false, validationError, null);
            }

            var existingForDate = await _foodJournalRepository.GetByUserAndDateAsync(foodJournal.UserId, foodJournal.JournalDate);
            if (existingForDate is not null)
            {
                return (false, "Exista deja un jurnal pentru aceasta zi.", null);
            }

            foodJournal.TotalKcalConsumed = Math.Max(0, foodJournal.TotalKcalConsumed);
            var createdJournal = await _foodJournalRepository.CreateAsync(foodJournal);
            return (true, null, createdJournal);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, int userId, FoodJournal foodJournal)
        {
            var existingJournal = await _foodJournalRepository.GetByIdWithEntriesAsync(id, userId);
            if (existingJournal is null)
            {
                return (false, "Jurnalul nu exista.");
            }

            foodJournal.UserId = userId;
            var validationError = ValidateJournal(foodJournal);
            if (validationError is not null)
            {
                return (false, validationError);
            }

            var duplicate = await _foodJournalRepository.GetByUserAndDateAsync(foodJournal.UserId, foodJournal.JournalDate);
            if (duplicate is not null && duplicate.Id != id)
            {
                return (false, "Exista deja un jurnal pentru aceasta zi.");
            }

            existingJournal.JournalDate = foodJournal.JournalDate;
            existingJournal.Notes = foodJournal.Notes?.Trim();
            existingJournal.UserId = foodJournal.UserId;
            existingJournal.TotalKcalConsumed = CalculateTotalKcal(existingJournal.JournalEntries);

            await _foodJournalRepository.UpdateAsync(existingJournal);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id, int userId)
        {
            var existingJournal = await _foodJournalRepository.GetByIdForUserAsync(id, userId);
            if (existingJournal is null)
            {
                return (false, "Jurnalul nu exista.");
            }

            await _foodJournalRepository.DeleteAsync(existingJournal);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage, JournalEntry? Entry)> AddEntryAsync(int userId, int journalId, JournalEntry entry)
        {
            var journal = await _foodJournalRepository.GetByIdWithEntriesAsync(journalId, userId);
            if (journal is null)
            {
                return (false, "Jurnalul nu exista.", null);
            }

            var validationError = ValidateEntry(entry);
            if (validationError is not null)
            {
                return (false, validationError, null);
            }

            entry.JournalId = journalId;
            entry.MealType = entry.MealType.Trim();
            entry.KcalConsumed = Math.Max(0, entry.KcalConsumed);
            if (entry.LoggedAt == default)
            {
                entry.LoggedAt = DateTime.UtcNow;
            }
            else
            {
                entry.LoggedAt = NormalizeToUtc(entry.LoggedAt);
            }

            var createdEntry = await _foodJournalRepository.AddEntryAsync(entry);

            // Keep denormalized total in sync with entries.
            journal.TotalKcalConsumed = CalculateTotalKcal(journal.JournalEntries) + createdEntry.KcalConsumed;
            await _foodJournalRepository.UpdateAsync(journal);

            return (true, null, createdEntry);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateEntryAsync(int userId, int entryId, JournalEntry entry)
        {
            var existingEntry = await _foodJournalRepository.GetEntryByIdAsync(entryId);
            if (existingEntry is null)
            {
                return (false, "Intrarea nu exista.");
            }

            var ownerJournal = await _foodJournalRepository.GetByIdForUserAsync(existingEntry.JournalId, userId);
            if (ownerJournal is null)
            {
                return (false, "Intrarea nu exista.");
            }

            var validationError = ValidateEntry(entry);
            if (validationError is not null)
            {
                return (false, validationError);
            }

            existingEntry.RecipeId = entry.RecipeId;
            existingEntry.IngredientId = entry.IngredientId;
            existingEntry.Quantity = entry.Quantity;
            existingEntry.MealType = entry.MealType.Trim();
            existingEntry.KcalConsumed = Math.Max(0, entry.KcalConsumed);
            if (entry.LoggedAt != default)
            {
                existingEntry.LoggedAt = NormalizeToUtc(entry.LoggedAt);
            }

            await _foodJournalRepository.UpdateEntryAsync(existingEntry);

            var journal = await _foodJournalRepository.GetByIdWithEntriesAsync(existingEntry.JournalId, userId);
            if (journal is not null)
            {
                journal.TotalKcalConsumed = CalculateTotalKcal(journal.JournalEntries);
                await _foodJournalRepository.UpdateAsync(journal);
            }

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteEntryAsync(int userId, int entryId)
        {
            var existingEntry = await _foodJournalRepository.GetEntryByIdAsync(entryId);
            if (existingEntry is null)
            {
                return (false, "Intrarea nu exista.");
            }

            var ownerJournal = await _foodJournalRepository.GetByIdForUserAsync(existingEntry.JournalId, userId);
            if (ownerJournal is null)
            {
                return (false, "Intrarea nu exista.");
            }

            await _foodJournalRepository.DeleteEntryAsync(existingEntry);

            var journal = await _foodJournalRepository.GetByIdWithEntriesAsync(existingEntry.JournalId, userId);
            if (journal is not null)
            {
                journal.TotalKcalConsumed = CalculateTotalKcal(journal.JournalEntries);
                await _foodJournalRepository.UpdateAsync(journal);
            }

            return (true, null);
        }

        private static string? ValidateJournal(FoodJournal journal)
        {
            if (journal.UserId <= 0)
            {
                return "Utilizator invalid.";
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            if (journal.JournalDate > today)
            {
                return "Data jurnalului nu poate fi in viitor.";
            }

            if (journal.TotalKcalConsumed < 0)
            {
                return "Totalul de calorii trebuie sa fie mai mare sau egal cu 0.";
            }

            return null;
        }

        private static string? ValidateEntry(JournalEntry entry)
        {
            if (entry.Quantity <= 0)
            {
                return "Cantitatea trebuie sa fie mai mare decat 0.";
            }

            if (string.IsNullOrWhiteSpace(entry.MealType))
            {
                return "Tipul mesei este obligatoriu.";
            }

            if (entry.KcalConsumed < 0)
            {
                return "Caloriile trebuie sa fie mai mari sau egale cu 0.";
            }

            var hasRecipe = entry.RecipeId.HasValue;
            var hasIngredient = entry.IngredientId.HasValue;
            if (hasRecipe == hasIngredient)
            {
                return "Intrarea trebuie sa aiba fie RecipeId, fie IngredientId.";
            }

            return null;
        }

        private static decimal CalculateTotalKcal(IEnumerable<JournalEntry> entries)
        {
            return entries.Sum(entry => Math.Max(0, entry.KcalConsumed));
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
            };
        }
    }
}
