namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a stream type in WIT.
/// </summary>
public record WitStreamType(
    WitType ElementType
) : WitType(WitTypeKind.Stream);