using System;
using Wasmtime.Interop;

namespace Wasmtime;

public sealed unsafe class Store : IDisposable
{
    internal wasmtime_store* Handle;
    internal wasmtime_context* Context;

    private wasmtime_wasip2_config_t* _wasiP2Config;

    public Store(Engine engine)
    {
        Handle = wasmtime_store_new(engine.Handle, null, IntPtr.Zero);
        Context = wasmtime_store_context(Handle);
    }

    internal bool IsWasiP2Added => _wasiP2Config != null;

    public void AddWasiP2()
    {
        var cfg = wasmtime_wasip2_config_new();
        wasmtime_wasip2_config_inherit_stdin(cfg);
        wasmtime_wasip2_config_inherit_stdout(cfg);
        wasmtime_wasip2_config_inherit_stderr(cfg);
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

    /// <inheritdoc />
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Store()
    {
        ReleaseUnmanagedResources();
    }
}