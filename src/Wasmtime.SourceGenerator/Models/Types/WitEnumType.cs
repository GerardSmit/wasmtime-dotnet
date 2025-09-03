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
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToEnum<");
        WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }
}