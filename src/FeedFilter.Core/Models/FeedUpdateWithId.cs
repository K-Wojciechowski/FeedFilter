using FeedFilter.Core.Enums;

namespace FeedFilter.Core.Models;

public record FeedUpdateWithId(
    string FeedId,
    string Description,
    string Url,
    Decision DefaultDecision,
    IReadOnlyList<Rule> Rules
) : FeedUpdate(Description, Url, DefaultDecision, Rules);
