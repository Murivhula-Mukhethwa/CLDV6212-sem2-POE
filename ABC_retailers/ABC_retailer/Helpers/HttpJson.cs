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

    public static HttpResponseData Ok<T>(HttpRequestData req, T body)
        => Write(req, HttpStatusCode.OK, body);

    public static HttpResponseData Created<T>(HttpRequestData req, T body)
        => Write(req, HttpStatusCode.Created, body);

    public static HttpResponseData Bad(HttpRequestData req, string message)
        => Text(req, HttpStatusCode.BadRequest, message);

    public static HttpResponseData NotFound(HttpRequestData req, string message = "Not Found")
        => Text(req, HttpStatusCode.NotFound, message);

    public static HttpResponseData NoContent(HttpRequestData req)
        => req.CreateResponse(HttpStatusCode.NoContent);

    public static HttpResponseData Text(HttpRequestData req, HttpStatusCode code, string message)
    {
        var r = req.CreateResponse(code);
        r.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        r.WriteString(message);   // ✅ no Encoding overload in isolated worker
        return r;
    }

    private static HttpResponseData Write<T>(HttpRequestData req, HttpStatusCode code, T body)
    {
        var r = req.CreateResponse(code);
        r.Headers.Add("Content-Type", "application/json; charset=utf-8");
        r.WriteString(JsonSerializer.Serialize(body, _json)); // ✅ use WriteString only
        return r;
    }
}
