using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCore.Proxy;
using FeedFilter.Core;
using FeedFilter.Database;
using FeedFilter.Web.Server.Auth;
using FeedFilter.Web.Server.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace FeedFilter.Web.Server;

internal class Program {
  public static void Main(string[] args) {
    var builder = WebApplication.CreateBuilder(args);

    // Basic ASP.NET Core configuration
    builder.Services.AddControllers().AddJsonOptions(options => {
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
      options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
    builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

    // Our services and dependencies
    builder.Services.AddProxies();
    builder.Services.AddDbContext<FeedFilterDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<IFeedFilterRepository, FeedFilterRepository>();
    builder.Services.AddScoped<IFilteringEngine, FilteringEngine>();
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddHttpClient(Constants.ProxyHttpClientName)
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = true, MaxAutomaticRedirections = 3 });

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
      app.MapFallbackToFile("/index.html");
    }

    app.Run();
  }
}
