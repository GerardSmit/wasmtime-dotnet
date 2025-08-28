namespace Wasmtime.SourceGenerator.Models;

public record WitOptionType(
    WitType ElementType
) : WitType(WitTypeKind.Option);