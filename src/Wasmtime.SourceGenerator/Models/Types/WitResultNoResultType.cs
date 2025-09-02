namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a result type in WIT with no result type.
/// </summary>
public record WitResultNoResultType(
    WitType ErrorType
) : WitType(WitTypeKind.Result);