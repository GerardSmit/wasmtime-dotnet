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
    private readonly SemaphoreSlim _semaphore = new(1, 1);

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
        _semaphore.Wait(TimeSpan.FromSeconds(5));

        try
        {
            fixed (ComponentValue* valuesPtr = values)
            {
                var results = CallInternal(GetFunction(name), resultCount, valuesPtr, values.Length);

                return new ComponentCallResults(_semaphore, results);
            }
        }
        catch
        {
            _semaphore.Release();
            throw;
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
        _semaphore.Wait(TimeSpan.FromSeconds(5));

        try
        {
            var results = CallInternal(GetFunction(name), resultCount, values, valuesLength);

            return new ComponentCallResults(_semaphore, results);
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    /// <summary>
    /// Calls a function in the component instance without any synchronization.
    /// The caller is responsible for ensuring that calls to the current component instance are not made concurrently.
    /// </summary>
    /// <param name="name">Name of the function to call.</param>
    /// <param name="resultCount">Number of results to expect from the call.</param>
    /// <param name="values">Arguments to pass to the function.</param>
    /// <returns>The results of the function call.</returns>
    public ComponentCallResults CallUnsafe(string name, int resultCount, ReadOnlySpan<ComponentValue> values)
    {
        return CallUnsafe(GetFunction(name), resultCount, values);
    }

    /// <summary>
    /// Calls a function in the component instance without any synchronization.
    /// The caller is responsible for ensuring that calls to the current component instance are not made concurrently.
    /// </summary>
    /// <param name="name">Name of the function to call.</param>
    /// <param name="resultCount">Number of results to expect from the call.</param>
    /// <param name="values">Arguments to pass to the function.</param>
    /// <param name="valuesLength">Length of the arguments array.</param>
    /// <returns>The results of the function call.</returns>
    public ComponentCallResults CallUnsafe(string name, int resultCount, ComponentValue* values, int valuesLength)
    {
        return CallUnsafe(GetFunction(name), resultCount, values, valuesLength);
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
        _semaphore.Wait(TimeSpan.FromSeconds(5));

        try
        {
            fixed (ComponentValue* valuesPtr = values)
            {
                var results = CallInternal(function, resultCount, valuesPtr, values.Length);

                return new ComponentCallResults(_semaphore, results);
            }
        }
        catch
        {
            _semaphore.Release();
            throw;
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
        _semaphore.Wait(TimeSpan.FromSeconds(5));

        try
        {
            var results = CallInternal(function, resultCount, values, valuesLength);

            return new ComponentCallResults(_semaphore, results);
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    /// <summary>
    /// Calls a function in the component instance without any synchronization.
    /// The caller is responsible for ensuring that calls to the current component instance are not made concurrently.
    /// </summary>
    /// <param name="function">Instance of <see cref="GetFunction"/> to call.</param>
    /// <param name="resultCount">Number of results to expect from the call.</param>
    /// <param name="values">Arguments to pass to the function.</param>
    /// <returns>The results of the function call.</returns>
    public ComponentCallResults CallUnsafe(ComponentInstanceFunction function, int resultCount, ReadOnlySpan<ComponentValue> values)
    {
        fixed (ComponentValue* valuesPtr = values)
        {
            var results = CallInternal(function, resultCount, valuesPtr, values.Length);
            return new ComponentCallResults(null, results);
        }
    }

    /// <summary>
    /// Calls a function in the component instance without any synchronization.
    /// The caller is responsible for ensuring that calls to the current component instance are not made concurrently.
    /// </summary>
    /// <param name="function">Instance of <see cref="GetFunction"/> to call.</param>
    /// <param name="resultCount">Number of results to expect from the call.</param>
    /// <param name="values">Arguments to pass to the function.</param>
    /// <param name="valuesLength">Length of the arguments array.</param>
    /// <returns>The results of the function call.</returns>
    public ComponentCallResults CallUnsafe(ComponentInstanceFunction function, int resultCount, ComponentValue* values, int valuesLength)
    {
        var results = CallInternal(function, resultCount, values, valuesLength);
        return new ComponentCallResults(null, results);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ComponentCallResultsInternal CallInternal(ComponentInstanceFunction function, int resultCount, ComponentValue* values, int valuesLength)
    {
#if NET
        ObjectDisposedException.ThrowIf(_store.Disposed, nameof(Store));
#else
        if (_store.Disposed) throw new ObjectDisposedException(nameof(Store));
#endif

        var results = ComponentCallResultsInternal.ThreadInstance;
        results.Initialize(resultCount, function.Function, _store.Context);

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

        return results;
    }

}