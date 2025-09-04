namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a tuple type in WIT.
/// </summary>
public record WitTupleType(
    EquatableArray<WitType> ElementTypes
) : WitType(WitTypeKind.Tuple)
{
    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        if (ElementTypes.Length == 0)
        {
            throw new InvalidOperationException("Tuples with zero elements are not supported.");
        }

        sb.Append('(');
        for (var i = 0; i < ElementTypes.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            ElementTypes[i].WriteCSharpType(sb, resolver);
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

        for (var i = 0; i < ElementTypes.Length; i++)
        {
            sb.AppendLine(i > 0 ? ", " : "");
            ElementTypes[i].WriteResultGetter(sb, uniqueName, i, resolver);
        }

        sb.DecrementIndent();
        sb.AppendLine();
        sb.Append(')');
    }

    /// <inheritdoc />
    protected override void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey,
        ITypeContainerResolver resolver, bool copyConstants)
    {
        sb.Append("default");
    }
}