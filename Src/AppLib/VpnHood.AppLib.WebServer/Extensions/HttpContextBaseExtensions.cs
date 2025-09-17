using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;
using WatsonWebserver.Core;

namespace VpnHood.AppLib.WebServer.Extensions;

internal static class HttpContextBaseExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static T? GetQueryParameter<T>(this HttpContextBase ctx, string key, T? defaultValue)
    {
        return ctx.Request.QuerystringExists(key)
            ? ctx.GetQueryParameter<T>(key)
            : defaultValue;
    }

    public static T GetQueryParameter<T>(this HttpContextBase ctx, string key)
    {
        if (!ctx.Request.QuerystringExists(key))
            throw new ArgumentException($"Route parameter '{key}' is required.");

        var value = ctx.Request.RetrieveQueryValue(key);
        try {
            return ConvertString<T>(value) ?? 
                   throw new Exception($"The value of {key} should not be null.");
        }
        catch (Exception ex) {
            throw new ArgumentException($"Cannot convert '{value}' to {typeof(T)} for parameter '{key}'", ex);
        }
    }

    public static T? GetRouteParameter<T>(this HttpContextBase ctx, string key, T? defaultValue)
    {
        var value = ctx.Request.Url.Parameters.Get(key);
        return value is null 
            ? defaultValue 
            : ctx.GetRouteParameter<T>(key);
    }

    public static T GetRouteParameter<T>(this HttpContextBase ctx, string key)
    {
        var value = ctx.Request.Url.Parameters.Get(key);
        if (value is null)
            throw new ArgumentException($"Route parameter '{key}' is required.");

        try {
            return ConvertString<T>(value) ??
                   throw new Exception($"The value of {key} should not be null.");
        }
        catch (Exception ex) {
            throw new ArgumentException($"Cannot convert '{value}' to {typeof(T)} for parameter '{key}'", ex);
        }
    }

    private static T? ConvertString<T>(string value)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (underlyingType == typeof(Guid))
            return (T)(object)Guid.Parse(value);

        if (underlyingType.IsEnum)
            return (T)Enum.Parse(underlyingType, value, true);

        if (underlyingType == typeof(bool))
            return (T)(object)bool.Parse(value);

        if (underlyingType == typeof(DateTime))
            return (T)(object)DateTime.Parse(value);

        if (underlyingType == typeof(DateTimeOffset))
            return (T)(object)DateTimeOffset.Parse(value);

        if (underlyingType == typeof(TimeSpan))
            return (T)(object)TimeSpan.Parse(value);

        // Use TypeConverter for complex conversions
        var converter = TypeDescriptor.GetConverter(underlyingType);
        if (converter.CanConvertFrom(typeof(string)))
            return (T?)converter.ConvertFromString(value);

        return (T)Convert.ChangeType(value, underlyingType);
    }

    public static async Task SendNoContent(this HttpContextBase ctx)
    {
        ctx.Response.StatusCode = (int)HttpStatusCode.NoContent;
        await ctx.Response.Send();
    }

    public static Task SendNotFound(HttpContextBase context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        return context.Response.Send();
    }

    public static async Task SendJson(this HttpContextBase ctx, object? data, int statusCode = 200)
    {
        if (data is null) {
            ctx.Response.StatusCode = (int)HttpStatusCode.NoContent;
            await ctx.Response.Send();
            return;
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.Send(json);
    }

    public static T ReadJson<T>(this HttpContextBase ctx)
    {
        try {
            var bytes = ctx.Request.DataAsBytes;
            if (bytes == null || bytes.Length == 0)
                throw new InvalidOperationException($"Request body is empty for {typeof(T).Name}");

            // Determine encoding from Content-Type if available
            var encoding = Encoding.UTF8;
            var ct = ctx.Request.ContentType;
            if (!string.IsNullOrWhiteSpace(ct))
                try {
                    var parts = ct.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    var charsetPart = parts.FirstOrDefault(p => p.StartsWith("charset=", StringComparison.OrdinalIgnoreCase));
                    if (charsetPart != null) {
                        var charset = charsetPart["charset=".Length..];
                        encoding = Encoding.GetEncoding(charset);
                    }
                }
                catch {
                    // ignore invalid charset and default to UTF-8
                }

            var body = encoding.GetString(bytes);

            var obj = JsonSerializer.Deserialize<T>(body, JsonOptions) ??
                      throw new InvalidOperationException($"Failed to deserialize JSON to {typeof(T).Name}. Body: {body}");

            return obj;
        }
        catch (JsonException ex) {
            throw new InvalidOperationException($"Invalid JSON for {typeof(T).Name}: {ex.Message}");
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"Error reading JSON for {typeof(T).Name}: {ex.Message}", ex);
        }
    }
}
