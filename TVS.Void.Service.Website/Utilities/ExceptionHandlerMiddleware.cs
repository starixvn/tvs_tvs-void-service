using System;
using System.Net;
using System.Threading.Tasks;
using Fanex.Logging.Extensions.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace TVS.Void.Service.Website.Utilities;

public class ExceptionHandlerMiddleware(RequestDelegate next, IConfiguration configuration)
{
#pragma warning disable S3604
    public IConfiguration Configuration { get; } = configuration;
#pragma warning restore S3604

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await ExceptionHandler.HandleAsync(exception, context);
            await HandleException(context, exception);
        }
    }

    private static async Task HandleException(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var errorMessage = $"<b>Exception Error: {exception.Message} </b> {exception.StackTrace}";
        await context.Response.WriteAsync(errorMessage).ConfigureAwait(false);
    }
}