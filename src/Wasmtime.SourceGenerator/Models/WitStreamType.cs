namespace Wasmtime.SourceGenerator.Models;

public record WitStreamType(
    WitType ElementType
) : WitType(WitTypeKind.Stream);