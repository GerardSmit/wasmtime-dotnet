using System;
using System.Runtime.CompilerServices;
using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Simple builder for creating and manipulating records of record entries.
/// </summary>
public readonly unsafe ref struct RecordBuilder
{
    private readonly wasmtime_component_valrecord _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBuilder"/> struct with the specified items and length.
    /// </summary>
    /// <param name="ptr">An points of <see cref="RecordBuilderItem"/> representing the items in the record.</param>
    /// <param name="length">The number of items in the record.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than zero.</exception>
    public RecordBuilder(RecordBuilderItem* ptr, int length)
    {
        _source = new wasmtime_component_valrecord
        {
            data = (wasmtime_component_valrecord_entry*)ptr,
            size = (UIntPtr)length
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBuilder"/> struct with the specified source.
    /// </summary>
    /// <param name="source">The source <see cref="wasmtime_component_valrecord"/> to wrap.</param>
    internal RecordBuilder(wasmtime_component_valrecord source)
    {
        _source = source;
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
    /// <param name="offset">The zero-based index of the entry to get.</param
    /// <returns>The value of the entry at the specified offset.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComponentValue Get(int offset)
    {
        return new ComponentValue(_source.data[offset].val);
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
                return (new ByteVector(entry->name), new ComponentValue(entry->val));
            }
        }
    }
}

