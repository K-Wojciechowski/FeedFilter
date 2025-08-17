using FeedFilter.Core.Enums;

namespace FeedFilter.Core.Models;

public interface IFeed {
  string Description { get; }
  string Url { get; }
  Decision DefaultDecision { get; }
  IReadOnlyList<Rule> Rules { get; }
}
