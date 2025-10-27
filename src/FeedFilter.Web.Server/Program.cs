using System.Text.Json;
using System.Text.Json.Serialization;
using FeedFilter.Core;
using FeedFilter.Database;
using FeedFilter.Web.Server.Auth;
using FeedFilter.Web.Server.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace FeedFilter.Web.Server;

internal class Program {
  public static async Task Main(string[] args) {
    var builder = WebApplication.CreateBuilder(args);

    // Basic ASP.NET Core configuration
    builder.Services.AddControllers().AddJsonOptions(options => {
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
      options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
    builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
    builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSimpleConsole(options => {
      options.IncludeScopes = true;
      options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    }));

    // Our services and dependencies
    builder.Services.AddDbContext<FeedFilterDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<IFeedFilterRepository, FeedFilterRepository>();
    builder.Services.AddFeedFilterCore();
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddHttpClient(Constants.ProxyHttpClientName)
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = true, MaxAutomaticRedirections = 3 });

    var migrationEnabled = builder.Configuration.GetValue("Database:MigrateOnStartup", false);
    // Admin authentication
    var adminUiEnabled = builder.Configuration.GetValue("Admin:UiEnabled", false);
    var adminApiEnabled = adminUiEnabled || builder.Configuration.GetValue("Admin:ApiEnabled", false);

    builder.Services.AddAuthentication(BearerTokenAuthenticationSchemeHandler.Name)
        .AddScheme<BearerTokenAuthenticationSchemeOptions, BearerTokenAuthenticationSchemeHandler>(
            BearerTokenAuthenticationSchemeHandler.Name,
            o => {
              o.AdminToken = builder.Configuration.GetValue<string?>("Admin:Token");
              o.AdminApiEnabled = adminApiEnabled;
            });


    var app = builder.Build();

    if (adminUiEnabled) {
      app.UseDefaultFiles();
      app.UseStaticFiles();
    }

    if (app.Environment.IsDevelopment()) {
      app.MapOpenApi();
      app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
      });
    }


    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    if (adminUiEnabled) {
      app.MapFallbackToFile("_{path}", "/index.html");
      app.MapFallbackToFile("_edit/{id}", "/index.html");
    }

    if (migrationEnabled) {
      for (var attempt = 0; attempt < 5; attempt++) {
        using (var scope = app.Services.CreateScope()) {
          var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
          try {
            var db = scope.ServiceProvider.GetRequiredService<FeedFilterDbContext>();
            await db.Database.MigrateAsync().ConfigureAwait(false);
            logger.LogInformation("Database migration completed successfully");
          } catch (Exception ex) {
            logger.LogCritical(ex, "Database migration attempt {Attempt} failed: {Error}", attempt + 1, ex.Message);
            if (attempt == 4) {
              throw;
            }
            await Task.Delay(5000).ConfigureAwait(false);
          }
        }
      }
    }

    await app.RunAsync().ConfigureAwait(false);
  }
}
