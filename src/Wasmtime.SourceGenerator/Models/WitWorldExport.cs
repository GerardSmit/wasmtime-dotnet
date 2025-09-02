namespace Wasmtime.SourceGenerator.Models;

public record WitWorldExport(string ExportName, WitType Type) : WitWorldItem;

public record WitWorldImport(string ImportName, WitType Type) : WitWorldItem;