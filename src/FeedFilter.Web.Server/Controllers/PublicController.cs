using System.Net;
using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;
using FeedFilter.Core;
using FeedFilter.Core.Models;
using FeedFilter.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedFilter.Web.Server.Controllers;

[Controller]
[Route("")]
[AllowAnonymous]
public class PublicController(
    IFilteringEngine engine,
    IFeedFilterRepository repository,
    ILogger<PublicController> logger
) : ControllerBase {
  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("{feedId}", Name = "ProxyFeed")]
  public async Task Get([FromRoute] string feedId, CancellationToken cancellationToken) {
    var feed = await repository.GetFeed(feedId, cancellationToken).ConfigureAwait(false);
    if (feed == null) {
      logger.LogInformation($"Feed with ID '{feedId}' not found");
      HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
      await HttpContext.Response.WriteAsync("Feed not found", cancellationToken).ConfigureAwait(false);
      return;
    }

    var httpProxyOptions = HttpProxyOptionsBuilder.Instance
        .WithHttpClientName(Constants.ProxyHttpClientName)
        .WithBeforeSend(BeforeSend)
        .WithAfterReceive((_, message) => AfterReceive(message, feed))
        .Build();

    await this.HttpProxyAsync(feed.Url, httpProxyOptions: httpProxyOptions).ConfigureAwait(false);
  }

  private async Task AfterReceive(HttpResponseMessage message, IFeed feed) {
    if (message.StatusCode == HttpStatusCode.OK) {
      var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
      try {
        var result = engine.Filter(feed, content);
        message.Content = new StringContent(result.FilteredXml);
      } catch (Exception e) {
        message.StatusCode = HttpStatusCode.InternalServerError;
        message.Content = new StringContent("FeedFilter error");
        logger.LogError(e, "Failed to filter feed: {Error}", e.Message);
      }
    }
  }

  private static Task BeforeSend(HttpContext context, HttpRequestMessage message) {
    message.Headers.TryAddWithoutValidation("User-Agent", Constants.UserAgentString);
    return Task.CompletedTask;
  }
}
