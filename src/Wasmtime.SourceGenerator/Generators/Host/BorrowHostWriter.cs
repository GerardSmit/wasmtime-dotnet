using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Generators.Host;

public class BorrowHostWriter(
    WitType elementType
) : HostWriter(WitTypeKind.Borrow)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.Borrow<");
        elementType.HostWriter.WriteCSharpType(sb, resolver);
        sb.Append('>');
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName,
        ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".ToBorrow<");
        elementType.HostWriter.WriteCSharpType(sb, resolver);
        sb.Append(">()");
    }
}
