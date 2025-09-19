namespace EasySmhiRestApi.ApiValidator
{
    public interface IApiKeyValidator
    {
        Task<ApiKeyInfo?> ValidateAsync(string apiKey);
    }
}
