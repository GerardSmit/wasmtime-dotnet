using System;
using Wasmtime.Interop;

namespace Wasmtime;

internal unsafe class ComponentCallResultsInternal : IDisposable
{
    [ThreadStatic] private static ComponentCallResultsInternal? _cachedInstance;

    internal static ComponentCallResultsInternal ThreadInstance => _cachedInstance ??= new ComponentCallResultsInternal();

    public readonly ComponentValue[] Array;
    private wasmtime_component_func _func;
    private wasmtime_context* _context;

    internal ComponentCallResultsInternal()
    {
        Array = new ComponentValue[16];
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

        fixed (ComponentValue* ptrValue = Array)
        {
            var ptr = (wasmtime_component_val*)ptrValue;

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