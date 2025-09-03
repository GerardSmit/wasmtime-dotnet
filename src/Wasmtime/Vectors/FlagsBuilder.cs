using System;
using System.Runtime.InteropServices;
using System.Text;
using Wasmtime.Interop;

namespace Wasmtime;

public readonly unsafe struct FlagsBuilder : IDisposable
{
    private readonly wasmtime_component_valflags _vector;
    private readonly bool _disposeValues;

    internal FlagsBuilder(wasmtime_component_valflags vector, bool disposeValues = false)
    {
        _vector = vector;
        _disposeValues = disposeValues;
    }

    public FlagsBuilder(int size, bool disposeValues = true)
    {
        _disposeValues = disposeValues;
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
            if (_disposeValues)
            {
                new ByteVector(_vector.data[i]).Dispose();
            }

            _vector.data[i] = default;
        }

        fixed (wasmtime_component_valflags* vec = &_vector)
        {
            wasmtime_component_valflags_delete(vec);
        }
    }
}
