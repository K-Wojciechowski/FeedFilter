using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FeedFilter.Web.Server.OpenApi;

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer {
  public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
      CancellationToken cancellationToken) {
    var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false);
    if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer")) {
      // Add the security scheme at the document level
      var requirements = new Dictionary<string, IOpenApiSecurityScheme> {
          ["Bearer"] = new OpenApiSecurityScheme() {
              Type = SecuritySchemeType.Http,
              Scheme = "bearer", // "bearer" refers to the header name here
              In = ParameterLocation.Header,
              BearerFormat = "Token"
          }
      };
      document.Components ??= new OpenApiComponents();
      document.Components.SecuritySchemes = requirements;


      // Apply it as a requirement for all operations
      var apiOperations = document.Paths
        .Where(pathItem => pathItem.Key.StartsWith("/api"))
        .SelectMany(pathItem => pathItem.Value.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>());

      foreach (var operation in apiOperations) {
        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement {
          [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
      }
    }
  }
}
