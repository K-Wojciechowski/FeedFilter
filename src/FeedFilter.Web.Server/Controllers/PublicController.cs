using System.Net;
using FeedFilter.Core;
using FeedFilter.Core.Models;
using FeedFilter.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace FeedFilter.Web.Server.Controllers;

[Controller]
[Route("")]
[AllowAnonymous]
public class PublicController(
    IFilteringEngine engine,
    IFeedFilterRepository repository,
    IHttpClientFactory httpClientFactory,
    ILogger<PublicController> logger
) : ControllerBase {

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("", Name = "Index")]
  public IActionResult Index() {
    return PlainTextResponse("", 418);
  }

  // This endpoint lets us serve the favicon even if static files are disabled.
  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("favicon.ico", Name = "Favicon")]
  public IResult Favicon() {
    var assembly = typeof(PublicController).Assembly;
    var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.res.favicon.ico");
    return stream == null ? Results.NotFound() : Results.Stream(stream, "image/x-icon");
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("{feedId:regex(^[[a-z0-9-.]]+$)}", Name = "ProxyFeed")]
  public async Task<IActionResult> Get([FromRoute] string feedId, CancellationToken cancellationToken) {
    var feed = await repository.GetFeed(feedId, cancellationToken).ConfigureAwait(false);
    if (feed == null) {
      logger.LogInformation($"Feed with ID '{feedId}' not found");
      HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
      return NotFound("Feed not found");
    }

    var httpClient = httpClientFactory.CreateClient(Constants.ProxyHttpClientName);
    var proxyContext = new ProxyContext(feed, HttpContext.Request.Headers.IfModifiedSince, HttpContext.Request.Headers.IfNoneMatch);

    try {
      return await Proxy(new Uri(feed.Url), proxyContext, httpClient, cancellationToken).ConfigureAwait(false);
    } catch (Exception e) {
      logger.LogError(e, "Failed to filter feed: {Error}", e);
      return PlainTextResponse("FeedFilter error", 500);
    }
  }

  private async Task<IActionResult> Proxy(Uri uri, ProxyContext proxyContext, HttpClient httpClient, CancellationToken cancellationToken) {
    var feedId = proxyContext.Feed.FeedId;
    var request = new HttpRequestMessage(HttpMethod.Get, uri);
    request.Headers.UserAgent.TryParseAdd(Constants.UserAgentString);

    if (proxyContext.IfModifiedSince.Count > 0) {
      request.Headers.TryAddWithoutValidation("If-Modified-Since", (IEnumerable<string>)proxyContext.IfModifiedSince);
    }

    if (proxyContext.IfNoneMatch.Count > 0) {
      request.Headers.TryAddWithoutValidation("If-None-Match", (IEnumerable<string>)proxyContext.IfNoneMatch);
    }

    logger.LogInformation("Sending HTTP request to '{Uri}' for feed '{FeedId}'", uri, feedId);
    var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    var numericStatusCode = (int)response.StatusCode;
    logger.LogInformation("HTTP request to '{Uri}' for feed '{FeedId}' returned status code {StatusCode} {StatusCodeName}",
        uri, feedId, numericStatusCode, response.StatusCode);

    if (response.Content.Headers.TryGetValues("Last-Modified", out var lastModified)) {
      HttpContext.Response.Headers.LastModified = new StringValues(lastModified.ToArray());
    }

    if (response.Headers.TryGetValues("ETag", out var eTag)) {
      HttpContext.Response.Headers.ETag = new StringValues(eTag.ToArray());
    }

    if (response.StatusCode == HttpStatusCode.NotModified) {
      logger.LogDebug("Returning NotModified for feed '{FeedId}'", feedId);
      return StatusCode((int)HttpStatusCode.NotModified);
    }

    if (numericStatusCode is >= 300 and <= 399 && response.Headers.Location != null) {
      logger.LogDebug("Redirecting feed '{FeedId}' to '{Uri}'", feedId, response.Headers.Location);
      return await Proxy(response.Headers.Location!, proxyContext, httpClient, cancellationToken).ConfigureAwait(false);
    }

    if (response.StatusCode == HttpStatusCode.OK) {
      var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
      var result = engine.Filter(proxyContext.Feed, content);
      var mediaType = response.Content.Headers.ContentType?.MediaType ?? "application/xml";
      var contentType = $"{mediaType}; charset=utf-8";
      logger.LogDebug("Returning successful filtered response for feed '{FeedId}'", feedId);
      return PlainTextResponse(result.FilteredXml, contentType: contentType);
    }

    logger.LogError("Unexpected status code when requesting '{Uri}' for '{FeedId}': {StatusCode} {StatusCodeName}",
        uri, feedId, numericStatusCode, response.StatusCode);
    if (logger.IsEnabled(LogLevel.Debug)) {
      logger.LogDebug("Response body: {Body}", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
    }

    return PlainTextResponse($"Feed returned status code {numericStatusCode} {response.StatusCode}", numericStatusCode);
  }

  public IActionResult PlainTextResponse(string text, int? statusCode = null, string? contentType = null) =>
      new ContentResult { StatusCode = statusCode ?? 200, Content = text, ContentType = contentType ?? "text/plain" };

  private record ProxyContext(Feed Feed, StringValues IfModifiedSince, StringValues IfNoneMatch);
}
