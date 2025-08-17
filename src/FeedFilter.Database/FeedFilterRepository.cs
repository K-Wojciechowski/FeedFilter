using FeedFilter.Core.Exceptions;
using FeedFilter.Core.Models;
using FeedFilter.Database.DbModels;
using Microsoft.EntityFrameworkCore;

namespace FeedFilter.Database;

public class FeedFilterRepository(FeedFilterDbContext dbContext, TimeProvider timeProvider) : IFeedFilterRepository {
  private static readonly Func<FeedFilterDbContext, string, CancellationToken, Task<DbFeed?>> _getDbFeedQuery =
      EF.CompileAsyncQuery((FeedFilterDbContext context, string feedId, CancellationToken ct) =>
          context.Feeds
              .Include(f => f.Rules.OrderBy(r => r.Index))
              .FirstOrDefault(f => f.FeedId == feedId));

  public async Task<List<Feed>> GetFeeds(CancellationToken cancellationToken) {
    var feeds = await dbContext.Feeds
        .OrderBy(f => f.FeedId)
        .Include(f => f.Rules.OrderBy(r => r.Index))
        .ToListAsync(cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    return feeds.Select(Mapper.Map).ToList();
  }

  public async Task<Feed> GetRequiredFeed(string feedId, CancellationToken cancellationToken) {
    var rawFeed = await GetDbFeed(feedId, cancellationToken).ConfigureAwait(false);
    return rawFeed != null ? Mapper.Map(rawFeed) : throw new FeedNotFoundException(feedId);
  }

  public async Task<Feed?> GetFeed(string feedId, CancellationToken cancellationToken) {
    var dbFeed = await GetDbFeed(feedId, cancellationToken).ConfigureAwait(false);
    return dbFeed != null ? Mapper.Map(dbFeed) : null;
  }

  public async Task SaveFeed(string feedId, FeedUpdate feed, CancellationToken cancellationToken) {
    var now = timeProvider.GetUtcNow();
    var dbFeed = await GetDbFeed(feedId, cancellationToken).ConfigureAwait(false);
    if (dbFeed == null) {
      dbFeed = new DbFeed {
          FeedId = feedId,
          Description = feed.Description,
          Url = feed.Url,
          DefaultDecision = feed.DefaultDecision,
          DateCreated = now,
          DateUpdated = now,
          Rules = []
      };
      dbContext.Add(dbFeed);
    } else {
      dbFeed.Description = feed.Description;
      dbFeed.Url = feed.Url;
      dbFeed.DefaultDecision = feed.DefaultDecision;
      dbFeed.DateUpdated = now;

      dbContext.RemoveRange(dbFeed.Rules);
      dbFeed.Rules = [];
    }

    var rules = feed.Rules.Select(r => Mapper.Map(r, feedId)).ToList();
    dbContext.AddRange(rules);
    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
  }

  public async Task DeleteFeed(string feedId, CancellationToken cancellationToken) {
    var feed = await dbContext.Feeds.FindAsync([feedId], cancellationToken).ConfigureAwait(false);
    if (feed == null) {
      throw new FeedNotFoundException(feedId);
    }

    dbContext.Feeds.Remove(feed);
    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
  }

  private async Task<DbFeed?> GetDbFeed(string feedId, CancellationToken cancellationToken)
    => await _getDbFeedQuery(dbContext, feedId, cancellationToken).ConfigureAwait(false);
}
