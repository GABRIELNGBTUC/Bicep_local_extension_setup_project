using System.Text.Json;
using Bicep.Local.Extension.Protocol;
using Bicep.LocalExtension.Setup.ExtensionHost.Helpers;
using Bicep.LocalExtension.Setup.Shared.Models;

namespace Bicep.LocalExtension.Setup.ExtensionHost.Handlers;

public class Class1Handler : IResourceHandler
{
    public string ResourceType => nameof(Class1);
    
    private record Identifiers(
        string? Name,
        string? Prop1);

    public Task<LocalExtensionOperationResponse> CreateOrUpdate(ResourceSpecification request,
        CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client =>
        {
            Console.WriteLine(JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            var properties = RequestHelper.GetProperties<Class1>(request.Properties);
            
            try
            {
                // Get existing resource

                // Create or Update resource
            }
            //If not found exception
            catch (Exception)
            {
                // Create 
            }

            return RequestHelper.CreateSuccessResponse(request, properties,
                new Identifiers(properties.Name, properties.Prop1));
        });

    // Corresponds to existing
    public Task<LocalExtensionOperationResponse> Preview(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client => {
            var properties = RequestHelper.GetProperties<Class1>(request.Properties);
            
            
            
            await Task.Yield();

            // Remove any property that is not needed in the response

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Name, properties.Prop1));
        });

    public Task<LocalExtensionOperationResponse> Get(ResourceReference request, CancellationToken cancellationToken)
    {
        //Get identifiers
        var name = RequestHelper.GetIdentifierData(request, nameof(Class1.Name))?.ToString();

        throw new NotImplementedException();
    }

    public Task<LocalExtensionOperationResponse> Delete(ResourceReference request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}