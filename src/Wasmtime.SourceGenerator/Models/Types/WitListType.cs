namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a list type in WIT.
/// </summary>
public record WitListType(
    WitType ElementType
) : WitType(WitTypeKind.List)
{
    public override string GetCSharpType(WorldTypeResolver resolver)
    {
        return ElementType.GetCSharpType(resolver) + "[]";
    }

    public override void WriteCSharpType(IndentedStringBuilder sb, WorldTypeResolver resolver)
    {
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append("[]");
    }
}