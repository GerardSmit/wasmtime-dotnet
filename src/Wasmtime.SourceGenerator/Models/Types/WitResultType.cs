namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a result type in WIT.
/// </summary>
public record WitResultType(
    WitType OkType,
    WitType ErrType
) : WitType(WitTypeKind.Result);