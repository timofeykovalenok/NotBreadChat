using Chat.API.Interfaces;

namespace Chat.API.Extensions
{
    public static class RazorRendererExtensions
    {
        public static Task<string[]> RenderPartialToStringArrayAsync<TModel>(
            this IRazorRenderer razorRenderer, 
            string partialName, 
            IEnumerable<TModel> models)
        {
            var renderTasks = models.Select(model =>
                razorRenderer.RenderPartialToStringAsync(partialName, model));

            return Task.WhenAll(renderTasks);
        }

        public static Task<string> RenderPartialForUserAsync<TModel>(
            this IRazorRenderer razorRenderer,
            string partialName,
            TModel model,
            long userId)
        {
            //Добавляем UserId для ViewData, чтобы для пользователя генерировалось соответствующее представление
            return razorRenderer.RenderPartialToStringAsync(partialName, model, new() { ["UserId"] = userId });
        }
    }
}
