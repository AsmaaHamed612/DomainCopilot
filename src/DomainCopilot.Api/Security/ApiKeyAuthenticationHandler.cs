using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace DomainCopilot.Api.Security;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-API-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var suppliedKey = Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrEmpty(suppliedKey))
            return Task.FromResult(AuthenticateResult.NoResult());

        var (role, userId) = Matches(suppliedKey, configuration["Authentication:AdjusterApiKey"])
            ? ("Adjuster", configuration["Authentication:AdjusterId"] ?? "adjuster")
            : Matches(suppliedKey, configuration["Authentication:ReviewerApiKey"])
                ? ("Reviewer", configuration["Authentication:ReviewerId"])
                : (null, null);
        if (role is null || string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(AuthenticateResult.Fail("The API key is invalid."));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool Matches(string suppliedKey, string? configuredKey)
    {
        if (string.IsNullOrEmpty(configuredKey)) return false;
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(suppliedKey),
            Encoding.UTF8.GetBytes(configuredKey));
    }
}
