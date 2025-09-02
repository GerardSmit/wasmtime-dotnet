using System.Diagnostics.CodeAnalysis;

namespace Wasmtime.SourceGenerator.Models;

public record WitTypeDefinitions(EquatableArray<WitTypeDef> Items)
{
    private Dictionary<string, WitType>? _types;
    private Dictionary<string, ITypeContainer>? _typeContainers;

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
            if (item is WitRecord record)
            {
                dict[record.Name] = record.Type;
            }

            if (item is WitInterface interf)
            {
                dict[interf.Name] = interf.Type;
            }

            if (item is WitUse use)
            {
                var container = new WitCustomType(use.Package, use.Interface);

                foreach (var (name, alias) in use.Items)
                {
                    dict[alias] = new WitStrictCustomType(container, name);
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
