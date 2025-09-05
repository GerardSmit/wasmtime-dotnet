namespace Wasmtime.SourceGenerator.Models;

public record WitRecord(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitField> Fields) : WitTypeDef
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public WitType Type { get; } = new WitRecordType(Package, Name, Fields);
}