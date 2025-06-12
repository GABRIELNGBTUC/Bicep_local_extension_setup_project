using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Azure.Bicep.Types;
using Azure.Bicep.Types.Concrete;
using Azure.Bicep.Types.Index;
using Azure.Bicep.Types.Serialization;
using Bicep.LocalExtension.Setup.Generator;
using Bicep.LocalExtension.Setup.Shared;

namespace Bicep.LocalExtension.Setup.Generator;

public static class TypeGenerator
{
    internal static string CamelCase(string input)
        => $"{input[..1].ToLowerInvariant()}{input[1..]}";

    internal static TypeBase GenerateForRecord(TypeFactory factory, ConcurrentDictionary<Type, TypeBase> typeCache, Type type)
    {
        var typeProperties = new Dictionary<string, ObjectTypeProperty>();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var annotation = property.GetCustomAttributes<TypeAnnotationAttribute>(true).FirstOrDefault();
            var propertyType = property.PropertyType;
            TypeBase typeReference;

            if (propertyType == typeof(string) && annotation?.IsSecure == true)
            {
                typeReference = factory.Create(() => new StringType(sensitive: true));
            }
            else if (propertyType == typeof(string))
            {
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => new StringType()));
            }
            else if (propertyType == typeof(bool))
            {
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => new BooleanType()));
            }
            else if (propertyType == typeof(int))
            {
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => new IntegerType()));
            }
            else if (propertyType.IsClass)
            {
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => GenerateForRecord(factory, typeCache, propertyType)));
            }
            else if (propertyType.IsGenericType &&
                propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                propertyType.GetGenericArguments()[0] is { IsEnum: true } enumType)
            {
                var enumMembers = enumType.GetEnumNames()
                    .Select(x => factory.Create(() => new StringLiteralType(x)))
                    .Select(x => factory.GetReference(x))
                    .ToImmutableArray();

                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => new UnionType(enumMembers)));
            }
            else
            {
                throw new NotImplementedException($"Unsupported property type {propertyType}");
            }

            typeProperties[CamelCase(property.Name)] = new ObjectTypeProperty(
                factory.GetReference(typeReference),
                annotation?.Flags ?? ObjectTypePropertyFlags.None,
                annotation?.Description);
        }

        return new ObjectType(
            type.Name,
            typeProperties,
            null);
    }

    internal static ResourceType GenerateResource(TypeFactory factory, ConcurrentDictionary<Type, TypeBase> typeCache, Type type)
    {
        var realName = type.Name;
        Type? parentType = type;
        do
        {
            parentType = parentType.GetCustomAttribute<BicepParentTypeAttribute>(true)?.ParentType;
            if(parentType is not null)
            {
                realName = $"{parentType!.Name}/{realName}";
            }
        } while (type.GetCustomAttribute<BicepParentTypeAttribute>(true)?.ParentType is not null &&
                 parentType != null);
        return factory.Create(() => new ResourceType(
            realName,
            ScopeType.Unknown,
            null,
            factory.GetReference(factory.Create(() => GenerateForRecord(factory, typeCache, type))),
            ResourceFlags.None,
            null));
    }

    internal static string GetString(Action<Stream> streamWriteFunc)
    {
        using var memoryStream = new MemoryStream();
        streamWriteFunc(memoryStream);

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    internal static ITypeReference GetReferenceFromType(TypeFactory factory, TypeBase type)
    {
        try
        {
            var typeReference = factory.Create(() => type);
            return factory.GetReference(typeReference);
        }
        catch (ArgumentException ex)
        {
            
        }
        return factory.GetReference(type);
    }

    [RequiresUnreferencedCode("Retrieves the valid bicep types from the current assembly")]
    public static Dictionary<string, string> GenerateTypes(string extensionName, string version, ExtensionConfiguration configuration, Type? sourceAssembly = null)
    {
        var factory = new TypeFactory([]);
        var secureStringType = factory.Create(() => new StringType(sensitive: true));

        var configurationType = factory.Create(() => new ObjectType("configuration", 
            configuration._properties.ToDictionary(p => p.Name,
            p => new ObjectTypeProperty(p.GetBicepTypeReference(ref factory), p.Flags, p.Description)),
            null));

        var settings = new TypeSettings(
            name: extensionName,
            version: version,
            isSingleton: true,
            configurationType: new CrossFileTypeReference("types.json", factory.GetIndex(configurationType)));

        var typeCache = new ConcurrentDictionary<Type, TypeBase>();
        var serializableTypes =
            sourceAssembly is null ?
            Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<BicepSerializableType>() != null)
            : Assembly.GetAssembly(sourceAssembly).GetTypes().Where(t => t.GetCustomAttribute<BicepSerializableType>() != null);
        var resourceTypes = serializableTypes.Select(type => GenerateResource(factory, typeCache, type));

        var index = new TypeIndex(
            resourceTypes.ToDictionary(x => x.Name, x => new CrossFileTypeReference("types.json", factory.GetIndex(x))),
            new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<CrossFileTypeReference>>>(),
            settings,
            null);

        return new Dictionary<string, string>{
            ["index.json"] = GetString(stream => TypeSerializer.SerializeIndex(stream, index)),
            ["types.json"] = GetString(stream => TypeSerializer.Serialize(stream, factory.GetTypes())),
        };
    }
    
    public static void CopyTypesToDirectory(string extensionName, string version, ExtensionConfiguration configuration, string outDir, Type? sourceAssembly = null)
    {
        var result = GenerateTypes(extensionName, version, configuration, sourceAssembly);
        foreach (var kvp in result)
        {
            var filePath = Path.Combine(outDir, kvp.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            File.WriteAllText(filePath, kvp.Value);
        }
    }
}