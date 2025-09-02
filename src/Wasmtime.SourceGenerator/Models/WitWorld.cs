using System.Diagnostics.CodeAnalysis;

namespace Wasmtime.SourceGenerator.Models;

public record WitWorld(
    string Name,
    WitTypeDefinitions Definitions
) : ITypeContainer
{
    public bool TryGetType(string name, [NotNullWhen(true)] out WitType? type)
    {
        return Definitions.TryGetType(name, out type);
    }

    public bool TryGetContainer(string name, [NotNullWhen(true)] out ITypeContainer? container)
    {
        container = null;
        return false;
    }
}