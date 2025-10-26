using Microsoft.Extensions.DependencyInjection;

namespace FeedFilter.Core;

public static class ServiceCollectionExtensions {
  public static IServiceCollection AddFeedFilterCore(this IServiceCollection services) => services
      .AddScoped<IFilteringEngine, FilteringEngine>()
      .AddSingleton<IXmlParser, XmlParser>();
}
