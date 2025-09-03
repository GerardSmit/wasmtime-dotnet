namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a tuple type in WIT.
/// </summary>
public record WitTupleType(
    EquatableArray<WitType> ElementTypes
) : WitType(WitTypeKind.Tuple)
{
    /// <param name="resolver"></param>
    /// <inheritdoc />
    public override string GetCSharpType(ITypeContainerResolver resolver)
    {
        if (ElementTypes.Length == 0)
        {
            throw new InvalidOperationException("Tuples with zero elements are not supported.");
        }

        return "(" + string.Join(", ", ElementTypes.Select(t => t.GetCSharpType(resolver))) + ")";
    }

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
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, ITypeContainerResolver resolver)
    {
        sb.Append('(');

        // TODO: Cache the tuple variable so it's only called once?
        var tuple = paramName + ".ToTuple()";

        for (var i = 0; i < ElementTypes.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            ElementTypes[i].WriteResultGetter(sb, tuple, i, resolver);
        }

        sb.Append(')');
    }
}