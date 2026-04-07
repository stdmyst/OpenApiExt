using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenApiExt.MvcConventions;

namespace OpenApiExt;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds conventions for controllers that do not use attribute routing (not decorated with <see cref="ApiControllerAttribute"/> and <see cref="RouteAttribute"/>).
    /// </summary>
    public static IServiceCollection AddOpenApiMvcConventions(this IServiceCollection services)
        => services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new ControllerVisibleConvention());
            options.Conventions.Add(new AttributeRouteModelConvention());
        });
}