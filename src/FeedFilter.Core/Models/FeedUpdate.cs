using System.ComponentModel.DataAnnotations;
using FeedFilter.Core.Enums;

namespace FeedFilter.Core.Models;

public record FeedUpdate(
    [Required] string Description,
    [Required] string Url,
    [Required] Decision DefaultDecision,
    [Required] IReadOnlyList<Rule> Rules
) : IFeed;
