namespace Wasmtime.SourceGenerator.Models;

public record WitEnum(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitEnumValue> Values
) : WitEnumBase(Package, Name, Values)
{
    public override WitType Type { get; } = new WitEnumType(Package, Name);
}


public record WitFlags(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitEnumValue> Values
) : WitEnumBase(Package, Name, Values)
{
    public override WitType Type { get; } = new WitFlagsType(Package, Name);
}

public abstract record WitEnumBase(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitEnumValue> Values
) : WitTypeDef
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public abstract WitType Type { get; }
}

public record struct WitEnumValue(string Name)
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public override string ToString()
    {
        return Name;
    }
}