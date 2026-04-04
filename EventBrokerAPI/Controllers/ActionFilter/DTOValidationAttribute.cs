using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventBrokerAPI.Controllers.ActionFilter;

public class DTOValidationAttribute : IActionFilter
{
    public DTOValidationAttribute() { }
    public void OnActionExecuting(ActionExecutingContext context)
    {
        
        var action = context.RouteData.Values["action"];
        var controller = context.RouteData.Values["controller"];

        var param = context.ActionArguments.SingleOrDefault(x => x.Value != null && x.Key.EndsWith("DTO")).Value;
        if (param is null)
        {
            context.Result = new BadRequestObjectResult($"Переданный DTO объект null. Controller: {controller}, action: {action}");
            return;
        }

        if (!context.ModelState.IsValid)
            context.Result = new UnprocessableEntityObjectResult(context.ModelState);
    }
    public void OnActionExecuted(ActionExecutedContext context) { }
}
