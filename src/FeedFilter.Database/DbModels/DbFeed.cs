using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeedFilter.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FeedFilter.Database.DbModels;

[PrimaryKey(nameof(FeedId))]
[Table("Feeds")]
public class DbFeed {
  [MaxLength(200)] public required string FeedId { get; set; }
  [MaxLength(400)] public required string Description { get; set; }
  [MaxLength(400)] public required string Url { get; set; }
  public Decision DefaultDecision { get; set; }
  public required DateTimeOffset DateCreated { get; set; }
  public required DateTimeOffset DateUpdated { get; set; }
  public required List<DbRule> Rules { get; set; }
}
