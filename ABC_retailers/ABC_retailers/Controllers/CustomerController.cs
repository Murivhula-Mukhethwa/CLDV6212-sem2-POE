using ABC_retailers.Models;
using ABC_retailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_retailers.Controllers;

public class CustomerController : Controller
{
    private readonly IFunctionsApi _functionsApi;

    public CustomerController(IFunctionsApi functionsApi)
    {
        _functionsApi = functionsApi;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _functionsApi.GetCustomersAsync();
        return View(customers);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);

        await _functionsApi.CreateCustomerAsync(customer);
        TempData["Success"] = "Customer created successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var customer = await _functionsApi.GetCustomerAsync(id);
        if (customer == null) return NotFound();

        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);

        await _functionsApi.UpdateCustomerAsync(customer.CustomerId, customer);
        TempData["Success"] = "Customer updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        await _functionsApi.DeleteCustomerAsync(id);
        TempData["Success"] = "Customer deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
