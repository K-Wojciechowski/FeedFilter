using System.Net;
using FeedFilter.Core;
using FeedFilter.Core.Models;
using FeedFilter.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedFilter.Web.Server.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class TestAdminController(IFilteringEngine engine, IHttpClientFactory httpClientFactory, IFeedFilterRepository repository)
    : ControllerBase {
  [HttpPost("test", Name = "TestFeed")]
  public async Task<ActionResult<FeedFilteringResult>> TestFeed(
      [FromBody] FeedUpdate feed,
      CancellationToken cancellationToken)
    => await TestFeedCore(feed, cancellationToken).ConfigureAwait(false);

  [HttpPost("feeds/{feedId}/test", Name = "TestFeedById")]
  public async Task<ActionResult<FeedFilteringResult>> TestFeedById(
      [FromRoute] string feedId,
      CancellationToken cancellationToken) {
    var feed = await repository.GetFeed(feedId, cancellationToken).ConfigureAwait(false);
    if (feed == null) return NotFound();
    return await TestFeedCore(feed, cancellationToken).ConfigureAwait(false);
  }

  private async Task<ActionResult<FeedFilteringResult>> TestFeedCore(
      [FromBody] IFeed feed,
      CancellationToken cancellationToken) {
    var httpClient = httpClientFactory.CreateClient(Constants.ProxyHttpClientName);
    var message = new HttpRequestMessage(HttpMethod.Get, feed.Url);
    message.Headers.UserAgent.TryParseAdd(Constants.UserAgentString);
    var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);

    if (response.StatusCode != HttpStatusCode.OK) {
      return Problem(
          detail: $"Feed returned status code {response.StatusCode}",
          statusCode: (int)HttpStatusCode.BadGateway);
    }

    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    return engine.Filter(feed, content);
  }
}
