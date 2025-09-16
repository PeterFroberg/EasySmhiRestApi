namespace EasySmhiRestApi.ApiValidator
{
    public class ApiKeyInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
