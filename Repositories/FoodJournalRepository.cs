using Microsoft.EntityFrameworkCore;
using NutriPlan.Data;
using NutriPlan.Models;
using paw_np.Repositories.Interfaces;

namespace paw_np.Repositories
{
    public class FoodJournalRepository : IFoodJournalRepository
    {
        private readonly NutriPlanDbContext _context;

        public FoodJournalRepository(NutriPlanDbContext context)
        {
            _context = context;
        }

        public Task<List<FoodJournal>> GetAllAsync()
        {
            return _context.FoodJournals
                .Include(fj => fj.JournalEntries)
                    .ThenInclude(entry => entry.Recipe)
                .Include(fj => fj.JournalEntries)
                    .ThenInclude(entry => entry.Ingredient)
                .OrderByDescending(fj => fj.JournalDate)
                .ToListAsync();
        }

        public Task<List<FoodJournal>> GetAllByUserAsync(int userId)
        {
            return _context.FoodJournals
                .Include(fj => fj.JournalEntries)
                    .ThenInclude(entry => entry.Recipe)
                .Include(fj => fj.JournalEntries)
                    .ThenInclude(entry => entry.Ingredient)
                .Where(fj => fj.UserId == userId)
                .OrderByDescending(fj => fj.JournalDate)
                .ToListAsync();
        }

        public Task<FoodJournal?> GetByIdAsync(int id)
        {
            return _context.FoodJournals
                .FirstOrDefaultAsync(fj => fj.Id == id);
        }

        public Task<FoodJournal?> GetByIdForUserAsync(int id, int userId)
        {
            return _context.FoodJournals
                .FirstOrDefaultAsync(fj => fj.Id == id && fj.UserId == userId);
        }

        public Task<FoodJournal?> GetByIdWithEntriesAsync(int id, int userId)
        {
            return _context.FoodJournals
                .Include(fj => fj.JournalEntries)
                    .ThenInclude(entry => entry.Recipe)
                .Include(fj => fj.JournalEntries)
                    .ThenInclude(entry => entry.Ingredient)
                .FirstOrDefaultAsync(fj => fj.Id == id && fj.UserId == userId);
        }

        public Task<FoodJournal?> GetByUserAndDateAsync(int userId, DateOnly journalDate)
        {
            return _context.FoodJournals
                .Include(fj => fj.JournalEntries)
                    .ThenInclude(entry => entry.Recipe)
                .Include(fj => fj.JournalEntries)
                    .ThenInclude(entry => entry.Ingredient)
                .FirstOrDefaultAsync(fj => fj.UserId == userId && fj.JournalDate == journalDate);
        }

        public Task<bool> ExistsAsync(int id)
        {
            return _context.FoodJournals
                .AnyAsync(fj => fj.Id == id);
        }

        public async Task<FoodJournal> CreateAsync(FoodJournal entity)
        {
            _context.FoodJournals.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(FoodJournal entity)
        {
            _context.FoodJournals.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(FoodJournal entity)
        {
            _context.FoodJournals.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public Task<JournalEntry?> GetEntryByIdAsync(int entryId)
        {
            return _context.JournalEntries
                .Include(entry => entry.Recipe)
                .Include(entry => entry.Ingredient)
                .FirstOrDefaultAsync(entry => entry.Id == entryId);
        }

        public async Task<JournalEntry> AddEntryAsync(JournalEntry entry)
        {
            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task UpdateEntryAsync(JournalEntry entry)
        {
            _context.JournalEntries.Update(entry);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEntryAsync(JournalEntry entry)
        {
            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}
