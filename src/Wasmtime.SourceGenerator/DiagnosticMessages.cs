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

    public static readonly DiagnosticDescriptor Error = new(
        id: "WSGEN002",
        title: "Error while processing",
        messageFormat: "An error occurred while processing the package '{0}': {1}",
        category: WitGen,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public const string WitGen = "WitGen";
}
