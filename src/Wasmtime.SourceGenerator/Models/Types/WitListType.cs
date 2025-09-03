namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a list type in WIT.
/// </summary>
public record WitListType(
    WitType ElementType
) : WitType(WitTypeKind.List)
{
    /// <inheritdoc />
    public override string GetCSharpType(ITypeContainerResolver resolver)
    {
        return ElementType.GetCSharpType(resolver) + "[]";
    }

    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append("[]");
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToList<");
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }
}