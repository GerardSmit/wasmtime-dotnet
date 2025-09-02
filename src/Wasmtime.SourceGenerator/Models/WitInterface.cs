using System.Diagnostics.CodeAnalysis;

namespace Wasmtime.SourceGenerator.Models;

public record WitInterface(
    string Name,
    WitTypeDefinitions Definitions,
    EquatableArray<WitField> Fields
) : WitTypeDef, ITypeContainer
{
    public WitType Type { get; } = new WitInterfaceType(Name, Fields);

    public bool TryGetType(string name, [NotNullWhen(true)] out WitType? type)
    {
        return Definitions.TryGetType(name, out type);
    }

    public bool TryGetContainer(string name, [NotNullWhen(true)] out ITypeContainer? container)
    {
        return Definitions.TryGetContainer(name, out container);
    }
}