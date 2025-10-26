using FeedFilter.Core.Enums;
using FeedFilter.Core.Models;
using Shouldly;

namespace FeedFilter.Core.Test;

[TestClass]
public sealed class RuleTests {
  [TestMethod]
  public void Match_Exact_ReturnsTrueOnlyForExactMatch() {
    // Arrange
    var testExpression = Guid.NewGuid().ToString();
    var rule = CreateRule(TestType.Exact, testExpression);

    // Act & Assert
    rule.Match(testExpression).ShouldBeTrue();
    rule.Match(testExpression.ToUpper()).ShouldBeFalse();
    rule.Match(testExpression + "0").ShouldBeFalse();
    rule.Match("0" + testExpression).ShouldBeFalse();
    rule.Match("0" + testExpression + "0").ShouldBeFalse();
  }

  [TestMethod]
  public void Match_Contains_ReturnsFalseForCaseInsensitiveMatch() {
    // Arrange
    var testExpression = Guid.NewGuid().ToString();
    var rule = CreateRule(TestType.Contains, testExpression);

    // Act & Assert
    rule.Match(testExpression).ShouldBeTrue();
    rule.Match(testExpression.ToUpper()).ShouldBeFalse();
    rule.Match(testExpression + "0").ShouldBeTrue();
    rule.Match("0" + testExpression).ShouldBeTrue();
    rule.Match("0" + testExpression + "0").ShouldBeTrue();
  }

  [TestMethod]
  public void Match_StartsWith_ReturnsTrueForStartsWithOrExactMatch() {
    // Arrange
    var testExpression = Guid.NewGuid().ToString();
    var rule = CreateRule(TestType.StartsWith, testExpression);

    // Act & Assert
    rule.Match(testExpression).ShouldBeTrue();
    rule.Match(testExpression.ToUpper()).ShouldBeFalse();
    rule.Match(testExpression + "0").ShouldBeTrue();
    rule.Match("0" + testExpression).ShouldBeFalse();
    rule.Match("0" + testExpression + "0").ShouldBeFalse();
  }

  [TestMethod]
  public void Match_EndsWith_ReturnsTrueForEndsWithOrExactMatch() {
    // Arrange
    var testExpression = Guid.NewGuid().ToString();
    var rule = CreateRule(TestType.EndsWith, testExpression);

    // Act & Assert
    rule.Match(testExpression).ShouldBeTrue();
    rule.Match(testExpression.ToUpper()).ShouldBeFalse();
    rule.Match(testExpression + "0").ShouldBeFalse();
    rule.Match("0" + testExpression).ShouldBeTrue();
    rule.Match("0" + testExpression + "0").ShouldBeFalse();
  }

  [TestMethod]
  public void Match_PlainRegex_MatchesCaseInsensitively() {
    // Arrange
    var testExpression = Guid.NewGuid().ToString();
    var rule = CreateRule(TestType.Regex, testExpression);

    // Act & Assert
    rule.Match(testExpression).ShouldBeTrue();
    rule.Match(testExpression.ToUpper()).ShouldBeTrue();
    rule.Match(testExpression + "0").ShouldBeTrue();
    rule.Match("0" + testExpression).ShouldBeTrue();
    rule.Match("0" + testExpression + "0").ShouldBeTrue();
  }

  [TestMethod]
  public void Match_ComplicatedRegex_MatchesIfRegexMatches() {
    // Arrange
    var testExpression = "^a\\d{2,3}b";
    var rule = CreateRule(TestType.Regex, testExpression);

    // Act & Assert
    rule.Match("a111b").ShouldBeTrue();
    rule.Match("a111bqwerty").ShouldBeTrue();
    rule.Match("A22B").ShouldBeTrue();
    rule.Match("a333c").ShouldBeFalse();
  }

  private static Rule CreateRule(TestType testType, string testExpression) => new Rule(
      Index: 0,
      Field: ItemField.Title,
      CustomXPath: null,
      TestedAttributeName: null,
      TestType: testType,
      TestExpression: testExpression,
      Decision: Decision.Accept,
      Comment: null);
}
