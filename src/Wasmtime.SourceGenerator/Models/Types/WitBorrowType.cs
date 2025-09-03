namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a list type in WIT.
/// </summary>
public record WitBorrowType(
    WitType ElementType
) : WitType(WitTypeKind.Borrow)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.Borrow<");
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append('>');
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName,
        ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToBorrow<");
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }
}