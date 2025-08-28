namespace Wasmtime.SourceGenerator.Models;

public record WitTupleType(
    EquatableArray<WitType> ElementTypes
) : WitType(WitTypeKind.Tuple)
{
    public override string GetCSharpType()
    {
        if (ElementTypes.Length == 0)
        {
            throw new InvalidOperationException("Tuples with zero elements are not supported.");
        }

        return "(" + string.Join(", ", ElementTypes.Select(t => t.GetCSharpType())) + ")";
    }

    public override void WriteCSharpType(IndentedStringBuilder sb)
    {
        if (ElementTypes.Length == 0)
        {
            throw new InvalidOperationException("Tuples with zero elements are not supported.");
        }

        sb.Append('(');
        for (var i = 0; i < ElementTypes.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            ElementTypes[i].WriteCSharpType(sb);
        }
        sb.Append(')');
    }
}