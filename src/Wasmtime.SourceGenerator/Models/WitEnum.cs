namespace Wasmtime.SourceGenerator.Models;

public record WitEnum(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<string> Values
) : WitEnumBase(Package, Name, Values)
{
    public override WitType Type { get; } = new WitEnumType(Package, Name);
}


public record WitFlags(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<string> Values
) : WitEnumBase(Package, Name, Values)
{
    public override WitType Type { get; } = new WitFlagsType(Package, Name);
}

public abstract record WitEnumBase(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<string> Values
) : WitTypeDef
{
    public abstract WitType Type { get; }
}