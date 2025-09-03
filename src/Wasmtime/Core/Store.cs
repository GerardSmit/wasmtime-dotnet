using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using Wasmtime.Interop;

namespace Wasmtime;

public sealed unsafe class Store : IDisposable
{
    private readonly ConcurrentDictionary<nint, ComponentInstance> _instances = new();
    private readonly SemaphoreSlim _instanceSemaphore = new(1, 1);

    internal wasmtime_store* Handle;
    internal wasmtime_context* Context;
    internal bool Disposed;

    private wasmtime_wasip2_config_t* _wasiP2Config;

    public Store(Engine engine)
    {
        Handle = wasmtime_store_new(engine.Handle, null, IntPtr.Zero);
        Context = wasmtime_store_context(Handle);
    }

    internal bool IsWasiP2Added => _wasiP2Config != null;

    public void AddWasiP2(bool inheritStdin = false, bool inheritStdout = false, bool inheritStderr = false)
    {
        var cfg = wasmtime_wasip2_config_new();

        if (inheritStdin) wasmtime_wasip2_config_inherit_stdin(cfg);
        if (inheritStdout) wasmtime_wasip2_config_inherit_stdout(cfg);
        if (inheritStderr) wasmtime_wasip2_config_inherit_stderr(cfg);

        wasmtime_context_set_wasip2(Context, cfg);

        _wasiP2Config = cfg;
    }

    private void ReleaseUnmanagedResources()
    {
        if (Handle != null)
        {
            wasmtime_store_delete(Handle);
            Handle = null;
        }
    }

    /// <summary>
    /// Gets an instance of a component associated with this store.
    /// </summary>
    /// <param name="component">The component to instantiate.</param>
    /// <param name="linker">The linker to use for instantiation.</param>
    /// <returns>The instantiated component instance.</returns>
    public ComponentInstance GetComponentInstance(Component component, Linker linker)
    {
#if NET
        ObjectDisposedException.ThrowIf(Disposed, nameof(Store));
#else
        if (Disposed) throw new ObjectDisposedException(nameof(Store));
#endif

        return _instances.TryGetValue((nint)component.Handle, out var existing)
            ? existing
            : GetComponentInstanceInner(component, linker);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ComponentInstance GetComponentInstanceInner(Component component, Linker linker)
    {
        if (linker.IsWasiP2Added != IsWasiP2Added)
        {
            throw new WasmtimeException("WASI P2 must be added to both the linker and the store");
        }

        _instanceSemaphore.Wait();

        try
        {
            wasmtime_component_instance handle;
            var error = wasmtime_component_linker_instantiate(linker.ComponentHandle, Context, component.Handle, &handle);

            WasmtimeException.ThrowIfError(error);

            var instance = new ComponentInstance(component, handle, this);
            _instances.TryAdd((nint)component.Handle, instance);
            return instance;
        }
        finally
        {
            _instanceSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    ~Store()
    {
        ReleaseUnmanagedResources();
    }
}