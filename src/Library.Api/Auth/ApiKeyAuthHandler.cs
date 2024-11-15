using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Library.Api.Auth;

public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthSchemeOptions>
{
    public ApiKeyAuthHandler(
        IOptionsMonitor<ApiKeyAuthSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API KEY"));
        }

        var header = Request.Headers[HeaderNames.Authorization].ToString();
        if (header != Options.ApiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API KEY"));
        }

        var claim = new[]{
            new Claim(ClaimTypes.Name, "poschuler@poschuler.com"),
            new Claim(ClaimTypes.Role, "poschuler.com")
        };

        var claimsIdentity = new ClaimsIdentity(claim, "ApiKey");

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(claimsIdentity),
            Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
