using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a function type in WIT.
/// </summary>
public record WitRecordType(WitPackageNameVersion Package, string Name, EquatableArray<WitField> Fields) : WitType(WitTypeKind.Record)
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public override TypeHostWriter HostWriter => new RecordHostWriter(Package, CSharpName);
}

/// <summary>
/// Represents a function parameter in WIT.
/// </summary>
public readonly record struct WitField(
    string Name,
    WitType Type
)
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public string CSharpVariableName { get; } = StringUtils.GetName(Name, uppercaseFirst: false);
}