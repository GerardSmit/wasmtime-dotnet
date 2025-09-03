namespace Wasmtime.SourceGenerator.Models;

public record WitInterfaceType(
    string Name,
    EquatableArray<WitField> Fields
) : WitType(WitTypeKind.Interface);
