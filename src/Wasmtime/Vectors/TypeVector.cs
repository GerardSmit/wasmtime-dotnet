using System;
using System.Runtime.CompilerServices;
using Wasmtime.Interop;

namespace Wasmtime;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly unsafe struct TypeVector : IDisposable
{
    private readonly wasm_exporttype_vec_t _vec;

    public TypeVector()
    {
        fixed (wasm_exporttype_vec_t* vec = &_vec)
        {
            wasm_exporttype_vec_new_empty(vec);
        }
    }

    public TypeVector(nuint size)
    {
        fixed (wasm_exporttype_vec_t* vec = &_vec)
        {
            wasm_exporttype_vec_new_uninitialized(vec, size);
        }
    }

    internal TypeVector(TypeVector other)
    {
        fixed (wasm_exporttype_vec_t* vec = &_vec)
        {
            wasm_exporttype_vec_copy(vec, &other._vec);
        }
    }

    public int Size => (int)_vec.size;

    internal string DebuggerDisplay
    {
        get
        {
            if (Size == 0) return "[]";
            if (Size == 1) return $"[{GetKind(0)}]";

            var sb = new System.Text.StringBuilder();
            sb.Append('[');

            for (var i = 0; i < Size; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(GetKind(i).ToString());
            }

            sb.Append(']');
            return sb.ToString();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WasmExternKind GetKind(int index)
    {
        if (index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));

        var type = wasm_exporttype_type(_vec.data[index]);

        return (WasmExternKind) wasm_externtype_kind(type);
    }

    public void Dispose()
    {
        fixed (wasm_exporttype_vec_t* vec = &_vec)
        {
            wasm_exporttype_vec_delete(vec);
        }
    }
}
