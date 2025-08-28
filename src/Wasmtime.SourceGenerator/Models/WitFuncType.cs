namespace Wasmtime.SourceGenerator.Models;

public record WitFuncType(
    EquatableArray<WitFuncParameter> Parameters,
    EquatableArray<WitType> Results
) : WitType(WitTypeKind.Func);

public record WitFuncParameter(
   string Name,
   WitType Type
);