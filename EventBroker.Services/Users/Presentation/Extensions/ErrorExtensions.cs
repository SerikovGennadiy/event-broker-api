using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Users.Domain.Exceptions;
using Users.Domain.Exceptions.Common;

namespace Users.API.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void UseExceptionHandling(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            app.UseExceptionHandler((appError) =>
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
                            BadRequestException => StatusCodes.Status400BadRequest,
                            WhoAreYouException => StatusCodes.Status401Unauthorized,
                            SecurityTokenException => StatusCodes.Status401Unauthorized,
                            AccessDeniedException => StatusCodes.Status403Forbidden,
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
