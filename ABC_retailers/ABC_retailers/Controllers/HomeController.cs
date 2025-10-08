using System.Diagnostics;
using ABC_retailers.Models;
using ABC_retailers.Models.ViewModel;
using ABC_retailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_retailers.Controllers;

public class HomeController : Controller
{
    private readonly IFunctionsApi _functionsApi;

    public HomeController(IFunctionsApi functionsApi)
    {
        _functionsApi = functionsApi;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _functionsApi.GetCustomersAsync();
        var products = await _functionsApi.GetProductsAsync();
        var orders = await _functionsApi.GetOrdersAsync();

        var viewModel = new HomeViewModel
        {
            CustomerCount = customers.Count,
            ProductCount = products.Count,
            OrderCount = orders.Count
        };

        return View(viewModel);
    }

    public IActionResult Privacy() => View();

    [HttpPost]
    public async Task<IActionResult> InitializeStorage()
    {
        try
        {
            await _functionsApi.GetCustomersAsync(); // just call a method to initialize
            TempData["Success"] = "Azure Functions / Storage initialized successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to initialize storage: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
