using System.Collections.Immutable;

namespace Wasmtime.SourceGenerator.Models;

public record WitDirectory(
    EquatableDictionary<WitPackageName, WitPackage> Packages,
    ImmutableArray<ReportedDiagnostic> Diagnostics
)
{
    public static WitDirectory Empty { get; } = new(Diagnostics: default, Packages: default);
}

public record WitRawDirectory(
    string Path,
    EquatableArray<string> Files
);