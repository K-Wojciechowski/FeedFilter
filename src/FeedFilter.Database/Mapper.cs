using FeedFilter.Core.Models;
using FeedFilter.Database.DbModels;

namespace FeedFilter.Database;

public static class Mapper {
  public static Feed Map(DbFeed dbFeed) => new(
      FeedId: dbFeed.FeedId,
      Description: dbFeed.Description,
      Url: dbFeed.Url,
      DefaultDecision: dbFeed.DefaultDecision,
      DateCreated: dbFeed.DateCreated,
      DateUpdated: dbFeed.DateUpdated,
      Rules: dbFeed.Rules.Select(Map).ToList()
  );

  public static DbFeed Map(Feed feed) => new() {
      FeedId = feed.FeedId,
      Description = feed.Description,
      Url = feed.Url,
      DefaultDecision = feed.DefaultDecision,
      DateCreated = feed.DateCreated,
      DateUpdated = feed.DateUpdated,
      Rules = feed.Rules.Select(r => Map(r, feed.FeedId)).ToList()
  };

  public static Rule Map(DbRule dbRule) => new(
      Index: dbRule.Index,
      Field: dbRule.Field,
      CustomXPath: dbRule.CustomXPath,
      TestedAttributeName: dbRule.TestedAttributeName,
      TestType: dbRule.TestType,
      TestExpression: dbRule.TestExpression,
      Decision: dbRule.Decision,
      Comment: dbRule.Comment
  );

  public static DbRule Map(Rule rule, string feedId) => new() {
      FeedId = feedId,
      Index = rule.Index,
      Field = rule.Field,
      CustomXPath = rule.CustomXPath,
      TestedAttributeName = rule.TestedAttributeName,
      TestType = rule.TestType,
      TestExpression = rule.TestExpression,
      Decision = rule.Decision,
      Comment = rule.Comment
  };
}
