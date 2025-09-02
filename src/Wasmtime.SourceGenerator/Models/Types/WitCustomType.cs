namespace Wasmtime.SourceGenerator.Models;

public record WitCustomType(string Name) : WitType(WitTypeKind.User)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, WorldTypeResolver resolver)
    {
        resolver.Resolve(Name).WriteCSharpType(sb, resolver);
    }

    /// <inheritdoc />
    public override void WriteResultGetter(IndentedStringBuilder sb, string paramName, int index, WorldTypeResolver resolver)
    {
        resolver.Resolve(Name).WriteResultGetter(sb, paramName, index, resolver);
    }

    /// <inheritdoc />
    public override int GetParameterSize(WorldTypeResolver resolver)
    {
        return resolver.Resolve(Name).GetParameterSize(resolver);
    }

    /// <inheritdoc />
    public override void WriteParameter(IndentedStringBuilder sb, string name, WorldTypeResolver resolver)
    {
        resolver.Resolve(Name).WriteParameter(sb, name, resolver);
    }

    /// <inheritdoc />
    public override void WriteParameterInitializer(IndentedStringBuilder sb, string name, WorldTypeResolver resolver,
        bool isMemoryInitializer)
    {
        resolver.Resolve(Name).WriteParameterInitializer(sb, name, resolver, isMemoryInitializer);
    }

    /// <inheritdoc />
    public override void WriteParameterSetter(IndentedStringBuilder sb, string parametersVariable, string name, int startIndex, WorldTypeResolver resolver)
    {
        resolver.Resolve(Name).WriteParameterSetter(sb, parametersVariable, name, startIndex, resolver);
    }

    public override string GetCSharpType(WorldTypeResolver resolver)
    {
        return resolver.Resolve(Name).GetCSharpType(resolver);
    }

    public override void WriteBytes(IndentedStringBuilder sb, string name, string span, WorldTypeResolver resolver)
    {
        resolver.Resolve(Name).WriteBytes(sb, name, span, resolver);
    }

    public override int GetMemorySize(WorldTypeResolver resolver)
    {
        return resolver.Resolve(Name).GetMemorySize(resolver);
    }

    public override void WriteComponentValue(IndentedStringBuilder sb, string name, WorldTypeResolver resolver)
    {
        resolver.Resolve(Name).WriteComponentValue(sb, name, resolver);
    }

    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, WorldTypeResolver resolver)
    {
        resolver.Resolve(Name).WriteValueGetter(sb, paramName, resolver);
    }
}