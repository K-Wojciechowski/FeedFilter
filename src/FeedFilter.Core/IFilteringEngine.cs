using FeedFilter.Core.Models;

namespace FeedFilter.Core;

public interface IFilteringEngine {
  FeedFilteringResult Filter(IFeed feed, string xml);
}
