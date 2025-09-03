namespace Wasmtime.SourceGenerator.Models;

public record WitEnum(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<string> Values
) : WitTypeDef
{
    public WitType Type { get; } = new WitEnumType(Package, Name, Values);
}