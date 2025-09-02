namespace Wasmtime.SourceGenerator.Models;

public record WitWorld(
    string Name,
    EquatableArray<WitTypeDef> Items
);

public record WitTypeDef;

public record WitRecord(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitField> Fields) : WitTypeDef
{
    public WitType Type { get; } = new WitRecordType(Package, Name, Fields);
}

public record WitWorldItem : WitTypeDef;

public record WitWorldExport(string ExportName, WitType Type) : WitWorldItem;

public record WitWorldInclude(WitPackageNameVersion Package, string WorldName) : WitWorldItem;
