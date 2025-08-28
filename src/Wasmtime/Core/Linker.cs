using System;
using Wasmtime.Interop;

namespace Wasmtime;

public sealed unsafe class Linker : IDisposable
{
    internal wasmtime_linker* Handle;
    internal wasmtime_component_linker_t* ComponentHandle;

    public Linker(Engine engine)
    {
        Handle = wasmtime_linker_new(engine.Handle);
        ComponentHandle = wasmtime_component_linker_new(engine.Handle);
    }

    internal bool IsWasiP2Added { get; private set; }

    public void AddWasiP2()
    {
        wasmtime_component_linker_add_wasip2(ComponentHandle);
        IsWasiP2Added = true;
    }

    private void ReleaseUnmanagedResources()
    {
        if (Handle != null)
        {
            wasmtime_linker_delete(Handle);
            Handle = null;
        }

        if (ComponentHandle != null)
        {
            wasmtime_component_linker_delete(ComponentHandle);
            ComponentHandle = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Linker()
    {
        ReleaseUnmanagedResources();
    }
}