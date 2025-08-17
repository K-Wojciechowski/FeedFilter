using FeedFilter.Core.Models;
using FeedFilter.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedFilter.Web.Server.Controllers;

[ApiController]
[Route("api/feeds")]
[Authorize]
public class FeedAdminController(IFeedFilterRepository repository) : ControllerBase {
  [HttpGet("", Name = "GetFeeds")]
  [ProducesResponseType<List<Feed>>(StatusCodes.Status200OK, "application/json")]
  public async Task<List<Feed>> GetFeeds(CancellationToken cancellationToken) {
    return await repository.GetFeeds(cancellationToken).ConfigureAwait(false);
  }

  [HttpPost("", Name = "UpdateFeeds")]
  [ProducesResponseType<List<Feed>>(StatusCodes.Status200OK, "application/json")]
  public async Task<ActionResult<List<Feed>>> UpdateFeeds([FromBody] List<FeedUpdateWithId> feedUpdates,
      CancellationToken cancellationToken) {
    foreach (var feedUpdate in feedUpdates) {
      await repository.SaveFeed(feedUpdate.FeedId, feedUpdate, cancellationToken).ConfigureAwait(false);
    }

    return await repository.GetFeeds(cancellationToken).ConfigureAwait(false);
  }

  [HttpGet("{feedId}", Name = "GetFeed")]
  [ProducesResponseType<Feed>(StatusCodes.Status200OK, "application/json")]
  public async Task<ActionResult<Feed>> GetFeed([FromRoute] string feedId, CancellationToken cancellationToken) {
    var feed = await repository.GetFeed(feedId, cancellationToken).ConfigureAwait(false);
    return feed != null ? feed : new NotFoundResult();
  }

  [HttpPost("{feedId}", Name = "UpdateFeed")]
  [ProducesResponseType<Feed>(StatusCodes.Status200OK, "application/json")]
  public async Task<ActionResult<Feed>> UpdateFeed([FromRoute] string feedId, [FromBody] FeedUpdate feedUpdate,
      CancellationToken cancellationToken) {
    await repository.SaveFeed(feedId, feedUpdate, cancellationToken).ConfigureAwait(false);
    return await repository.GetRequiredFeed(feedId, cancellationToken).ConfigureAwait(false);
  }

  [HttpDelete("{feedId}", Name = "DeleteFeed")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> DeleteFeed([FromRoute] string feedId, CancellationToken cancellationToken) {
    await repository.DeleteFeed(feedId, cancellationToken).ConfigureAwait(false);
    return NoContent();
  }
}
