namespace FeedFilter.Core.Models;

public record FeedFilteringResult(
    IFeed Feed,
    string OriginalXml,
    string FilteredXml,
    List<EntryFilteringResult> EntryResults);
