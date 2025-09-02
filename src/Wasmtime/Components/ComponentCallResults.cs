using System;
using System.Threading;
using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Represents the results of a component call.
/// After use, the instance must be disposed. Otherwise, the function cannot be called again.
/// </summary>
/// <remarks>
/// This struct wraps around <see cref="ComponentCallResultsInternal"/> to ensure that the instance
/// is not used in async (the instance is thread-static).
/// </remarks>
public readonly unsafe ref struct ComponentCallResults : IDisposable
{
    private readonly wasmtime_component_val* _val;
    private readonly SemaphoreSlim? _semaphore;
    private readonly ComponentCallResultsInternal? _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentCallResults"/> struct.
    /// </summary>
    /// <param name="result">The internal result structure to wrap.</param>
    /// <param name="semaphore">The semaphore to release when disposed.</param>
    internal ComponentCallResults(SemaphoreSlim? semaphore, ComponentCallResultsInternal result)
    {
        _semaphore = semaphore;
        _result = result;
        Length = result.Length;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentCallResults"/> struct.
    /// </summary>
    /// <param name="val">The pointer to the array of result values.</param>
    /// <param name="length">The number of result values.</param>
    internal ComponentCallResults(wasmtime_component_val* val, int length)
    {
        _val = val;
        Length = length;
    }

    /// <summary>
    /// Gets the number of results returned by the component call.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the <see cref="bool"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="bool"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="bool"/>.</exception>
    public bool GetBoolean(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 0 ? (val.of.boolean != 0) : throw new InvalidOperationException("Value is not a boolean");
    }

    /// <summary>
    /// Gets the <see cref="sbyte"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="sbyte"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not an <see cref="sbyte"/>.</exception>
    public sbyte GetSByte(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 1 ? val.of.s8 : throw new InvalidOperationException("Value is not an sbyte");
    }

    /// <summary>
    /// Gets the <see cref="byte"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="byte"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="byte"/>.</exception>
    public byte GetByte(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 2 ? val.of.u8 : throw new InvalidOperationException("Value is not a byte");
    }

    /// <summary>
    /// Gets the <see cref="short"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="short"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="short"/>.</exception>
    public short GetInt16(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 3 ? val.of.s16 : throw new InvalidOperationException("Value is not an int16");
    }

    /// <summary>
    /// Gets the <see cref="ushort"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="ushort"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="ushort"/>.</exception>
    public ushort GetUInt16(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 4 ? val.of.u16 : throw new InvalidOperationException("Value is not a uint16");
    }

    /// <summary>
    /// Gets the <see cref="int"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="int"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not an <see cref="int"/>.</exception>
    public int GetInt32(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 5 ? val.of.s32 : throw new InvalidOperationException("Value is not an int32");
    }

    /// <summary>
    /// Gets the <see cref="uint"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="uint"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="uint"/>.</exception>
    public uint GetUInt32(int index)
    {
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 6 ? val.of.u32 : throw new InvalidOperationException("Value is not a uint32");
    }

    /// <summary>
    /// Gets the <see cref="long"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="long"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="long"/>.</exception>
    public long GetInt64(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 7 ? val.of.s64 : throw new InvalidOperationException("Value is not an int64");
    }

    /// <summary>
    /// Gets the <see cref="ulong"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="ulong"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="ulong"/>.</exception>
    public ulong GetUInt64(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 8 ? val.of.u64 : throw new InvalidOperationException("Value is not a uint64");
    }

    /// <summary>
    /// Gets the <see cref="float"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="float"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="float"/>.</exception>
    public float GetFloat(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 9 ? val.of.f32 : throw new InvalidOperationException("Value is not a float");
    }

    /// <summary>
    /// Gets the <see cref="double"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="double"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="double"/>.</exception>
    public double GetDouble(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 10 ? val.of.f64 : throw new InvalidOperationException("Value is not a double");
    }

    /// <summary>
    /// Gets the <see cref="char"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="char"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="char"/>.</exception>
    public char GetChar(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 11 ? (char)val.of.character : throw new InvalidOperationException("Value is not a char");
    }

    /// <summary>
    /// Gets the <see cref="string"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="string"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="string"/>.</exception>
    public unsafe string GetString(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 12 ? new ByteVector(&val.of.@string).GetString() : throw new InvalidOperationException("Value is not a string");
    }

    /// <summary>
    /// Gets the <see cref="RecordBuilder"/> result at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the result.</param>
    /// <returns>The <see cref="RecordBuilder"/> value at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the value at the index is not a <see cref="RecordBuilder"/>.</exception>
    public RecordBuilder GetRecordBuilder(int index)
    {
        if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
        var val = _result?.Array[index] ?? _val[index];
        return val.kind == 14 ? new RecordBuilder(val.of.record) : throw new InvalidOperationException("Value is not a record");
    }

    /// <summary>
    /// Releases resources associated with this instance.
    /// </summary>
    public void Dispose()
    {
        _result?.Dispose();
        _semaphore?.Release();
    }
}