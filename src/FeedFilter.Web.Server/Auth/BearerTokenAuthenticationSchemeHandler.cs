using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FeedFilter.Web.Server.Auth;

public class BearerTokenAuthenticationSchemeHandler(
    IOptionsMonitor<BearerTokenAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<BearerTokenAuthenticationSchemeOptions>(options, logger, encoder) {
  public const string Name = "Bearer";

  protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
    var authorizationHeaders = Context.Request.Headers.Authorization;
    if (authorizationHeaders.Count == 0) {
      return Task.FromResult(AuthenticateResult.NoResult());
    }

    if (authorizationHeaders.Count > 1) {
      return Task.FromResult(AuthenticateResult.Fail("Too many authorization headers"));
    }

    var authorizationHeader = authorizationHeaders[0];

    if (authorizationHeader == null) {
      return Task.FromResult(AuthenticateResult.Fail("Empty authorization header"));
    }

    var authorization = authorizationHeader.Split(' ', 2);
    if (authorization[0] != "Bearer") {
      return Task.FromResult(AuthenticateResult.Fail("Invalid authorization scheme"));
    }

    if (!Options.AdminApiEnabled) {
      return Task.FromResult(AuthenticateResult.Fail("Admin access is disabled"));
    }

    if (string.IsNullOrWhiteSpace(Options.AdminToken)) {
      return Task.FromResult(AuthenticateResult.Fail("Admin token is not configured"));
    }

    var providedToken = Encoding.UTF8.GetBytes(authorization[1]);
    var configuredToken = Encoding.UTF8.GetBytes(Options.AdminToken);

    if (!CryptographicOperations.FixedTimeEquals(providedToken, configuredToken)) {
      return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
    }

    var claims = new[] { new Claim(ClaimTypes.Name, "Administrator") };
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, this.Scheme.Name));
    var ticket = new AuthenticationTicket(principal, this.Scheme.Name);
    return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}
