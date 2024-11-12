using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Chat.API.Services.Interfaces;

namespace Chat.API.Services
{
    //Сервис, позволяющий рендерить Partial View для отправки по SignalR. Код взят отсюда:
    //https://www.learnrazorpages.com/advanced/render-partial-to-string

    public class RazorRenderer : IRazorRenderer
    {
        #region Injects

        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Ctors

        public RazorRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Public Methods        

        public async Task<string> RenderPartialToStringAsync<TModel>(
            string partialName, 
            TModel model, 
            Dictionary<string, object?>? additionalViewData = null)
        {
            var actionContext = GetActionContext();
            var partial = FindView(actionContext, partialName);

            var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };
            foreach (var viewDataItem in additionalViewData ?? [])
                viewData.TryAdd(viewDataItem.Key, viewDataItem.Value);

            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            using var output = new StringWriter();

            var viewContext = new ViewContext(
                actionContext,
                partial,
                viewData,
                tempData,
                output,
                new HtmlHelperOptions()
            );

            await partial.RenderAsync(viewContext);
            return output.ToString();
        }

        #endregion

        #region Private Methods

        private IView FindView(ActionContext actionContext, string partialName)
        {
            var getPartialResult = _viewEngine.GetView(null, partialName, false);
            if (getPartialResult.Success)
            {
                return getPartialResult.View;
            }
            var findPartialResult = _viewEngine.FindView(actionContext, partialName, false);
            if (findPartialResult.Success)
            {
                return findPartialResult.View;
            }
            var searchedLocations = getPartialResult.SearchedLocations.Concat(findPartialResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] { $"Unable to find partial '{partialName}'. The following locations were searched:" }.Concat(searchedLocations));
            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        #endregion
    }
}
