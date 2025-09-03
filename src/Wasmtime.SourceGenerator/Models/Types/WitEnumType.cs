namespace Wasmtime.SourceGenerator.Models;

public record WitEnumType(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<string> Values
) : WitType(WitTypeKind.Enum)
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
        sb.Append(paramName).Append(".ToEnum<");
        WriteCSharpType(sb, resolver);
        sb.Append(">(&");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.FromByteVector)");
    }

    /// <inheritdoc />
    public override void WriteResultGetter(IndentedStringBuilder sb, string paramName, int index, ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".GetEnum<");
        WriteCSharpType(sb, resolver);
        sb.Append(">(").Append(index);
        sb.Append(", &");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.FromByteVector)");
    }

    /// <inheritdoc />
    protected override void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.ComponentValue.CreateEnum<");
        WriteCSharpType(sb, resolver);
        sb.Append(">(").Append(paramKey);
        sb.Append(", &");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.ToByteVector)");
    }

}