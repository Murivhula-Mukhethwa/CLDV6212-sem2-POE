using ABC_retailers.Models;
using ABC_retailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_retailers.Controllers;

public class ProductController : Controller
{
    private readonly IFunctionsApi _functionsApi;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IFunctionsApi functionsApi, ILogger<ProductController> logger)
    {
        _functionsApi = functionsApi;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _functionsApi.GetProductsAsync();
        return View(products);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(product);

        try
        {
            var createdProduct = await _functionsApi.CreateProductAsync(product, imageFile);
            TempData["Success"] = $"Product '{createdProduct.ProductName}' created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError("", $"Error creating product: {ex.Message}");
            return View(product);
        }
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var product = await _functionsApi.GetProductAsync(id);
        if (product == null) return NotFound();

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(product);

        try
        {
            var updatedProduct = await _functionsApi.UpdateProductAsync(product.ProductId, product, imageFile);
            TempData["Success"] = $"Product '{updatedProduct.ProductName}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            ModelState.AddModelError("", $"Error updating product: {ex.Message}");
            return View(product);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _functionsApi.DeleteProductAsync(id);
            TempData["Success"] = "Product deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting product: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}
