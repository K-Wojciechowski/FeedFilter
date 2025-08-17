using FeedFilter.Database.DbModels;
using Microsoft.EntityFrameworkCore;

namespace FeedFilter.Database;

public class FeedFilterDbContext(DbContextOptions<FeedFilterDbContext> options) : DbContext(options) {
  public DbSet<DbFeed> Feeds { get; set; }
  public DbSet<DbRule> FilterRules { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<DbFeed>().ToTable("Feeds");
    modelBuilder.Entity<DbRule>().ToTable("Rules");
  }
}
