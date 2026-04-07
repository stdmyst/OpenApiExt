using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OpenApiExt.MvcConventions;

public class AttributeRouteModelConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (UsesAttributeRouting(controller)) 
                continue;
            
            foreach (var selectorModel in controller.Actions.SelectMany(a => a.Selectors)
                         .Where(s => s.AttributeRouteModel != null))
            {
                if (selectorModel.AttributeRouteModel is null) 
                    continue;
            
                selectorModel.AttributeRouteModel.Template ??= $"/{selectorModel.AttributeRouteModel.Name}";
            }
        }
    }
    
    private bool UsesAttributeRouting(ControllerModel controllerModel) 
        => controllerModel.Attributes.Any(attr => attr is ApiControllerAttribute or RouteAttribute);
}