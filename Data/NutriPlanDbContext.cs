using Microsoft.EntityFrameworkCore;
using NutriPlan.Models;

namespace NutriPlan.Data
{
    public class NutriPlanDbContext : DbContext
    {
        public NutriPlanDbContext(DbContextOptions<NutriPlanDbContext> options)
            : base(options) { }

        //tabele
        public DbSet<User> Users { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<FoodJournal> FoodJournals { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<MealPlanner> MealPlanners { get; set; }
        public DbSet<PlannerItem> PlannerItems { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });

            //Ingredients
            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasIndex(i => i.Name).IsUnique();
            });

            //Recipes
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Recipes)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //RecipeIngredients
            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                entity.HasOne(ri => ri.Recipe)
                      .WithMany(r => r.RecipeIngredients)
                      .HasForeignKey(ri => ri.RecipeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ri => ri.Ingredient)
                      .WithMany(i => i.RecipeIngredients)
                      .HasForeignKey(ri => ri.IngredientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //FoodJournal
            modelBuilder.Entity<FoodJournal>(entity =>
            {
                entity.HasOne(fj => fj.User)
                      .WithMany(u => u.FoodJournals)
                      .HasForeignKey(fj => fj.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Un singur jurnal per zi per utilizator
                entity.HasIndex(fj => new { fj.UserId, fj.JournalDate }).IsUnique();
            });

            //JournalEntries
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.HasOne(je => je.FoodJournal)
                      .WithMany(fj => fj.JournalEntries)
                      .HasForeignKey(je => je.JournalId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(je => je.Recipe)
                      .WithMany(r => r.JournalEntries)
                      .HasForeignKey(je => je.RecipeId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(je => je.Ingredient)
                      .WithMany(i => i.JournalEntries)
                      .HasForeignKey(je => je.IngredientId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            //MealPlanner
            modelBuilder.Entity<MealPlanner>(entity =>
            {
                entity.HasOne(mp => mp.User)
                      .WithMany(u => u.MealPlanners)
                      .HasForeignKey(mp => mp.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Un singur slot per zi + tip de masa per utilizator
                entity.HasIndex(mp => new { mp.UserId, mp.PlanDate, mp.MealType }).IsUnique();
            });

            //PlannerItems
            modelBuilder.Entity<PlannerItem>(entity =>
            {
                entity.HasOne(pi => pi.MealPlanner)
                      .WithMany(mp => mp.PlannerItems)
                      .HasForeignKey(pi => pi.PlannerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pi => pi.Recipe)
                      .WithMany(r => r.PlannerItems)
                      .HasForeignKey(pi => pi.RecipeId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(pi => pi.Ingredient)
                      .WithMany(i => i.PlannerItems)
                      .HasForeignKey(pi => pi.IngredientId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            //ShoppingList
            modelBuilder.Entity<ShoppingList>(entity =>
            {
                entity.HasOne(sl => sl.User)
                      .WithMany(u => u.ShoppingLists)
                      .HasForeignKey(sl => sl.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //ShoppingListItems
            modelBuilder.Entity<ShoppingListItem>(entity =>
            {
                entity.HasOne(sli => sli.ShoppingList)
                      .WithMany(sl => sl.Items)
                      .HasForeignKey(sli => sli.ShoppingListId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sli => sli.Ingredient)
                      .WithMany(i => i.ShoppingListItems)
                      .HasForeignKey(sli => sli.IngredientId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            //Seed data
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { Id = 1, Name = "Ou",         CaloriesPer100g = 155, Proteins = 13, Carbs = 1.1m,  Fats = 11,  Sugars = 1.1m, Unit = "buc" },
                new Ingredient { Id = 2, Name = "Piept pui",  CaloriesPer100g = 165, Proteins = 31, Carbs = 0,     Fats = 3.6m,Sugars = 0,    Unit = "g"   },
                new Ingredient { Id = 3, Name = "Orez alb",   CaloriesPer100g = 130, Proteins = 2.7m, Carbs = 28,  Fats = 0.3m,Sugars = 0,    Unit = "g"   },
                new Ingredient { Id = 4, Name = "Lapte 3.5%", CaloriesPer100g = 61,  Proteins = 3.2m, Carbs = 4.8m,Fats = 3.5m,Sugars = 4.8m, Unit = "ml"  },
                new Ingredient { Id = 5, Name = "Banana",     CaloriesPer100g = 89,  Proteins = 1.1m, Carbs = 23,  Fats = 0.3m,Sugars = 12,   Unit = "buc" }
            );
        }
    }
}
