using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiExt._Internals.Extensions;

namespace OpenApiExt._Internals.Transformers;

internal class ResponsesAuthTransformer(bool showRequiredRoles = false) : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var authorizeAttribute = metadata.OfType<AuthorizeAttribute>()
            .FirstOrDefault();

        if (authorizeAttribute is not null)
        {
            SetResponse(operation, ("401", new OpenApiResponse { Description = "Unauthorized" }));
        }

        var requiredRoles = GetRequiredRoles(metadata, authorizeAttribute);
        if (requiredRoles?.Count > 0)
        {
            var response = new OpenApiResponse
            {
                Description = showRequiredRoles 
                    ? $"Forbidden. Allowed for roles: {string.Join(", ", requiredRoles)}" 
                    : "Forbidden"
            };
            SetResponse(operation, ("403", response));
        }

        return Task.CompletedTask;
    }
    
    private List<string>? GetRequiredRoles(IList<object> metadata, AuthorizeAttribute? authorizeAttribute)
    {
        List<string>? roles = null;
        
        if (!string.IsNullOrWhiteSpace(authorizeAttribute?.Roles))
        {
            roles =  authorizeAttribute.Roles.Split(',').ToList();
        }
        else
        {
            var authPolicy = metadata.OfType<AuthorizationPolicy>().FirstOrDefault();
            var authRequirement = authPolicy?.Requirements.FirstOrDefault() as RolesAuthorizationRequirement;
            if (authRequirement?.AllowedRoles is not null)
                roles = authRequirement.AllowedRoles.ToList();
        }

        return roles;
    }

    private void SetResponse(OpenApiOperation operation, (string Key, OpenApiResponse Value) response)
    {
        operation.Responses ??= new OpenApiResponses();
        operation.Responses.AddOrUpdate(response.Key, response.Value);
    }
}