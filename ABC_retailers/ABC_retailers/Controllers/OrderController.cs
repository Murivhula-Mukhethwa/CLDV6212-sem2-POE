using ABC_retailers.Models;
using ABC_retailers.Models.ViewModel;
using ABC_retailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class OrderController : Controller
    {
        private readonly IFunctionsApi _api;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IFunctionsApi api, ILogger<OrderController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _api.GetOrdersAsync();
                return View(orders.OrderByDescending(o => o.OrderDateUtc).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                TempData["Error"] = "Could not load orders.";
                return View(new List<Order>());
            }
        }

        // CREATE (GET)
        public async Task<IActionResult> Create()
        {
            var vm = new OrderCreateViewModel
            {
                Customers = await _api.GetCustomersAsync(),
                Products = await _api.GetProductsAsync()
            };
            return View(vm);
        }

        // CREATE (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var customer = await _api.GetCustomerAsync(model.CustomerId);
                var product = await _api.GetProductAsync(model.ProductId);

                if (customer is null || product is null)
                {
                    ModelState.AddModelError("", "Invalid customer or product.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                if (product.StockAvailable < model.Quantity)
                {
                    ModelState.AddModelError("Quantity", $"Only {product.StockAvailable} in stock.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                var saved = await _api.CreateOrderAsync(model.CustomerId, model.ProductId, model.Quantity);
                if (saved == null)
                {
                    ModelState.AddModelError("", "Could not create order.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                TempData["Success"] = "Order created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // DETAILS
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var order = await _api.GetOrderAsync(id);
            return order is null ? NotFound() : View(order);
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var order = await _api.GetOrderAsync(id);
            return order is null ? NotFound() : View(order);
        }

        // EDIT (POST) - update status only
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order posted)
        {
            if (!ModelState.IsValid) return View(posted);

            try
            {
                await _api.UpdateOrderStatusAsync(posted.Id, posted.Status.ToString());
                TempData["Success"] = "Order updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order");
                ModelState.AddModelError("", $"Error updating order: {ex.Message}");
                return View(posted);
            }
        }

        // DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _api.DeleteOrderAsync(id);
                TempData["Success"] = "Order deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order");
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX: product lookup
        [HttpGet]
        public async Task<JsonResult> GetProductPrice(string productId)
        {
            try
            {
                var product = await _api.GetProductAsync(productId);
                if (product != null)
                {
                    return Json(new
                    {
                        success = true,
                        price = product.Price,
                        stock = product.StockAvailable,
                        productName = product.ProductName
                    });
                }
                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product price");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: status update
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(string id, string newStatus)
        {
            try
            {
                await _api.UpdateOrderStatusAsync(id, newStatus);
                return Json(new { success = true, message = $"Order status updated to {newStatus}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task PopulateDropdowns(OrderCreateViewModel model)
        {
            model.Customers = await _api.GetCustomersAsync();
            model.Products = await _api.GetProductsAsync();
        }
    }
}
