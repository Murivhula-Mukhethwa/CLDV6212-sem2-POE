using ABC_retailers.Models;

namespace ABC_retailers.Services;

public interface IFunctionsApi
{
    // Customers
    Task<List<Customer>?> GetCustomersAsync();
    Task<Customer?> GetCustomerAsync(string id);
    Task<Customer?> CreateCustomerAsync(Customer c);
    Task<Customer?> UpdateCustomerAsync(string id, Customer c);
    Task<bool> DeleteCustomerAsync(string id);

    // Products
    Task<List<Product>?> GetProductsAsync();
    Task<Product?> GetProductAsync(string id);
    Task<Product?> CreateProductAsync(Product p, IFormFile? imageFile);
    Task<Product?> UpdateProductAsync(string id, Product p, IFormFile? imageFile);
    Task<bool> DeleteProductAsync(string id);

    // Orders
    Task<List<Order>?> GetOrdersAsync();
    Task<Order?> GetOrderAsync(string id);
    Task<Order?> CreateOrderAsync(string customerId, string productId, int quantity);
    Task<Order?> UpdateOrderStatusAsync(string id, string newStatus);
    Task<bool> DeleteOrderAsync(string id);

    // Uploads
    Task<string?> UploadProofOfPaymentAsync(IFormFile file, string? orderId, string? customerName);
}
