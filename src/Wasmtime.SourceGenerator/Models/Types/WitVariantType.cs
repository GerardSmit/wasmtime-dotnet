namespace Wasmtime.SourceGenerator.Models;

public record WitVariantType(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitVariantCase> Values
) : WitType(WitTypeKind.Variant)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.Variant<");
        sb.Append("global::");
        Package.PackageName.WritePath(sb);
        sb.Append('.');
        sb.Append(ComponentSourceGenerator.GetName(Name));
        sb.Append('>');
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName,
        ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToVariant<");
        WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }

    /// <inheritdoc />
    protected override void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey,
        ITypeContainerResolver resolver, bool externallyOwned)
    {
        sb.Append("global::Wasmtime.ComponentValue.CreateVariant<");
        WriteCSharpType(sb, resolver);
        sb.Append(">(").Append(paramKey).Append(")");
    }
}