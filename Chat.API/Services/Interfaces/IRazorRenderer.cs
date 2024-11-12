namespace Chat.API.Services.Interfaces
{
    public interface IRazorRenderer
    {
        Task<string> RenderPartialToStringAsync<TModel>(
            string partialName,
            TModel model,
            Dictionary<string, object?>? additionalViewData = null);
    }
}
