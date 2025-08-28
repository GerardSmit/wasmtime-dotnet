using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Represents a compiled WebAssembly component.
/// </summary>
public unsafe class Component : IDisposable
{
    private readonly ConcurrentDictionary<string, nint> _cachedExports = new();
    private wasmtime_component_t* _handle;

    /// <summary>
    /// Instantiates the component using the specified <see cref="Linker"/> and <see cref="Store"/>.
    /// </summary>
    /// <param name="linker">The linker to use for instantiation.</param>
    /// <param name="store">The store to use for instantiation.</param>
    /// <returns>A new <see cref="ComponentInstance"/> representing the instantiated component.</returns>
    /// <exception cref="WasmtimeException">Thrown if instantiation fails or WASI P2 is not consistently added.</exception>
    public ComponentInstance CreateInstance(Linker linker, Store store)
    {
        if (linker.IsWasiP2Added != store.IsWasiP2Added)
        {
            throw new WasmtimeException("WASI P2 must be added to both the linker and the store");
        }

        wasmtime_component_instance instance;
        var error = wasmtime_component_linker_instantiate(linker.ComponentHandle, store.Context, _handle, &instance);

        WasmtimeException.ThrowIfError(error);

        return new ComponentInstance(this, instance, store);
    }

    private Component(wasmtime_component_t* handle)
    {
        _handle = handle;
    }

    internal bool TryGetExport(string name, out wasmtime_component_export_index_t* index)
    {
        if (_cachedExports.TryGetValue(name, out var idx))
        {
            index = (wasmtime_component_export_index_t*)idx;
            return true;
        }

        if (TryGetExportIndexInternal(name, out index))
        {
            return true;
        }

        index = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryGetExportIndexInternal(string name, out wasmtime_component_export_index_t* index)
    {
        var bytes = Encoding.UTF8.GetMaxByteCount(name.Length) <= 256
            ? stackalloc byte[256]
            : new byte[Encoding.UTF8.GetByteCount(name)];

        fixed (char* utf16 = name)
        fixed (byte* p = bytes)
        {
            var len = Encoding.UTF8.GetBytes(utf16, name.Length, p, bytes.Length);
            index = wasmtime_component_get_export_index(_handle, null, p, (nuint)len);

            if (index == null)
            {
                return false;
            }
        }

        if (_cachedExports.TryAdd(name, (nint)index))
        {
            return true;
        }

        wasmtime_component_export_index_delete(index);
        index = (wasmtime_component_export_index_t*)_cachedExports[name];
        return true;
    }

    /// <summary>
    /// Compiles a component from the given buffer.
    /// </summary>
    /// <param name="engine">The engine to use for compilation.</param>
    /// <param name="buf">A pointer to the buffer containing the component bytes.</param>
    /// <param name="len">The length of the buffer.</param>
    /// <returns>A compiled <see cref="Component"/>.</returns>
    /// <exception cref="WasmtimeException">Thrown if compilation fails.</exception>
    public static Component Compile(Engine engine, byte* buf, nuint len)
    {
        wasmtime_component_t* component = null;
        var error = wasmtime_component_new(engine.Handle, buf, len, &component);

        WasmtimeException.ThrowIfError(error);

        return new Component(component);
    }

    /// <summary>
    /// Compiles a component from the given byte span.
    /// </summary>
    /// <param name="engine">The engine to use for compilation.</param>
    /// <param name="bytes">The bytes of the component.</param>
    /// <returns>A compiled <see cref="Component"/>.</returns>
    /// <exception cref="WasmtimeException">Thrown if compilation fails.</exception>
    public static Component Compile(Engine engine, ReadOnlySpan<byte> bytes)
    {
        fixed (byte* p = bytes)
        {
            return Compile(engine, p, (nuint)bytes.Length);
        }
    }

    /// <summary>
    /// Compiles a component from the given WAT (WebAssembly Text) string.
    /// </summary>
    /// <param name="engine">The engine to use for compilation.</param>
    /// <param name="wat">The WAT string representing the component.</param>
    /// <returns>A compiled <see cref="Component"/>.</returns>
    /// <exception cref="WasmtimeException">Thrown if compilation fails.</exception>
    public static Component Compile(Engine engine, string wat)
    {
        var bytes = Encoding.UTF8.GetMaxByteCount(wat.Length) <= 256
            ? stackalloc byte[256]
            : new byte[Encoding.UTF8.GetByteCount(wat)];

        fixed (char* utf16 = wat)
        fixed (byte* utf8 = bytes)
        {
            return Compile(engine, utf8, (nuint)Encoding.UTF8.GetBytes(utf16, wat.Length, utf8, bytes.Length));
        }
    }

    /// <summary>
    /// Loads a component from a serialized file using a buffer pointer.
    /// </summary>
    /// <param name="engine">The engine to use for deserialization.</param>
    /// <param name="buf">A pointer to the file path as a UTF-8 buffer.</param>
    /// <returns>A deserialized <see cref="Component"/>.</returns>
    /// <exception cref="WasmtimeException">Thrown if deserialization fails.</exception>
    public static Component FromFile(Engine engine, byte* buf)
    {
        wasmtime_component_t* component = null;
        var error = wasmtime_component_deserialize_file(engine.Handle, buf, &component);

        WasmtimeException.ThrowIfError(error);

        return new Component(component);
    }

    /// <summary>
    /// Loads a component from a serialized file at the specified path.
    /// </summary>
    /// <param name="engine">The engine to use for deserialization.</param>
    /// <param name="path">The file path to the serialized component.</param>
    /// <returns>A deserialized <see cref="Component"/>.</returns>
    /// <exception cref="WasmtimeException">Thrown if deserialization fails.</exception>
    public static Component FromFile(Engine engine, string path)
    {
        var bytes = Encoding.UTF8.GetMaxByteCount(path.Length) <= 255
            ? stackalloc byte[256]
            : new byte[Encoding.UTF8.GetByteCount(path) + 1];

        fixed (char* utf16 = path)
        fixed (byte* utf8 = bytes)
        {
            var len = Encoding.UTF8.GetBytes(utf16, path.Length, utf8, bytes.Length - 1);
            bytes[len] = 0;

            return FromFile(engine, utf8);
        }
    }

    /// <summary>
    /// Deserializes a component from the given byte span.
    /// </summary>
    /// <param name="engine">The engine to use for deserialization.</param>
    /// <param name="bytes">The bytes of the serialized component.</param>
    /// <returns>A deserialized <see cref="Component"/>.</returns>
    /// <exception cref="WasmtimeException">Thrown if deserialization fails.</exception>
    public static Component Deserialize(Engine engine, ReadOnlySpan<byte> bytes)
    {
        fixed (byte* p = bytes)
        {
            return Deserialize(engine, p, bytes.Length);
        }
    }

    /// <summary>
    /// Deserializes a component from the given buffer.
    /// </summary>
    /// <param name="engine">The engine to use for deserialization.</param>
    /// <param name="buf">A pointer to the buffer containing the serialized component.</param>
    /// <param name="len">The length of the buffer.</param>
    /// <returns>A deserialized <see cref="Component"/>.</returns>
    /// <exception cref="WasmtimeException">Thrown if deserialization fails.</exception>
    public static Component Deserialize(Engine engine, byte* buf, int len)
    {
        wasmtime_component_t* component = null;
        var error = wasmtime_component_deserialize(engine.Handle, buf, (nuint)len, &component);

        WasmtimeException.ThrowIfError(error);

        return new Component(component);
    }

    /// <summary>
    /// Releases unmanaged resources associated with this component.
    /// </summary>
    private void ReleaseUnmanagedResources()
    {
        foreach (var export in _cachedExports)
        {
            wasmtime_component_export_index_delete((wasmtime_component_export_index_t*)export.Value);
        }

        _cachedExports.Clear();

        if (_handle != null)
        {
            wasmtime_component_delete(_handle);
            _handle = null;
        }
    }

    /// <summary>
    /// Releases resources associated with this component.
    /// </summary>
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure unmanaged resources are released.
    /// </summary>
    ~Component()
    {
        ReleaseUnmanagedResources();
    }
}