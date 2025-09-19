namespace EasySmhiRestApi
{
    using EasySmhiRestApi.ApiValidator;
    using EasySmhiRestApi.Clients;
    using EasySmhiRestApi.Utils;
    using Microsoft.AspNetCore.Authentication;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IOpenSmhiDataClient, OpenSmhiDataClient>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            // Bind ApiKeys
            builder.Services.Configure<List<ApiKeyInfo>>(
               builder.Configuration.GetSection("ApiKeys"));

            // Register API key validator
            builder.Services.AddScoped<IApiKeyValidator, AppSettingsApiKeyValidator>();
            builder.Services.AddScoped<IJsonUtils, JsonUtils>();

            // Add authentication
            builder.Services
                .AddAuthentication("ApiKey")
                .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticatorHandler>(
                    "ApiKey", null);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
