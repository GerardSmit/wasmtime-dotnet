using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a list type in WIT.
/// </summary>
public record WitListType(
    WitType ElementType
) : WitType(WitTypeKind.List)
{
    public override HostWriter HostWriter => new HostListWriter(ElementType);
}