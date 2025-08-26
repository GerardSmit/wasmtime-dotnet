using System;
using Wasmtime.Interop;

namespace Wasmtime;

public sealed unsafe class Engine : IDisposable
{
    internal wasm_engine_t* Handle;

    public Engine()
    {
        Handle = wasm_engine_new();
    }

    private void ReleaseUnmanagedResources()
    {
        if (Handle != null)
        {
            wasm_engine_delete(Handle);
            Handle = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Engine()
    {
        ReleaseUnmanagedResources();
    }
}