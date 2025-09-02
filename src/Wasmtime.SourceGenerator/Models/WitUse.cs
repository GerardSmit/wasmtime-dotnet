namespace Wasmtime.SourceGenerator.Models;

public record WitUse(
    WitPackageNameVersion Package,
    string Interface,
    EquatableArray<WitUseItem> Items
) : WitTypeDef;