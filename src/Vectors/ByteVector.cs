using System;
using System.Text;
using Wasmtime.Interop;

namespace Wasmtime;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public unsafe struct ByteVector : IDisposable
{
    private wasm_byte_vec_t _vector;

    public ByteVector()
    {
        fixed (wasm_byte_vec_t* vec = &_vector)
        {
            wasm_byte_vec_new_empty(vec);
        }
    }

    internal ByteVector(wasm_byte_vec_t* vector)
    {
        _vector = *vector;
    }

    public ByteVector(int size)
    {
        fixed (wasm_byte_vec_t* vec = &_vector)
        {
            wasm_byte_vec_new_uninitialized(vec, (UIntPtr)size);
        }
    }

    public ByteVector(ByteVector vector)
    {
        fixed (wasm_byte_vec_t* vec = &_vector)
        {
            wasm_byte_vec_copy(vec, &vector._vector);
        }
    }

    public ByteVector(byte* data, int size)
    {
        fixed (wasm_byte_vec_t* vec = &_vector)
        {
            wasm_byte_vec_new(vec, (UIntPtr)size, data);
        }
    }

    public ByteVector(ReadOnlySpan<byte> data)
    {
        fixed (wasm_byte_vec_t* vec = &_vector)
        fixed (byte* pData = data)
        {
            wasm_byte_vec_new(vec, (UIntPtr)data.Length, pData);
        }
    }

    public ByteVector(string data)
    {
        var bytes = Encoding.UTF8.GetMaxByteCount(data.Length) <= 256
            ? stackalloc byte[256]
            : new byte[Encoding.UTF8.GetByteCount(data)];

        fixed (char* utf16 = data)
        fixed (byte* p = bytes)
        fixed (wasm_byte_vec_t* vec = &_vector)
        {
            var len = Encoding.UTF8.GetBytes(utf16, data.Length, p, bytes.Length);

            wasm_byte_vec_new(vec, (UIntPtr)len, p);
        }
    }

    internal ByteVector(wasmtime_error* error)
    {
        fixed (wasm_byte_vec_t* vec = &_vector)
        {
            wasmtime_error_message(error, vec);
        }
    }

    internal wasm_byte_vec_t Handle => _vector;

    public bool HasValue => _vector.data != null;

    /// <summary>
    /// Gets the size of the vector.
    /// </summary>
    public int Size => (int)_vector.size;

    /// <summary>
    /// Gets the contents of the vector as a span of bytes.
    /// </summary>
    public Span<byte> Span => new(_vector.data, (int)_vector.size);

    internal string DebuggerDisplay => $"vec[{Size}]";

    /// <summary>
    /// Gets the contents of the vector as a UTF-8 string.
    /// </summary>
    /// <returns>The string.</returns>
    public string GetString()
    {
        return Encoding.UTF8.GetString(_vector.data, (int)_vector.size);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_vector.data != null)
        {
            fixed (wasm_byte_vec_t* vec = &_vector)
            {
                wasm_byte_vec_delete(vec);
            }
        }
    }
}
