using Azure.Bicep.Types.Concrete;

namespace Bicep.LocalExtension.Setup.Shared;

/// <summary>
/// When used on a property. Means that the property will be exposed as a property of the bicep resource.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TypeAnnotationAttribute : Attribute
{
    public TypeAnnotationAttribute(
        string? description,
        ObjectTypePropertyFlags flags = ObjectTypePropertyFlags.None,
        bool isSecure = false)
    {
        Description = description;
        Flags = flags;
        IsSecure = isSecure;
    }

    public string? Description { get; }

    public ObjectTypePropertyFlags Flags { get; }

    public bool IsSecure { get; }
}

/// <summary>
/// When used on a class. Means that it will be generated as a child resource of the parent type provided.
/// Example: If the class is "Issue" and the parent is "Repository". The type will be generated with the name "Repository/Issue"
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BicepParentTypeAttribute : Attribute
{
    public BicepParentTypeAttribute(
        Type parentType)
    {
        ParentType = parentType;
    }

    public Type ParentType { get; }
}

/// <summary>
/// When used on a class. Means that it will be generated as a bicep resource with the same name as the class
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BicepSerializableType : Attribute
{
    public BicepSerializableType()
    {
    }

}

[AttributeUsage(AttributeTargets.Property)]
public class BicepSerializableProperty : Attribute
{
    public BicepSerializableProperty()
    {
    }

}