namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a user-defined type.
/// </summary>
public record WitCustomType : WitType
{
    /// <summary>
    /// Represents a user-defined type.
    /// </summary>
    /// <param name="Package">The package that requested this type. It's possible that the type is defined in the previous name parts.</param>
    /// <param name="Name">The name of the type.</param>
    public WitCustomType(WitPackageNameVersion Package, string Name) : base(WitTypeKind.User)
    {
        this.Package = Package;
        this.Name = Name;
    }

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
    public override void WriteParameterInitializer(IndentedStringBuilder sb, string name,
        ITypeContainerResolver resolver,
        bool ignoreDispose,
        bool isMemoryInitializer)
    {
        Resolve(resolver).WriteParameterInitializer(sb, name, resolver, ignoreDispose, isMemoryInitializer);
    }

    /// <inheritdoc />
    public override void WriteParameterSetter(IndentedStringBuilder sb, string parametersVariable, string name, int startIndex, bool ignoreDispose, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteParameterSetter(sb, parametersVariable, name, startIndex, ignoreDispose, resolver);
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

    public override void WriteComponentValue(IndentedStringBuilder sb, string name, bool ignoreDispose, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteComponentValue(sb, name, ignoreDispose, resolver);
    }

    public override void WriteValueGetterInitializer(IndentedStringBuilder sb, string paramName, string uniqueName,
        ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteValueGetterInitializer(sb, paramName, uniqueName, resolver);
    }

    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteValueGetter(sb, paramName, uniqueName, resolver);
    }

    /// <summary>The package that requested this type. It's possible that the type is defined in the previous name parts.</summary>
    public WitPackageNameVersion Package { get; init; }

    /// <summary>The name of the type.</summary>
    public string Name { get; init; }

    public void Deconstruct(out WitPackageNameVersion Package, out string Name)
    {
        Package = this.Package;
        Name = this.Name;
    }
}