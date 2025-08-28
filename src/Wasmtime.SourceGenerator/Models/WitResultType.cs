namespace Wasmtime.SourceGenerator.Models;

public record WitResultType(
    WitType OkType,
    WitType ErrType
) : WitType(WitTypeKind.Result);