namespace Wasmtime.SourceGenerator.Models;

public record WitRecord(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitField> Fields) : WitTypeDef
{
    public WitType Type { get; } = new WitRecordType(Package, Name, Fields);
}