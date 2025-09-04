using System;
using System.Runtime.CompilerServices;
using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Simple builder for creating and manipulating records of record entries.
/// </summary>
public readonly unsafe ref struct RecordBuilder : IDisposable
{
    private readonly wasmtime_component_valrecord _source;
    private readonly bool _disposeNames = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBuilder"/> struct with the specified items and length.
    /// </summary>
    /// <param name="length">The number of items in the record.</param>
    /// <param name="disposeNames"><c>true</c> to dispose names when the record is disposed; otherwise, <c>false</c>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than zero.</exception>
    public RecordBuilder(int length, bool disposeNames = true)
    {
        _disposeNames = disposeNames;

        fixed (wasmtime_component_valrecord* p = &_source)
        {
            wasmtime_component_valrecord_new_uninit(p, (nuint)length);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBuilder"/> struct with the specified source.
    /// </summary>
    /// <param name="source">The source <see cref="wasmtime_component_valrecord"/> to wrap.</param>
    /// <param name="disposeNames"><c>true</c> to dispose names when the record is disposed; otherwise, <c>false</c>.</param>
    internal RecordBuilder(wasmtime_component_valrecord source, bool disposeNames = false)
    {
        _source = source;
        _disposeNames = disposeNames;
    }

    internal wasmtime_component_valrecord Value => _source;

    /// <summary>
    /// Sets the name and value of the entry at the specified offset.
    /// </summary>
    /// <param name="offset">The zero-based index of the entry to set.</param>
    /// <param name="name">The name to set for the entry.</param>
    /// <param name="value">The value to set for the entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int offset, ByteVector name, ComponentValue value)
    {
        var entry = &_source.data[offset];
        entry->name = name.Value;
        entry->val = value.Value;
    }

    /// <summary>
    /// Gets the value of the entry at the specified offset.
    /// </summary>
    /// <param name="offset">The zero-based index of the entry to get.</param>
    /// <returns>The value of the entry at the specified offset.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentValue Get(int offset)
    {
        return new ComponentValue(_source.data[offset].val, externallyOwned: true);
    }

    /// <summary>
    /// Gets an enumerator to iterate over the entries in the record.
    /// </summary>
    /// <returns>An enumerator to iterate over the entries in the record.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(_source);
    }

    /// <summary>
    /// Enumerates the entries in a record.
    /// </summary>
    public ref struct Enumerator
    {
        private readonly wasmtime_component_valrecord _record;
        private int _index;

        internal Enumerator(wasmtime_component_valrecord record)
        {
            _record = record;
            _index = -1;
        }

        /// <summary>
        /// Advances to the next entry in the record.
        /// </summary>
        public bool MoveNext()
        {
            _index++;
            return _index < (int)_record.size;
        }

        /// <summary>
        /// Gets the current entry in the record.
        /// </summary>
        public (ByteVector Name, ComponentValue Value) Current
        {
            get
            {
                var entry = &_record.data[_index];
                return (new ByteVector(entry->name), new ComponentValue(entry->val, externallyOwned: true));
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_source.data == null) return;

        var data = _source.data;

        for (var i = 0; i < (int)_source.size; i++)
        {
            if (_disposeNames)
            {
                new ByteVector(data[i].name).Dispose();
            }

            // 'wasmtime_component_valrecord_delete' will free the names,
            // since we don't want to dispose them, set the reference to null.
            data[i].name = default;
            ComponentValue.Dispose(ref data[i].val);
        }

        fixed (wasmtime_component_valrecord* p = &_source)
        {
            wasmtime_component_valrecord_delete(p);
        }
    }
}

