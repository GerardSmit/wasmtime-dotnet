using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator;

/// <summary>
/// Represents a type container resolver.
/// </summary>
public interface ITypeContainerResolver
{
    /// <summary>
    /// Resolves the specified full name to a type container.
    /// </summary>
    /// <param name="fullName">The full name of the type to resolve.</param>
    /// <returns>The resolved type container.</returns>
    ITypeContainer Resolve(WitPackageNameVersion fullName);
}