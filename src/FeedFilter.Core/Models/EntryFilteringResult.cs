using FeedFilter.Core.Enums;

namespace FeedFilter.Core.Models;

public record EntryFilteringResult(string? EntryTitle, Rule? DecidingRule, Decision Decision);
