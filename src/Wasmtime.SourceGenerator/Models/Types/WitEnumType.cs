using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

public record WitEnumType(
    WitPackageNameVersion Package,
    string Name
) : WitType(WitTypeKind.Enum)
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public override TypeHostWriter HostWriter => new EnumHostWriter(Package, CSharpName);
}

public record WitFlagsType(
    WitPackageNameVersion Package,
    string Name
) : WitType(WitTypeKind.Flags)
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public override TypeHostWriter HostWriter => new FlagsHostWriter(Package, CSharpName);
}
