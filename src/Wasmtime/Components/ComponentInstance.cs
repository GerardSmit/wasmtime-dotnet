using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Represents an instance of a WebAssembly component.
/// </summary>
public unsafe class ComponentInstance
{
    private readonly Dictionary<string, ComponentInstanceFunction> _cachedFunctions = new(StringComparer.Ordinal);
    private readonly Component _component;
    private readonly wasmtime_component_instance _handle;
    private readonly Store _store;

    internal readonly SemaphoreSlim Lock = new(1, 1);

    internal ComponentInstance(Component component, wasmtime_component_instance handle, Store store)
    {
        _component = component;
        _handle = handle;
        _store = store;
    }

    /// <summary>
    /// Calls a function in the component instance with synchronization.
    /// </summary>
    /// <param name="name">Name of the function to call.</param>
    /// <param name="resultCount">Number of results to expect from the call.</param>
    /// <param name="values">Arguments to pass to the function.</param>
    /// <returns>The results of the function call.</returns>
    public ComponentCallResults Call(string name, int resultCount, ReadOnlySpan<ComponentValue> values)
    {
        fixed (ComponentValue* valuesPtr = values)
        {
            return Call(name, resultCount, valuesPtr, values.Length);
        }
    }

    /// <summary>
    /// Calls a function in the component instance with synchronization.
    /// </summary>
    /// <param name="name">Name of the function to call.</param>
    /// <param name="resultCount">Number of results to expect from the call.</param>
    /// <param name="values">Arguments to pass to the function.</param>
    /// <param name="valuesLength">Length of the arguments array.</param>
    /// <returns>The results of the function call.</returns>
    public ComponentCallResults Call(string name, int resultCount, ComponentValue* values, int valuesLength)
    {
        return Call(GetFunction(name), resultCount, values, valuesLength);
    }

    /// <summary>
    /// Calls a function in the component instance with synchronization.
    /// </summary>
    /// <param name="function">Instance of <see cref="GetFunction"/> to call.</param>
    /// <param name="resultCount">Number of results to expect from the call.</param>
    /// <param name="values">Arguments to pass to the function.</param>
    /// <returns>The results of the function call.</returns>
    public ComponentCallResults Call(ComponentInstanceFunction function, int resultCount, ReadOnlySpan<ComponentValue> values)
    {
        fixed (ComponentValue* valuesPtr = values)
        {
            return Call(function, resultCount, valuesPtr, values.Length);
        }
    }

    /// <summary>
    /// Calls a function in the component instance with synchronization.
    /// </summary>
    /// <param name="function">Instance of <see cref="GetFunction"/> to call.</param>
    /// <param name="resultCount">Number of results to expect from the call.</param>
    /// <param name="values">Arguments to pass to the function.</param>
    /// <param name="valuesLength">Length of the arguments array.</param>
    /// <returns>The results of the function call.</returns>
    public ComponentCallResults Call(ComponentInstanceFunction function, int resultCount, ComponentValue* values, int valuesLength)
    {
        // Note: semaphore is released in ComponentCallResultsInternal.Dispose
        if (!Lock.Wait(TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("Could not acquire lock to call component function");
        }

        try
        {
#if NET
            ObjectDisposedException.ThrowIf(_store.Disposed, nameof(Store));
#else
            if (_store.Disposed) throw new ObjectDisposedException(nameof(Store));
#endif

            var results = ComponentCallResultsInternal.ThreadInstance;
            results.Initialize(resultCount, function.Function, _store.Context, Lock);

            fixed (ComponentValue* resultsPtr = results.Array)
            {
                var error = wasmtime_component_func_call(
                    &function.Function,
                    _store.Context,
                    (wasmtime_component_val*)values,
                    (nuint)valuesLength,
                    (wasmtime_component_val*)resultsPtr,
                    (nuint)results.Length
                );

                WasmtimeException.ThrowIfError(error);
            }

            return new ComponentCallResults(results);
        }
        catch
        {
            Lock.Release();
            throw;
        }
    }

    /// <summary>
    /// Gets the function in the component instance with the specified name.
    /// </summary>
    /// <param name="name">Name of the function to get.</param>
    /// <returns>The function with the specified name.</returns>
    /// <exception cref="WasmtimeException">Thrown if the function is not found in the component instance.</exception>
    /// <remarks>
    /// This method is not thread-safe.
    /// </remarks>
    public ComponentInstanceFunction GetFunction(string name)
    {
        return _cachedFunctions.TryGetValue(name, out var handle)
            ? handle
            : LoadFunction(name);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ComponentInstanceFunction LoadFunction(string name)
    {
        if (!Lock.Wait(TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("Could not acquire lock to load component function");
        }

        try
        {
#if NET
            ObjectDisposedException.ThrowIf(_store.Disposed, nameof(Store));
#else
            if (_store.Disposed) throw new ObjectDisposedException(nameof(Store));
#endif

            if (!_component.TryGetExport(name, out var index))
            {
                throw new WasmtimeException($"Function '{name}' not found in component");
            }

            byte success;
            wasmtime_component_func func;

            fixed (wasmtime_component_instance* instance = &_handle)
            {
                success = wasmtime_component_instance_get_func(instance, _store.Context, index, &func);
            }

            if (success != 1)
            {
                throw new WasmtimeException($"Export '{name}' is not a function");
            }

            var function = new ComponentInstanceFunction(func);
            _cachedFunctions[name] = function;
            return function;
        }
        finally
        {
            Lock.Release();
        }
    }

}