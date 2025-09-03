namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a list type in WIT.
/// </summary>
public record WitBorrowType(
    WitType ElementType
) : WitType(WitTypeKind.List);