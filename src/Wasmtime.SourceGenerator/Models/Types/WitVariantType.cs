using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

public record WitVariantType(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitVariantCase> Values
) : WitType(WitTypeKind.Variant)
{
    public override HostWriter HostWriter => new VariantHostWriter(Package, Name);
}