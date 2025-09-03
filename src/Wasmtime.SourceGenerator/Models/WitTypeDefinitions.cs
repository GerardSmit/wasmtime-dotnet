using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Wasmtime.SourceGenerator.Models;

public record WitTypeDefinitions(EquatableArray<WitTypeDef> Items)
{
    private Dictionary<string, WitType>? _types;
    private Dictionary<string, ITypeContainer>? _typeContainers;

    public ImmutableArray<T> FindAll<T>(ITypeContainerResolver resolver)
        where T : WitTypeDef
    {
        var builder = ImmutableArray.CreateBuilder<T>();
        FindAll(Items, resolver, builder);
        return builder.ToImmutable();
    }

    private static void FindAll<T>(EquatableArray<WitTypeDef> items, ITypeContainerResolver resolver, ImmutableArray<T>.Builder builder)
        where T : WitTypeDef
    {
        foreach (var item in items)
        {
            if (item is T t)
            {
                builder.Add(t);
            }

            if (item is WitWorldInclude include &&
                resolver.Resolve(include.Package) is WitPackageVersion version &&
                version.Worlds.TryGetValue(include.WorldName, out var world))
            {
                FindAll(world.Definitions.Items, resolver, builder);
            }
        }
    }

    public bool TryGetType(string name, [NotNullWhen(true)] out WitType? type)
    {
        _types ??= BuildTypeDictionary(Items);

        return _types.TryGetValue(name, out type);
    }

    public bool TryGetContainer(string name, [NotNullWhen(true)] out ITypeContainer? container)
    {
        _typeContainers ??= BuildTypeContainerDictionary(Items);

        return _typeContainers.TryGetValue(name, out container);
    }

    private static Dictionary<string, WitType> BuildTypeDictionary(EquatableArray<WitTypeDef> items)
    {
        var dict = new Dictionary<string, WitType>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            switch (item)
            {
                case WitRecord record:
                    dict[record.Name] = record.Type;
                    break;
                case WitInterface interf:
                    dict[interf.Name] = interf.Type;
                    break;
                case WitEnum @enum:
                    dict[@enum.Name] = @enum.Type;
                    break;
                case WitFlags flags:
                    dict[flags.Name] = flags.Type;
                    break;
                case WitTypeAlias typeAlias:
                    dict[typeAlias.Name] = typeAlias.Type;
                    break;
                case WitVariant variant:
                    dict[variant.Name] = variant.Type;
                    break;
                case WitResource resource:
                    dict[resource.Name] = resource.Type;
                    break;
                case WitUse use:
                {
                    var container = new WitCustomType(use.Package, use.Interface);

                    foreach (var (name, alias) in use.Items)
                    {
                        dict[alias] = new WitAliasType(container, name);
                    }

                    break;
                }
            }
        }

        return dict;
    }

    private static Dictionary<string, ITypeContainer> BuildTypeContainerDictionary(EquatableArray<WitTypeDef> items)
    {
        var dict = new Dictionary<string, ITypeContainer>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            if (item is WitInterface interf)
            {
                dict[interf.Name] = interf;
            }
        }

        return dict;
    }
}
