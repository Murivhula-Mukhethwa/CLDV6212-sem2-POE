using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABC_retailerfunction.Helpers;

public static class HttpJson
{
    static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public static async Task<T?> ReadAsync<T>(HttpRequestData req)
    {
        using var s = req.Body;
        return await JsonSerializer.DeserializeAsync<T>(s, _json);
    }

    // ✅ Public wrappers — functions can still call these without change
    public static HttpResponseData Ok<T>(HttpRequestData req, T body)
        => WriteSync(req, HttpStatusCode.OK, body);

    public static HttpResponseData Created<T>(HttpRequestData req, T body)
        => WriteSync(req, HttpStatusCode.Created, body);

    public static HttpResponseData Bad(HttpRequestData req, string message)
        => TextSync(req, HttpStatusCode.BadRequest, message);

    public static HttpResponseData NotFound(HttpRequestData req, string message = "Not Found")
        => TextSync(req, HttpStatusCode.NotFound, message);

    public static HttpResponseData NoContent(HttpRequestData req)
        => req.CreateResponse(HttpStatusCode.NoContent);

    // ==========================================================
    // 🧠 Internally handles async-safe writing without changing callers
    // ==========================================================

    private static HttpResponseData TextSync(HttpRequestData req, HttpStatusCode code, string message)
    {
        var r = req.CreateResponse(code);
        r.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        // ✅ Use WriteStringAsync but block safely to avoid async violations
        var task = r.WriteStringAsync(message);
        task.ConfigureAwait(false).GetAwaiter().GetResult();

        return r;
    }

    private static HttpResponseData WriteSync<T>(HttpRequestData req, HttpStatusCode code, T body)
    {
        var r = req.CreateResponse(code);
        r.Headers.Add("Content-Type", "application/json; charset=utf-8");

        // ✅ Serialize first
        var json = JsonSerializer.Serialize(body, _json);

        // ✅ Async-safe write, even in non-async context
        var task = r.WriteStringAsync(json);
        task.ConfigureAwait(false).GetAwaiter().GetResult();

        return r;
    }
}