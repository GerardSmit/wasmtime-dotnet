using System.Diagnostics.CodeAnalysis;
using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator;

public interface ITypeContainer
{
    bool TryGetType(string name, [NotNullWhen(true)] out WitType? type);

    bool TryGetContainer(string name, [NotNullWhen(true)] out ITypeContainer? container);
}