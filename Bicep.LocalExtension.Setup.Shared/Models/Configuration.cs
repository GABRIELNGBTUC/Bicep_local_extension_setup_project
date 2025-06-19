using Azure.Bicep.Types.Concrete;

namespace Bicep.LocalExtension.Setup.Shared.Models;

public record Configuration(
    [property: TypeAnnotation("Authentication mode",
        ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    AuthenticationMode AuthenticationMode
    );

public enum AuthenticationMode
{
    ManagedIdentity,
    ServicePrincipal
}

//Example on how to make the configuration a discriminator type
[BicepDiscriminatorType("configuration", "authenticationMode",
    typeof(ManagedIdentityConfiguration),
    typeof(ServicePrincipalConfiguration))]
public record ConfigurationBase(
    [property: TypeAnnotation("Shared property between all configurations", ObjectTypePropertyFlags.None)]
    string sharedProperty);

public record ManagedIdentityConfiguration(
    string sharedProperty,
    [property: TypeAnnotation("Authentication mode", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required), 
               BicepStringLiteralValue(nameof(AuthenticationMode.ManagedIdentity))]
    AuthenticationMode authenticationMode) : ConfigurationBase(sharedProperty);

public record ServicePrincipalConfiguration(
    string sharedProperty,
    [property: TypeAnnotation("Authentication mode", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required), 
               BicepStringLiteralValue(nameof(AuthenticationMode.ServicePrincipal))]
    AuthenticationMode authenticationMode,
    [property: TypeAnnotation("Service principal id", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    string servicePrincipalId,
    [property: TypeAnnotation("Service principal secret", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required, true)]
    string servicePrincipalSecret
    ) : ConfigurationBase(sharedProperty);

