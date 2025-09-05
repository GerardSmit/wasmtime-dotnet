using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a list type in WIT.
/// </summary>
public record WitBorrowType(
    WitType ElementType
) : WitType(WitTypeKind.Borrow)
{
    public override TypeHostWriter HostWriter => new BorrowHostWriter(ElementType);
}