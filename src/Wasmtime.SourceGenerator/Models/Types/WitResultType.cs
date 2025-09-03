namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a result type in WIT.
/// </summary>
public record WitResultType(
    WitType OkType,
    WitType ErrType
) : WitType(WitTypeKind.Result)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.Result<");
        OkType.WriteCSharpType(sb, resolver);
        sb.Append(", ");
        ErrType.WriteCSharpType(sb, resolver);
        sb.Append('>');
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName,
        ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToResult<");
        OkType.WriteCSharpType(sb, resolver);
        sb.Append(", ");
        ErrType.WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }
}