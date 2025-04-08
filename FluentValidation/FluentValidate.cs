using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace dotnet_api_extensions.FluentValidation
{
    /// <summary>
    /// Filter to use if automated aspnet pipeline is not feasible
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="serviceProvider"></param>
    public class FluentValidate(IServiceProvider serviceProvider) : IAsyncActionFilter
    {

        #region constructor

        #endregion

        #region Methods

        /// <summary>
        /// Called before the action executes, after model binding is complete
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <param name="next"></param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //only in POST requests
            if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                await next();
                return;
            }

            foreach (var argument in context.ActionArguments.Where(x => !CommonHelper.IsSimpleType(x.Value?.GetType())))
            {
                Type genericType = typeof(IValidator<>).MakeGenericType(argument.Value!.GetType());
                var validator = (IValidator)serviceProvider.GetService(genericType);
                if (validator is null) continue;
                var contextValidator = new ValidationContext<object>(argument.Value);
                var result = await validator.ValidateAsync(contextValidator);
                if (result.IsValid) continue;

                result.AddToModelState(context.ModelState, "");
                var hasJsonData = context.HttpContext.Request.ContentType?.Contains("application/json") ?? false;
                if (!hasJsonData) continue;
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            await next();
        }
        #endregion
    }
}
