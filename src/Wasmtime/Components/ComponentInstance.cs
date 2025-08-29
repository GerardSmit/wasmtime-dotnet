using System;
using Wasmtime.Interop;

namespace Wasmtime;

public unsafe class ComponentInstance
{
    private readonly Component _component;
    private readonly wasmtime_component_instance _handle;
    private readonly Store _store;

    internal ComponentInstance(Component component, wasmtime_component_instance handle, Store store)
    {
        _component = component;
        _handle = handle;
        _store = store;
    }

    public ComponentCallResults Call(string name, int resultCount, ReadOnlySpan<ComponentValue> values)
    {
        if (!_component.TryGetExport(name, out var index))
        {
            throw new WasmtimeException($"Function '{name}' not found in component");
        }

        byte success;
        wasmtime_component_func func;
        var store = _store.Context;

        fixed (wasmtime_component_instance* instance = &_handle)
        {
            success = wasmtime_component_instance_get_func(instance, store, index, &func);
        }

        if (success != 1)
        {
            throw new WasmtimeException($"Export '{name}' is not a function");
        }

        var results = ComponentCallResultsInternal.ThreadInstance;
        results.Initialize(resultCount, func, store);

        fixed (ComponentValue* argsPtr = values)
        fixed (wasmtime_component_val* resultsPtr = results.Array)
        {
            var error = wasmtime_component_func_call(
                &func,
                store,
                (wasmtime_component_val*)argsPtr,
                (nuint)values.Length,
                resultsPtr,
                (nuint)results.Length
            );

            WasmtimeException.ThrowIfError(error);
        }

        return new ComponentCallResults(results);
    }

}