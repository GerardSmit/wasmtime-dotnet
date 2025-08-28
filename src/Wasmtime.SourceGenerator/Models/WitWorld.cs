namespace Wasmtime.SourceGenerator.Models;

public record WitWorld(
    string Name,
    EquatableDictionary<string, WitType> Exports
);
