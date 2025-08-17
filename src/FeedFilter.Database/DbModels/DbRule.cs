using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeedFilter.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FeedFilter.Database.DbModels;

[PrimaryKey(nameof(FeedId), nameof(Index))]
[Table("Rules")]
public class DbRule {
  [MaxLength(200)] public required string FeedId { get; set; }
  public int Index { get; set; }
  public ItemField Field { get; set; }
  [MaxLength(200)] public string? CustomXPath { get; set; }
  [MaxLength(100)] public string? TestedAttributeName { get; set; }
  public TestType TestType { get; set; }
  [MaxLength(200)] public required string TestExpression { get; set; }
  public Decision Decision { get; set; }
  [MaxLength(400)] public string? Comment { get; set; }
}
