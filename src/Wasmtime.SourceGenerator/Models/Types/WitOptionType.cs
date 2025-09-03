namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents an option type in WIT.
/// </summary>
public record WitOptionType(
    WitType ElementType
) : WitType(WitTypeKind.Option)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.Option<");
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append('>');
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToOption<");
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }
}