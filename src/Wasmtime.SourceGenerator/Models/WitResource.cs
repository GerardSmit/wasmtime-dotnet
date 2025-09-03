namespace Wasmtime.SourceGenerator.Models;

public record WitResource(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitResourceConstructor> Constructors,
    EquatableArray<WitField> Fields
) : WitTypeDef
{
    public WitType Type { get; } = new WitResourceType(Package, Name, Fields);
}

public record WitResourceConstructor(
    EquatableArray<WitFuncParameter> Parameters
);