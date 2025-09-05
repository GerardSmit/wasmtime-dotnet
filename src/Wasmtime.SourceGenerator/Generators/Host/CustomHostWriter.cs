using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Generators.Host;

public class CustomHostWriter(WitCustomType type) : HostWriter(WitTypeKind.User)
{
    private HostWriter Resolve(ITypeContainerResolver resolver)
    {
        return type.Resolve(resolver).HostWriter;
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
        bool externallyOwned)
    {
        Resolve(resolver).WriteParameterInitializer(sb, name, resolver, ignoreDispose, externallyOwned);
    }

    /// <inheritdoc />
    public override void WriteParameterSetter(IndentedStringBuilder sb, string parametersVariable, string name,
        int startIndex, bool ignoreDispose, ITypeContainerResolver resolver, bool externallyOwned)
    {
        Resolve(resolver).WriteParameterSetter(sb, parametersVariable, name, startIndex, ignoreDispose, resolver, externallyOwned);
    }

    public override void WriteBytes(IndentedStringBuilder sb, string name, string span, ITypeContainerResolver resolver)
    {
        Resolve(resolver).WriteBytes(sb, name, span, resolver);
    }

    public override void WriteComponentValue(IndentedStringBuilder sb, string name, bool ignoreDispose,
        ITypeContainerResolver resolver, bool externallyOwned)
    {
        Resolve(resolver).WriteComponentValue(sb, name, ignoreDispose, resolver, externallyOwned);
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
}
