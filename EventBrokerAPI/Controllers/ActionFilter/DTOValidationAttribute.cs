using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;

/// <summary>Перехват до ModelBinding'а</summary>
/// <remarks>проверка на пустой запрос для PUT и POST</remarks>
[AttributeUsage(AttributeTargets.All)]
public class ValidateDTOFilter : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        if (actionDescriptor == null) return;

        var action = context.RouteData.Values["action"];
        var controller = context.RouteData.Values["controller"];

        // TODO добавить проекрку на пустой объект
        // Получаем параметры action
        if (context.HttpContext.Request.ContentLength == 0)
        {
            context.Result = new BadRequestObjectResult(
                $"Тело запроса не может быть пустым. Controller: {controller}, action: {action}");
            return;
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}