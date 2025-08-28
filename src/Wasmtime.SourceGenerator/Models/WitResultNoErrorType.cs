namespace Wasmtime.SourceGenerator.Models;

public record WitResultNoErrorType(
    WitType OkType
) : WitType(WitTypeKind.Result);