using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a user-defined type.
/// </summary>
/// <param name="Package">The package that requested this type. It's possible that the type is defined in the previous name parts.</param>
/// <param name="Name">The name of the type.</param>
public record WitCustomType(WitPackageNameVersion Package, string Name) : WitType(WitTypeKind.User)
{
    /// <summary>
    /// Gets the container that holds this type.
    /// </summary>
    /// <param name="resolver">The type resolver to use.</param>
    /// <param name="allowContainer">If the type was not found, allow returning a container with the same name. This is used for subtypes.</param>
    /// <param name="typeName"></param>
    /// <returns>The container that holds this type.</returns>
    protected internal virtual ITypeContainer GetContainer(
        ITypeContainerResolver resolver,
        bool allowContainer = false,
        string? typeName = null
    )
    {
        typeName ??= Name;

        var current = Package;

        while (current.PackageName.Name.Length > 0)
        {
            var container = resolver.Resolve(current);

            if ((
                    container.TryGetType(typeName, out var type)
                    // Nested aliases are not allowed if we're looking for a container
                    && (
                        !allowContainer
                        || type is not WitAliasType
                    )
                ) || (
                    allowContainer
                    && container.TryGetContainer(Name, out _)
                ))
            {
                return container;
            }

            // The current type was not found, try the previous name parts
            (_, current) = current.WithoutLastNamePart();
        }

        throw new InvalidOperationException($"Could not resolve type '{typeName}' in '{Package}'");
    }

    public WitType Resolve(ITypeContainerResolver resolver)
    {
        var container = GetContainer(resolver, allowContainer: false);

        if (!container.TryGetType(Name, out var type))
        {
            throw new InvalidOperationException($"Could not resolve type '{Name}' in '{Package}'");
        }

        return type;
    }

    public override TypeHostWriter HostWriter => new CustomHostWriter(this);

    public void Deconstruct(out WitPackageNameVersion Package, out string Name)
    {
        Package = this.Package;
        Name = this.Name;
    }
}