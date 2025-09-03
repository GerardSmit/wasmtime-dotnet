using System;
using Wasmtime.Interop;

namespace Wasmtime;

internal unsafe class ComponentCallResultsInternal : IDisposable
{
    [ThreadStatic] private static ComponentCallResultsInternal? _cachedInstance;

    internal static ComponentCallResultsInternal ThreadInstance => _cachedInstance ??= new ComponentCallResultsInternal();

    public readonly wasmtime_component_val[] Array;
    private wasmtime_component_func _func;
    private wasmtime_context* _context;

    internal ComponentCallResultsInternal()
    {
        Array = new wasmtime_component_val[16];
    }

    public int Length { get; private set; }

    internal void Initialize(int count, wasmtime_component_func func, wasmtime_context* context)
    {
        Length = count;
        _func = func;
        _context = context;
    }

    public void Dispose()
    {
        if (_context == null)
        {
            return;
        }

        fixed (wasmtime_component_func* ptr = &_func)
        {
            wasmtime_component_func_post_return(ptr, _context);
        }

        fixed (wasmtime_component_val* ptr = Array)
        {
            for (var i = 0; i < Length; i++)
            {
                ComponentValue.Dispose(ref ptr[i]);
                wasmtime_component_val_delete(&ptr[i]);
            }
        }

        System.Array.Clear(Array, 0, Length);

        _context = null;
        _func = default;
        Length = 0;
    }
}