namespace Wasmtime.SourceGenerator.Models;

public record WitResourceType(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitField> Fields
) : WitType(WitTypeKind.Resource);