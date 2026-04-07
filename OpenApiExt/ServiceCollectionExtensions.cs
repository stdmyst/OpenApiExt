using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenApiExt.MvcConventions;

namespace OpenApiExt;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiMvcConventions(this IServiceCollection services)
        => services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new ControllerVisibleConvention());
            options.Conventions.Add(new AttributeRouteModelConvention());
        });
}