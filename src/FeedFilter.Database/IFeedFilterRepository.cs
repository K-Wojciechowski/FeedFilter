using FeedFilter.Core.Models;

namespace FeedFilter.Database;

public interface IFeedFilterRepository {
  Task<List<Feed>> GetFeeds(CancellationToken cancellationToken);

  Task<Feed> GetRequiredFeed(string feedId, CancellationToken cancellationToken);

  Task<Feed?> GetFeed(string feedId, CancellationToken cancellationToken);

  Task SaveFeed(string feedId, FeedUpdate feed, CancellationToken cancellationToken);

  Task DeleteFeed(string feedId, CancellationToken cancellationToken);
}
