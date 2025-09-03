using System.Text.RegularExpressions;

namespace Wasmtime.SourceGenerator;

public static class ParserExtensions
{
    public static string GetTextWithoutEscape(this WitParser.IdentifierContext context)
    {
        return context.GetText().TrimStart('%');
    }

    public static string ToSafeVariable(this string name)
    {
        return name.Replace("-", "_").Replace(".", "_").Replace('[', '_').Replace("]", "");
    }
}
