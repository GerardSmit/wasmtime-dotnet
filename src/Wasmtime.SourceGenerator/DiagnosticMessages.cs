using Microsoft.CodeAnalysis;

namespace Wasmtime.SourceGenerator;

public static class DiagnosticMessages
{
    public static readonly DiagnosticDescriptor MultipleFilePackagesInDirectory = new(
        id: "WSGEN001",
        title: "Multiple packages in directory",
        messageFormat: "Only a single package is allowed per directory. Found multiple packages: {0} and {1} in directory '{2}'.",
        category: WitGen,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public const string WitGen = "WitGen";
}
