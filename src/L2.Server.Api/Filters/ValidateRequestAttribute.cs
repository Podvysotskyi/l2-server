using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace L2.Server.Api.Filters;

public abstract class ValidateRequestAttribute<TRequest> : ActionFilterAttribute
    where TRequest : class
{
    protected abstract Dictionary<string, string[]> Validate(TRequest request);

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.ActionArguments.Values.OfType<TRequest>().SingleOrDefault();
        if (request is null)
        {
            return;
        }

        var errors = Validate(request);
        if (errors.Count == 0)
        {
            return;
        }

        var problemDetailsFactory = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>();
        var modelState = new ModelStateDictionary();
        foreach (var (key, messages) in errors)
        {
            foreach (var message in messages)
            {
                modelState.AddModelError(key, message);
            }
        }

        context.Result = new BadRequestObjectResult(problemDetailsFactory.CreateValidationProblemDetails(
            context.HttpContext,
            modelState));
    }
}
