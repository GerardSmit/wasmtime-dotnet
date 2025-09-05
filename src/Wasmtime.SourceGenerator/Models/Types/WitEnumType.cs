using Wasmtime.SourceGenerator.Generators.Host;

namespace Wasmtime.SourceGenerator.Models;

public record WitEnumType(
    WitPackageNameVersion Package,
    string Name
) : WitType(WitTypeKind.Enum)
{
    public override HostWriter HostWriter => new HostEnumWriter(Package, Name);
}

public record WitFlagsType(
    WitPackageNameVersion Package,
    string Name
) : WitType(WitTypeKind.Flags)
{
    public override HostWriter HostWriter => new HostFlagsWriter(Package, Name);
}
