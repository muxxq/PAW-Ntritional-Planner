using Microsoft.EntityFrameworkCore;
using NutriPlan.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using paw_np.Repositories;
using paw_np.Repositories.Interfaces;
using paw_np.Services;
using paw_np.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NutriPlanDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var mvc = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
{
    mvc.AddRazorRuntimeCompilation();
}
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/Login";
    });

// Repositories
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
builder.Services.AddScoped<IFoodJournalRepository, FoodJournalRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IMealPlannerRepository, MealPlannerRepository>();

// Services
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();
builder.Services.AddScoped<IFoodJournalService, FoodJournalService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IMealPlannerService, MealPlannerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "ingredients",
    pattern: "ingredients/{action=Index}/{id?}",
    defaults: new { controller = "Ingredients" });

app.MapControllerRoute(
    name: "shopping-list",
    pattern: "shopping-list/{action=Index}/{id?}",
    defaults: new { controller = "ShoppingList" });

app.MapControllerRoute(
    name: "meals",
    pattern: "meals/{action=Index}/{id?}",
    defaults: new { controller = "FoodJournal" });

app.MapControllerRoute(
    name: "planner",
    pattern: "planner/{action=Index}/{id?}",
    defaults: new { controller = "MealPlanner" });

app.MapControllerRoute(
    name: "recipes",
    pattern: "recipes/{action=Index}/{id?}",
    defaults: new { controller = "Recipes" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();