namespace EasySmhiRestApi.ApiValidator
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;
    using System.Security.Claims;
    using System.Text.Encodings.Web;

    public class ApiKeyAuthenticatorHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IApiKeyValidator _apiValidator;

        public ApiKeyAuthenticatorHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApiKeyValidator validator)
            : base(options, logger, encoder, clock)
        {
            _apiValidator = validator;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Api-Key", out var keyValues))
            {
                return AuthenticateResult.Fail("Missing API key.");
            }

            var key = keyValues.FirstOrDefault();
            var info = await _apiValidator.ValidateAsync(key);
            if (info == null)
            {
                return AuthenticateResult.Fail("Invalid API key.");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, info.Owner) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
