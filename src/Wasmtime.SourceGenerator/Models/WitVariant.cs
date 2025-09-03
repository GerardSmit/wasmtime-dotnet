namespace Wasmtime.SourceGenerator.Models;

public record WitVariant(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitVariantCase> Cases
) : WitTypeDef
{
    public WitType Type => new WitVariantType(Package, Name, Cases);
}

public record WitVariantCase(
    string Name,
    WitType? Type
);