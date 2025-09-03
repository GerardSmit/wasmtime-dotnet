namespace Wasmtime.SourceGenerator.Models;

public record WitResource(
    string Name,
    EquatableArray<WitResourceConstructor> Constructors,
    EquatableArray<WitField> Fields
) : WitTypeDef
{
    public WitType Type { get; } = new WitInterfaceType(Name, Fields);
}

public record WitResourceConstructor(
    EquatableArray<WitFuncParameter> Parameters
);