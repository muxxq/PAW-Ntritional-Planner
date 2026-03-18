using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using paw_np.Models;

namespace paw_np.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        return View("dashboard");
    }

    public IActionResult Meals()
    {
        return View("meals");
    }

    public IActionResult Planner()
    {
        return View("planner");
    }

    public IActionResult Recipes()
    {
        return View("recipes");
    }

    public IActionResult Profile()
    {
        return View("profile");
    }

    public IActionResult Login()
    {
        return View("login");
    }

    public IActionResult Register()
    {
        return View("register");
    }

    public IActionResult ShoppingList()
    {
        return View("ShoppingList");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
