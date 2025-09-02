namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// A custom type that is a strict subtype of another custom type.
/// This is used for imported interfaces via `use` statements in WIT.
/// </summary>
/// <param name="Parent">The parent custom type.</param>
/// <param name="Name">The name of the subtype.</param>
public record WitStrictCustomType(WitCustomType Parent, string Name) : WitCustomType(Parent.Package, Name)
{
    protected internal override ITypeContainer GetContainer(ITypeContainerResolver resolver, bool allowContainer = false)
    {
        var parentContainer = Parent.GetContainer(resolver, allowContainer: true);

        if (!parentContainer.TryGetContainer(Parent.Name, out var container))
        {
            throw new InvalidOperationException($"Could not resolve subtype '{Name}' in '{Parent.Name}'");
        }

        return container;
    }
}
