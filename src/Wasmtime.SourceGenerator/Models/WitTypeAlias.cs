namespace Wasmtime.SourceGenerator.Models;

public record WitTypeAlias(
    string Name,
    WitType Type
) : WitTypeDef;