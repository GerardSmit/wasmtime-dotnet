namespace Wasmtime.SourceGenerator.Models;

public record WitResourceType(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitField> Fields
) : WitType(WitTypeKind.Resource)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::");
        Package.PackageName.WritePath(sb);
        sb.Append('.');
        sb.Append(ComponentSourceGenerator.GetName(Name));
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName,
        ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToResource<");
        WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }

    /// <inheritdoc />
    public override void WriteResultGetter(IndentedStringBuilder sb, string paramName, int index, ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".GetResource<");
        WriteCSharpType(sb, resolver);
        sb.Append(">(").Append(index).Append(')');
    }
}
