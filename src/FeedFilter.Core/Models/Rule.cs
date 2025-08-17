using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FeedFilter.Core.Enums;

namespace FeedFilter.Core.Models;

public record Rule(
    [Required] int Index,
    [Required] ItemField Field,
    string? CustomXPath,
    string? TestedAttributeName,
    [Required] TestType TestType,
    [Required] string TestExpression,
    [Required] Decision Decision,
    string? Comment
) {
  private readonly Regex? _compiledRegex =
      TestType == TestType.Regex ? new Regex(TestExpression, RegexOptions.IgnoreCase) : null;

  public bool Match(string value) => TestType switch {
      TestType.Exact => value == TestExpression,
      TestType.Contains => value.Contains(TestExpression),
      TestType.StartsWith => value.StartsWith(TestExpression),
      TestType.EndsWith => value.EndsWith(TestExpression),
      TestType.Regex => _compiledRegex!.IsMatch(value),
      _ => throw new InvalidOperationException("Unexpected TestType")
  };
}
