// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Protocol;

namespace Bicep.LocalExtension.Setup.ExtensionHost.Helpers;

public static class RequestHelper
{

    public static async Task<LocalExtensionOperationResponse> HandleRequest(JsonObject? config, Func<HandleService, Task<LocalExtensionOperationResponse>> onExecuteFunc)
    {
       //Initialize the handle service here if it is necessary
       // For example, login to Azure DevOps with the PAT in the confituation
       //Example for GitHub
       //var credentials = new Credentials(config!["token"]!.GetValue<string>());
       //var client = new GitHubClient(new ProductHeaderValue("Bicep.LocalDeploy"), new InMemoryCredentialStore(credentials));

        try
        {
            return await onExecuteFunc(new HandleService()); //onExecuteFunc(client);
        }
        catch (Exception exception)
        {
            // Github example on how to return multiple errors
            // if (exception is ApiException apiException &&
            //     apiException.ApiError?.Errors is {} apiErrors)
            // {
            //     var errorDetails = apiErrors
            //         .Select(error => new ErrorDetail(error.Code, error.Field ?? "", error.Message)).ToArray();
            //
            //     return CreateErrorResponse("ApiError", apiException.ApiError.Message, errorDetails);
            // }

            return CreateErrorResponse("UnhandledError", exception.Message);
        }
    }

    public static TProperties GetProperties<TProperties>(JsonObject properties)
        => properties.Deserialize<TProperties>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

    public static LocalExtensionOperationResponse CreateSuccessResponse<TProperties, TIdentifiers>(ResourceReference request, TProperties properties, TIdentifiers identifiers)
    {
        return new(
            new(
                request.Type,
                request.ApiVersion,
                "Succeeded",
                (JsonNode.Parse(JsonSerializer.Serialize(identifiers, new JsonSerializerOptions(JsonSerializerDefaults.Web))) as JsonObject)!,
                request.Config,
                (JsonNode.Parse(JsonSerializer.Serialize(properties, new JsonSerializerOptions(JsonSerializerDefaults.Web))) as JsonObject)!),
            null);
    }

    public static LocalExtensionOperationResponse CreateSuccessResponse<TProperties, TIdentifiers>(ResourceSpecification request, TProperties properties, TIdentifiers identifiers)
    {
        return new(
            new(
                request.Type,
                request.ApiVersion,
                "Succeeded",
                (JsonNode.Parse(JsonSerializer.Serialize(identifiers, new JsonSerializerOptions(JsonSerializerDefaults.Web))) as JsonObject)!,
                request.Config,
                (JsonNode.Parse(JsonSerializer.Serialize(properties, new JsonSerializerOptions(JsonSerializerDefaults.Web))) as JsonObject)!),
            null);
    }

    public static LocalExtensionOperationResponse CreateErrorResponse(string code, string message, ErrorDetail[]? details = null, string? target = null)
    {
        return new LocalExtensionOperationResponse(
            null,
            new(new(code, target ?? "", message, details ?? [], [])));
    }
    
    public static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
        {
            return input;
        }

        return char.ToLower(input[0]) + input.Substring(1);
    }
    

    public static JsonNode? GetIdentifierData(ResourceReference reference, string propertyName) => reference.Identifiers[ToCamelCase(propertyName)];
}
