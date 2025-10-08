using ABC_retailers.Models;

namespace ABC_retailers.Services;

public class FunctionsApi : IFunctionsApi
{
    private readonly List<Customer> _customers = new();
    private readonly List<Product> _products = new();
    private readonly List<Order> _orders = new();

    // ---------- Customers ----------
    public Task<List<Customer>> GetCustomersAsync() => Task.FromResult(_customers);

    public Task<Customer?> GetCustomerAsync(string id) =>
        Task.FromResult(_customers.FirstOrDefault(c => c.CustomerId == id));

    public Task<Customer> CreateCustomerAsync(Customer c)
    {
        _customers.Add(c);
        return Task.FromResult(c);
    }

    public Task<Customer> UpdateCustomerAsync(string id, Customer c)
    {
        var existing = _customers.FirstOrDefault(x => x.CustomerId == id);
        if (existing != null)
        {
            existing.Name = c.Name;
            existing.Surname = c.Surname;
            existing.Username = c.Username;
            existing.Email = c.Email;
            existing.ShippingAddress = c.ShippingAddress;
        }
        return Task.FromResult(existing!);
    }

    public Task DeleteCustomerAsync(string id)
    {
        _customers.RemoveAll(c => c.CustomerId == id);
        return Task.CompletedTask;
    }

    // ---------- Products ----------
    public Task<List<Product>> GetProductsAsync() => Task.FromResult(_products);

    public Task<Product?> GetProductAsync(string id) =>
        Task.FromResult(_products.FirstOrDefault(p => p.ProductId == id));

    public Task<Product> CreateProductAsync(Product p, IFormFile? imageFile)
    {
        if (imageFile != null) p.ImageUrl = imageFile.FileName;
        _products.Add(p);
        return Task.FromResult(p);
    }

    public Task<Product> UpdateProductAsync(string id, Product p, IFormFile? imageFile)
    {
        var existing = _products.FirstOrDefault(x => x.ProductId == id);
        if (existing != null)
        {
            existing.ProductName = p.ProductName;
            existing.Description = p.Description;
            existing.Price = p.Price;
            existing.StockAvailable = p.StockAvailable;
            if (imageFile != null) existing.ImageUrl = imageFile.FileName;
        }
        return Task.FromResult(existing!);
    }

    public Task DeleteProductAsync(string id)
    {
        _products.RemoveAll(p => p.ProductId == id);
        return Task.CompletedTask;
    }

    // ---------- Orders ----------
    public Task<List<Order>> GetOrdersAsync() => Task.FromResult(_orders);

    public Task<Order?> GetOrderAsync(string id) =>
        Task.FromResult(_orders.FirstOrDefault(o => o.OrderId == id));

    public Task<Order> CreateOrderAsync(string customerId, string productId, int quantity)
    {
        var customer = _customers.FirstOrDefault(c => c.CustomerId == customerId);
        var product = _products.FirstOrDefault(p => p.ProductId == productId);

        if (customer == null || product == null)
            throw new Exception("Invalid customer or product");

        var order = new Order
        {
            RowKey = Guid.NewGuid().ToString(),
            CustomerId = customerId,
            ProductId = productId,
            ProductName = product.ProductName,
            Quantity = quantity,
            UnitPrice = product.Price,
            TotalPrice = quantity * product.Price,
            Status = "Submitted",
            OrderDate = DateTime.UtcNow
        };

        _orders.Add(order);
        return Task.FromResult(order);
    }

    public Task UpdateOrderStatusAsync(string id, string newStatus)
    {
        var order = _orders.FirstOrDefault(o => o.OrderId == id);
        if (order != null)
        {
            order.Status = newStatus;
        }
        return Task.CompletedTask;
    }

    public Task DeleteOrderAsync(string id)
    {
        _orders.RemoveAll(o => o.OrderId == id);
        return Task.CompletedTask;
    }

    // ---------- Uploads ----------
    public Task<string> UploadProofOfPaymentAsync(IFormFile file, string? orderId = null, string? customerName = null)
    {
        // just return the filename for simplicity
        return Task.FromResult(file.FileName);
    }
}
