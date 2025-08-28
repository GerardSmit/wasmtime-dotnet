namespace Wasmtime.SourceGenerator.Models;

public record WitListType(
    WitType ElementType
) : WitType(WitTypeKind.List)
{
    public override string GetCSharpType()
    {
        return ElementType.GetCSharpType() + "[]";
    }

    public override void WriteCSharpType(IndentedStringBuilder sb)
    {
        ElementType.WriteCSharpType(sb);
        sb.Append("[]");
    }
}