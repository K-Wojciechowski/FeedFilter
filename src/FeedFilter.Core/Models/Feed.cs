using FeedFilter.Core.Enums;

namespace FeedFilter.Core.Models;

public record Feed(
    string FeedId,
    string Description,
    string Url,
    Decision DefaultDecision,
    DateTimeOffset DateCreated,
    DateTimeOffset DateUpdated,
    IReadOnlyList<Rule> Rules
) : IFeed;
