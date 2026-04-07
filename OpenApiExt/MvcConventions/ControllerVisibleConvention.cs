using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OpenApiExt.MvcConventions;

public class ControllerVisibleConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var mustIgnore = controller.Attributes
                .Any(attr => attr is ApiExplorerSettingsAttribute { IgnoreApi: true });
            
            if (mustIgnore) continue;
            
            controller.ApiExplorer.IsVisible = true;
        }
    }
}