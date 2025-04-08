using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace dotnet_api_extensions.MVC
{
    /// <summary>
    /// To generate MVC view
    /// </summary>
    public class ViewGenerator
    {
        /// <summary>
        /// Get MVC View as String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <param name="isMainPage"></param>
        /// <returns></returns>
        public static async Task<string> RenderViewToString<T>(Controller controller, string viewName, T model, bool isMainPage = false)
        {
            controller.ViewData.Model = model;

            using var writer = new StringWriter();
            var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !isMainPage);

            if (viewResult.Success == false)
            {
                return $"{viewName} View cannot be found.";
            }

            var viewContext = new ViewContext
                (controller.ControllerContext, viewResult.View, controller.ViewData,
                controller.TempData, writer, new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return writer.GetStringBuilder().ToString();
        }
    }
}