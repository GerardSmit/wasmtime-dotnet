using System;
using System.Threading;
using Wasmtime.Interop;

namespace Wasmtime;

internal unsafe class ComponentCallResultsInternal : IDisposable
{
    [ThreadStatic] private static ComponentCallResultsInternal? _cachedInstance;

    internal static ComponentCallResultsInternal ThreadInstance => _cachedInstance ??= new ComponentCallResultsInternal();

    public readonly ComponentValue[] Array;
    private wasmtime_component_func _func;
    private wasmtime_context* _context;
    private SemaphoreSlim? _semaphore;

    private ComponentCallResultsInternal()
    {
        Array = new ComponentValue[16];
    }

    public int Length { get; private set; }

    internal void Initialize(int count, wasmtime_component_func func, wasmtime_context* context, SemaphoreSlim semaphore)
    {
        if (_semaphore is not null)
        {
            throw new InvalidOperationException("This instance is already in use.");
        }

        Length = count;
        _func = func;
        _context = context;
        _semaphore = semaphore;
    }

    public void Dispose()
    {
        if (_semaphore is not {} semaphore)
        {
            throw new ObjectDisposedException(nameof(ComponentCallResultsInternal));
        }

        fixed (wasmtime_component_func* ptr = &_func)
        {
            wasmtime_component_func_post_return(ptr, _context);
        }

        System.Array.Clear(Array, 0, Length);

        _semaphore = null;
        _context = null;
        _func = default;
        Length = 0;

        semaphore.Release();
    }
}