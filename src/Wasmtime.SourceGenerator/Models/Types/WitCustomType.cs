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
    /// <returns>The container that holds this type.</returns>
    protected internal virtual ITypeContainer GetContainer(ITypeContainerResolver resolver, bool allowContainer = false)
    {
        var current = Package;

        while (current.PackageName.Name.Length > 0)
        {
            var container = resolver.Resolve(current);

            if (container.TryGetType(Name, out _) || (allowContainer && container.TryGetContainer(Name, out _)))
            {
                return container;
            }

            // The current type was not found, try the previous name parts
            (_, current) = current.WithoutLastNamePart();
        }

        throw new InvalidOperationException($"Could not resolve type '{Name}' in '{Package}'");
    }

    private WitType Resolve(ITypeContainerResolver resolver)
    {
        var container = GetContainer(resolver, allowContainer: false);

        if (!container.TryGetType(Name, out var type))
        {
            throw new InvalidOperationException($"Could not resolve type '{Name}' in '{Package}'");
        }

        return type;
    }

    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteCSharpType(sb, resolver);
    }

    /// <inheritdoc />
    public override void WriteResultGetter(IndentedStringBuilder sb, string paramName, int index, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteResultGetter(sb, paramName, index, resolver);
    }

    /// <inheritdoc />
    public override int GetParameterSize(ITypeContainerResolver resolver)
    {
        return Resolve(resolver).GetParameterSize(resolver);
    }

    /// <inheritdoc />
    public override void WriteParameter(IndentedStringBuilder sb, string name, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteParameter(sb, name, resolver);
    }

    /// <inheritdoc />
    public override void WriteParameterInitializer(IndentedStringBuilder sb, string name, ITypeContainerResolver resolver,
        bool isMemoryInitializer)
    {
        Resolve(resolver).WriteParameterInitializer(sb, name, resolver, isMemoryInitializer);
    }

    /// <inheritdoc />
    public override void WriteParameterSetter(IndentedStringBuilder sb, string parametersVariable, string name, int startIndex, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteParameterSetter(sb, parametersVariable, name, startIndex, resolver);
    }

    public override string GetCSharpType(ITypeContainerResolver resolver)
    {
        return Resolve(resolver).GetCSharpType(resolver);
    }

    public override void WriteBytes(IndentedStringBuilder sb, string name, string span, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteBytes(sb, name, span, resolver);
    }

    public override int GetMemorySize(ITypeContainerResolver resolver)
    {
        return Resolve(resolver).GetMemorySize(resolver);
    }

    public override void WriteComponentValue(IndentedStringBuilder sb, string name, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteComponentValue(sb, name, resolver);
    }

    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteValueGetter(sb, paramName, resolver);
    }
}