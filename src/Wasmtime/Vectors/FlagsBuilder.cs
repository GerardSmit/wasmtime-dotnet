using System;
using System.Runtime.InteropServices;
using System.Text;
using Wasmtime.Interop;

namespace Wasmtime;

public readonly unsafe struct FlagsBuilder : IDisposable
{
    private readonly wasmtime_component_valflags _vector;

    internal FlagsBuilder(wasmtime_component_valflags vector)
    {
        _vector = vector;
    }

    public FlagsBuilder(int size)
    {
        fixed (wasmtime_component_valflags* vec = &_vector)
        {
            wasmtime_component_valflags_new_uninit(vec, (UIntPtr)size);
        }
    }

    public int Length => (int)_vector.size;

    internal wasmtime_component_valflags Value => _vector;

    public ByteVector this[int index]
    {
        get
        {
            if (index < 0 || index >= (int)_vector.size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return new ByteVector(_vector.data[index]);
        }
        set
        {
            if (index < 0 || index >= (int)_vector.size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var data = (ByteVector*)_vector.data;
            data[index] = value;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_vector.data == null) return;

        for (var i = 0; i < (int)_vector.size; i++)
        {
            _vector.data[i] = default;
        }

        fixed (wasmtime_component_valflags* vec = &_vector)
        {
            wasmtime_component_valflags_delete(vec);
        }
    }
}
