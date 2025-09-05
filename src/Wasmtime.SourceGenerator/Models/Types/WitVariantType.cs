using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

public record WitVariantType(
    WitPackageNameVersion Package,
    string Name,
    EquatableArray<WitVariantCase> Values
) : WitType(WitTypeKind.Variant)
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public override TypeHostWriter HostWriter => new VariantHostWriter(Package, CSharpName);
}