using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Wasmtime.Interop;

namespace Wasmtime;

public interface IComponentImports
{
    void Register(Linker linker);
}

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

    public void Define(IComponentImports imports)
    {
        imports.Register(this);
    }

    public void DefineFunction(string name, ComponentFunctionDelegate function, object? state = null)
    {
        var data = ComponentExport.RegisterFunction(function, state);
        var root = wasmtime_component_linker_root(ComponentHandle);
        var bytes = Encoding.UTF8.GetMaxByteCount(name.Length) <= 256
            ? stackalloc byte[256]
            : new byte[Encoding.UTF8.GetByteCount(name)];

        fixed (char* utf16 = name)
        fixed (byte* utf8 = bytes)
        {
            var length = Encoding.UTF8.GetBytes(utf16, name.Length, utf8, bytes.Length);

            wasmtime_component_linker_instance_add_func(
                root,
                utf8,
                (nuint)length,
                (nint)ComponentExport.CallerPtr,
                (void*)data,
                IntPtr.Zero
            );
        }
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