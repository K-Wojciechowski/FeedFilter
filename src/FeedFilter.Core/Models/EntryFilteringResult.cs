using FeedFilter.Core.Enums;

namespace FeedFilter.Core.Models;

public record EntryFilteringResult(string? EntryTitle, Rule? DecidingRule, IReadOnlyCollection<string>? TestedValues, Decision Decision);
