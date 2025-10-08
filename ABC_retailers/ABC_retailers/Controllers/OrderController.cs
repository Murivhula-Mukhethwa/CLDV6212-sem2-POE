using ABC_retailers.Models;
using ABC_retailers.Models.ViewModel;
using ABC_retailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_retailers.Controllers;

public class OrderController : Controller
{
    private readonly IFunctionsApi _functionsApi;

    public OrderController(IFunctionsApi functionsApi)
    {
        _functionsApi = functionsApi;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _functionsApi.GetOrdersAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var order = await _functionsApi.GetOrderAsync(id);
        if (order == null) return NotFound();

        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        var customers = await _functionsApi.GetCustomersAsync();
        var products = await _functionsApi.GetProductsAsync();

        var model = new OrderCreateViewModel
        {
            Customers = customers,
            Products = products
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var order = await _functionsApi.CreateOrderAsync(model.CustomerId, model.ProductId, model.Quantity);
            TempData["Success"] = "Order created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error creating order: {ex.Message}");
            model.Customers = await _functionsApi.GetCustomersAsync();
            model.Products = await _functionsApi.GetProductsAsync();
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(string id, string newStatus)
    {
        try
        {
            await _functionsApi.UpdateOrderStatusAsync(id, newStatus);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        await _functionsApi.DeleteOrderAsync(id);
        TempData["Success"] = "Order deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
