using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Generators.Host;

public class TupleHostWriter(EquatableArray<WitType> elementTypes) : HostWriter(WitTypeKind.Tuple)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        if (elementTypes.Length == 0)
        {
            throw new InvalidOperationException("Tuples with zero elements are not supported.");
        }

        sb.Append('(');
        for (var i = 0; i < elementTypes.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            elementTypes[i].HostWriter.WriteCSharpType(sb, resolver);
        }
        sb.Append(')');
    }

    /// <inheritdoc />
    public override void WriteValueGetterInitializer(IndentedStringBuilder sb, string paramName, string uniqueName, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.ComponentCallResults ").Append(uniqueName).Append(" = ").Append(paramName).AppendLine(".ToTuple();");
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName, ITypeContainerResolver resolver)
    {
        sb.Append('(');
        sb.IncrementIndent();

        for (var i = 0; i < elementTypes.Length; i++)
        {
            sb.AppendLine(i > 0 ? ", " : "");
            elementTypes[i].HostWriter.WriteResultGetter(sb, uniqueName, i, resolver);
        }

        sb.DecrementIndent();
        sb.AppendLine();
        sb.Append(')');
    }

    /// <inheritdoc />
    protected override void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey,
        ITypeContainerResolver resolver, bool externallyOwned)
    {
        sb.Append("default");
    }
}
