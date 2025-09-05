namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a result type in WIT with no error type.
/// </summary>
public record WitResultNoErrorType(
    WitType OkType
) : WitType(WitTypeKind.Result);