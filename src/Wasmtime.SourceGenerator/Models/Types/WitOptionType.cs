using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents an option type in WIT.
/// </summary>
public record WitOptionType(
    WitType ElementType
) : WitType(WitTypeKind.Option)
{
    public override TypeHostWriter HostWriter => new OptionHostWriter(ElementType);
}