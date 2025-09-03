using System;
using System.Runtime.InteropServices;
using System.Text;
using Wasmtime.Interop;

namespace Wasmtime;

public readonly unsafe struct ListBuilder : IDisposable
{
    private readonly wasmtime_component_vallist _vector;

    internal ListBuilder(wasmtime_component_vallist vector)
    {
        _vector = vector;
    }

    public ListBuilder(int size)
    {
        fixed (wasmtime_component_vallist* vec = &_vector)
        {
            wasmtime_component_vallist_new_uninit(vec, (UIntPtr)size);
        }
    }

    public int Length => (int)_vector.size;

    internal wasmtime_component_vallist Value => _vector;

    public ComponentValue this[int index]
    {
        get
        {
            if (index < 0 || index >= (int)_vector.size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return new ComponentValue(_vector.data[index]);
        }
        set
        {
            if (index < 0 || index >= (int)_vector.size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var data = (ComponentValue*)_vector.data;
            data[index] = value;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_vector.data == null) return;

        for (var i = 0; i < (int)_vector.size; i++)
        {
            ComponentValue.Dispose(ref _vector.data[i]);
        }

        fixed (wasmtime_component_vallist* vec = &_vector)
        {
            wasmtime_component_vallist_delete(vec);
        }
    }
}
