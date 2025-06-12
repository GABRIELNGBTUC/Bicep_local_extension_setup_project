using Azure.Bicep.Types.Concrete;

namespace Bicep.LocalExtension.Setup.Shared.Models;


[BicepSerializableType]
public class Class1
{
    [TypeAnnotation("The name of the resource", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Name { get; set; }

    [TypeAnnotation("First property", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Prop1 { get; set; }
}