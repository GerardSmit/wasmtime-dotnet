namespace Wasmtime.SourceGenerator.Models;

public record WitResultNoResultType(
    WitType ErrorType
) : WitType(WitTypeKind.Result);