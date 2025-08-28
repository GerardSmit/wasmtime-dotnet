namespace Wasmtime.SourceGenerator;

public static class ParserExtensions
{
    public static string GetTextWithoutEscape(this WitParser.IdentifierContext context)
    {
        return context.GetText().TrimStart('%');
    }
}
