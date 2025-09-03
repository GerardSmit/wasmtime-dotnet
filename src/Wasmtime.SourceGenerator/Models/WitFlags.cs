namespace Wasmtime.SourceGenerator.Models;

public record WitFlags(
    string Name,
    EquatableArray<string> Values
) : WitTypeDef;