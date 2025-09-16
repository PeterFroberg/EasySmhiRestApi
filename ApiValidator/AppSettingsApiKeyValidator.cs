namespace EasySmhiRestApi.ApiValidator
{
    using Microsoft.Extensions.Options;

    public class AppSettingsApiKeyValidator : IApiKeyValidator
    {
        private readonly IEnumerable<ApiKeyInfo> _apiKeys;

        public AppSettingsApiKeyValidator(IOptions<List<ApiKeyInfo>> options)
        {
            _apiKeys = options.Value;
        }

        public Task<ApiKeyInfo?> ValidateAsync(string key)
        {
            var match = _apiKeys.FirstOrDefault(k => k.Key == key && !string.IsNullOrEmpty(k.Key));
            return Task.FromResult<ApiKeyInfo?>(match);
        }
    }
}
