using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userName = context.User.Identity?.Name;
        var requestBody = await GetRequestBody(context.Request);

        _logger.LogInformation($"IP: {ipAddress}, UserName: {userName}, RequestBody: {requestBody}, TimeStamp: {DateTime.Now}");

        await _next(context);
    }

    private async Task<string> GetRequestBody(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var requestBody = await reader.ReadToEndAsync();

        request.Body.Seek(0, SeekOrigin.Begin); // Reset the request body stream position
        return requestBody;
    }
}
