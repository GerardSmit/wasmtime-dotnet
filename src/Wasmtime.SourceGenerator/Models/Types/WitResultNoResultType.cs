namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a result type in WIT with no result type.
/// </summary>
public record WitResultNoResultType(
    WitType ErrType
) : WitType(WitTypeKind.Result)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.Result<");
        sb.Append("global::Wasmtime.Unit");
        sb.Append(", ");
        ErrType.WriteCSharpType(sb, resolver);
        sb.Append('>');
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName,
        ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToResult<");
        sb.Append("global::Wasmtime.Unit");
        sb.Append(", ");
        ErrType.WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }
}