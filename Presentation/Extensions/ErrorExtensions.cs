using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Security.Authentication;
using Microsoft.IdentityModel.Tokens;
using Domain.Exceptions.Auth;

namespace EventBrokerAPI.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void UseExceptionHandling(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            app.UseExceptionHandler((IApplicationBuilder appError) =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var error = contextFeature.Error;
                        context.Response.StatusCode = error switch
                        {
                            NotFoundException => StatusCodes.Status404NotFound,
                            BadRequestException => StatusCodes.Status400BadRequest,
                            ConflictException => StatusCodes.Status409Conflict,
                            WhoAreYouException => StatusCodes.Status401Unauthorized,
                            SecurityTokenException => StatusCodes.Status401Unauthorized,
                            _ => StatusCodes.Status500InternalServerError
                        };

                        logger.LogError(error, "Произошла ошибка: {ErrorMessage}", error.Message);

                        await context.Response.WriteAsync(new ErrorDetail()
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = contextFeature.Error.Message
                        }.ToString());
                    }
                });
            });
        }
    }
}
