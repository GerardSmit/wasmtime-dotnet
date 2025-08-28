namespace Wasmtime.SourceGenerator.Models;

public record WitPackage(
    WitPackageName PackageName,
    EquatableDictionary<string, WitPackageVersion> Versions
);

public record WitPackageVersion(
    string Version,
    EquatableDictionary<string, WitWorld> Worlds
);