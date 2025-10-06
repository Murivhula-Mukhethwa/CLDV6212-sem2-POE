using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ABC_retailers.Models;

namespace ABC_retailers.Services;

public class FunctionsApiClient : IFunctionsApi
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    // Centralize your Function routes
    private const string CustomersRoute = "customers";
    private const string ProductsRoute = "products";
    private const string OrdersRoute = "orders";
    private const string UploadsRoute = "uploads/proof-of-payment";

    public FunctionsApiClient(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("Functions");
    }

    // ---------- Helpers ----------
    private static HttpContent JsonBody(object obj)
        => new StringContent(JsonSerializer.Serialize(obj, _json), Encoding.UTF8, "application/json");

    private static async Task<T?> TryReadJsonAsync<T>(HttpResponseMessage resp)
    {
        if (!resp.IsSuccessStatusCode)
            return default;

        var stream = await resp.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream, _json);
    }

    // ---------- Customers ----------
    public async Task<List<Customer>?> GetCustomersAsync()
        => await TryReadJsonAsync<List<Customer>>(await _http.GetAsync(CustomersRoute));

    public async Task<Customer?> GetCustomerAsync(string id)
        => await TryReadJsonAsync<Customer>(await _http.GetAsync($"{CustomersRoute}/{id}"));

    public async Task<Customer?> CreateCustomerAsync(Customer c)
        => await TryReadJsonAsync<Customer>(await _http.PostAsync(CustomersRoute, JsonBody(new
        {
            name = c.Name,
            surname = c.Surname,
            username = c.Username,
            email = c.Email,
            shippingAddress = c.ShippingAddress
        })));

    public async Task<Customer?> UpdateCustomerAsync(string id, Customer c)
        => await TryReadJsonAsync<Customer>(await _http.PutAsync($"{CustomersRoute}/{id}", JsonBody(new
        {
            name = c.Name,
            surname = c.Surname,
            username = c.Username,
            email = c.Email,
            shippingAddress = c.ShippingAddress
        })));

    public async Task<bool> DeleteCustomerAsync(string id)
    {
        var resp = await _http.DeleteAsync($"{CustomersRoute}/{id}");
        return resp.IsSuccessStatusCode;
    }

    // ---------- Products ----------
    public async Task<List<Product>?> GetProductsAsync()
        => await TryReadJsonAsync<List<Product>>(await _http.GetAsync(ProductsRoute));

    public async Task<Product?> GetProductAsync(string id)
        => await TryReadJsonAsync<Product>(await _http.GetAsync($"{ProductsRoute}/{id}"));

    public async Task<Product?> CreateProductAsync(Product p, IFormFile? imageFile)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(p.ProductName), "ProductName");
        form.Add(new StringContent(p.Description ?? string.Empty), "Description");
        form.Add(new StringContent(p.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
        form.Add(new StringContent(p.StockAvailable.ToString(System.Globalization.CultureInfo.InvariantCulture)), "StockAvailable");
        if (!string.IsNullOrWhiteSpace(p.ImageUrl)) form.Add(new StringContent(p.ImageUrl), "ImageUrl");

        if (imageFile is not null && imageFile.Length > 0)
        {
            var file = new StreamContent(imageFile.OpenReadStream());
            file.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType ?? "application/octet-stream");
            form.Add(file, "ImageFile", imageFile.FileName);
        }

        return await TryReadJsonAsync<Product>(await _http.PostAsync(ProductsRoute, form));
    }

    public async Task<Product?> UpdateProductAsync(string id, Product p, IFormFile? imageFile)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(p.ProductName), "ProductName");
        form.Add(new StringContent(p.Description ?? string.Empty), "Description");
        form.Add(new StringContent(p.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
        form.Add(new StringContent(p.StockAvailable.ToString(System.Globalization.CultureInfo.InvariantCulture)), "StockAvailable");
        if (!string.IsNullOrWhiteSpace(p.ImageUrl)) form.Add(new StringContent(p.ImageUrl), "ImageUrl");

        if (imageFile is not null && imageFile.Length > 0)
        {
            var file = new StreamContent(imageFile.OpenReadStream());
            file.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType ?? "application/octet-stream");
            form.Add(file, "ImageFile", imageFile.FileName);
        }

        return await TryReadJsonAsync<Product>(await _http.PutAsync($"{ProductsRoute}/{id}", form));
    }

    public async Task<bool> DeleteProductAsync(string id)
    {
        var resp = await _http.DeleteAsync($"{ProductsRoute}/{id}");
        return resp.IsSuccessStatusCode;
    }

    // ---------- Orders ----------
    public async Task<List<Order>?> GetOrdersAsync()
    {
        var dtos = await TryReadJsonAsync<List<OrderDto>>(await _http.GetAsync(OrdersRoute));
        return dtos?.Select(ToOrder).ToList();
    }

    public async Task<Order?> GetOrderAsync(string id)
    {
        var dto = await TryReadJsonAsync<OrderDto>(await _http.GetAsync($"{OrdersRoute}/{id}"));
        return dto is null ? null : ToOrder(dto);
    }

    public async Task<Order?> CreateOrderAsync(string customerId, string productId, int quantity)
    {
        var payload = new { customerId, productId, quantity };
        var dto = await TryReadJsonAsync<OrderDto>(await _http.PostAsync(OrdersRoute, JsonBody(payload)));
        return dto is null ? null : ToOrder(dto);
    }

    public async Task<Order?> UpdateOrderStatusAsync(string id, string newStatus)
    {
        var payload = new { status = newStatus };
        var dto = await TryReadJsonAsync<OrderDto>(
            await _http.PatchAsync($"{OrdersRoute}/{id}/status", JsonBody(payload))
        );
        return dto is null ? null : ToOrder(dto);
    }

    public async Task<bool> DeleteOrderAsync(string id)
    {
        var resp = await _http.DeleteAsync($"{OrdersRoute}/{id}");
        return resp.IsSuccessStatusCode;
    }

    // ---------- Uploads ----------
    public async Task<string?> UploadProofOfPaymentAsync(IFormFile file, string? orderId, string? customerName)
    {
        using var form = new MultipartFormDataContent();
        var sc = new StreamContent(file.OpenReadStream());
        sc.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
        form.Add(sc, "ProofOfPayment", file.FileName);

        if (!string.IsNullOrWhiteSpace(orderId)) form.Add(new StringContent(orderId), "OrderId");
        if (!string.IsNullOrWhiteSpace(customerName)) form.Add(new StringContent(customerName), "CustomerName");

        var resp = await _http.PostAsync(UploadsRoute, form);
        if (!resp.IsSuccessStatusCode) return null;

        var doc = await TryReadJsonAsync<Dictionary<string, string>>(resp);
        return doc is not null && doc.TryGetValue("fileName", out var name) ? name : file.FileName;
    }

    // ---------- Mapping ----------
    private static Order ToOrder(OrderDto d)
    {
        var status = Enum.TryParse<OrderStatus>(d.Status, ignoreCase: true, out var s)
            ? s : OrderStatus.Submitted;

        return new Order
        {
            Id = d.Id,
            CustomerId = d.CustomerId,
            ProductId = d.ProductId,
            ProductName = d.ProductName,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            OrderDateUtc = d.OrderDateUtc,
            Status = status
        };
    }

    private sealed record OrderDto(
        string Id,
        string CustomerId,
        string ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        DateTimeOffset OrderDateUtc,
        string Status);
}

// Minimal PATCH extension for HttpClient
internal static class HttpClientPatchExtensions
{
    public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        => client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, requestUri) { Content = content });
}
