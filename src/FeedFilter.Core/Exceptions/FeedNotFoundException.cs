namespace FeedFilter.Core.Exceptions;

public class FeedNotFoundException(string feedId) : Exception($"Feed with id '{feedId}' not found") {
  public string FeedId { get; } = feedId;
}
