using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a tuple type in WIT.
/// </summary>
public record WitTupleType(
    EquatableArray<WitType> ElementTypes
) : WitType(WitTypeKind.Tuple)
{
    public override TypeHostWriter HostWriter => new TupleHostWriter(ElementTypes);
}