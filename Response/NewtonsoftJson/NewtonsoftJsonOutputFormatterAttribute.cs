using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

using System.Buffers;

namespace dotnet_api_extensions.Response.NewtonsoftJson
{
	/// <summary>
	/// 
	/// </summary>
	public class NewtonsoftJsonOutputFormatterAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			if (context.Result is ObjectResult objectResult)
			{
				var jsonOptions = context.HttpContext.RequestServices.GetService<IOptions<MvcNewtonsoftJsonOptions>>();
				if (jsonOptions == null)
					return;


				objectResult.Formatters.RemoveType<SystemTextJsonOutputFormatter>();
				objectResult.Formatters.Add(new NewtonsoftJsonOutputFormatter(
					jsonOptions.Value.SerializerSettings,
					context.HttpContext.RequestServices.GetRequiredService<ArrayPool<char>>(),
					context.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>().Value, null));
			}
			else
			{
				base.OnActionExecuted(context);
			}
		}
	}
}
