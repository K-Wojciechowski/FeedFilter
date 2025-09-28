namespace FeedFilter.Web.Server;

public static class Constants {
  private static readonly string _version = typeof(Constants).Assembly.GetName().Version?.ToString() ?? "UNKNOWN";
  public static readonly string UserAgentString = $"FeedFilter/{_version}";
  public const string ProxyHttpClientName = "FeedFilterProxyClient";
}
