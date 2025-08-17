using Microsoft.AspNetCore.Authentication;

namespace FeedFilter.Web.Server.Auth;

public class BearerTokenAuthenticationSchemeOptions : AuthenticationSchemeOptions {
  public bool AdminApiEnabled { get; set; }
  public string? AdminToken { get; set; }
}
