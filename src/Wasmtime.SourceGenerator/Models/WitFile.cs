namespace Wasmtime.SourceGenerator.Models;

public record WitFile(
    EquatableDictionary<WitPackageName, WitPackage> Packages
)
{
    public static WitFile Empty { get; } = new(Packages: default);
}