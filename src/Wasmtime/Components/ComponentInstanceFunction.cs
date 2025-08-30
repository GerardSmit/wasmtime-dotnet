using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Represents a function in a WebAssembly component instance.
/// </summary>
public readonly struct ComponentInstanceFunction
{
    internal readonly wasmtime_component_func Function;

    internal ComponentInstanceFunction(wasmtime_component_func func)
    {
        Function = func;
    }
}
