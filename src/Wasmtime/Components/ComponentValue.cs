using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Represents a value used in component calls for Wasmtime.
/// </summary>
public struct ComponentValue : IDisposable
{
    // ** DO NOT ADD FIELDS TO THIS STRUCTURE. **
    // This struct is a direct mapping to the native wasmtime_component_val structure.
    // Adding fields will change the memory layout and break interop.
    private wasmtime_component_val _val;

    internal wasmtime_component_val Value => _val;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with an existing native handle.
    /// </summary>
    /// <param name="val">Pointer to the native component value.</param>
    internal ComponentValue(wasmtime_component_val val)
    {
        _val = val;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public ComponentValue(bool value)
    {
        _val.kind = 0;
        _val.of.boolean = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with an <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="sbyte"/> value.</param>
    public ComponentValue(sbyte value)
    {
        _val.kind = 1;
        _val.of.s8 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="byte"/> value.</param>
    public ComponentValue(byte value)
    {
        _val.kind = 2;
        _val.of.u8 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="short"/> value.
    /// </summary>
    /// <param name="value">The <see cref="short"/> value.</param>
    public ComponentValue(short value)
    {
        _val.kind = 3;
        _val.of.s16 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="ushort"/> value.
    /// </summary>
    /// <param name="value">The <see cref="ushort"/> value.</param>
    public ComponentValue(ushort value)
    {
        _val.kind = 4;
        _val.of.u16 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with an <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The <see cref="int"/> value.</param>
    public ComponentValue(int value)
    {
        _val.kind = 5;
        _val.of.s32 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The <see cref="uint"/> value.</param>
    public ComponentValue(uint value)
    {
        _val.kind = 6;
        _val.of.u32 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The <see cref="long"/> value.</param>
    public ComponentValue(long value)
    {
        _val.kind = 7;
        _val.of.s64 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The <see cref="ulong"/> value.</param>
    public ComponentValue(ulong value)
    {
        _val.kind = 8;
        _val.of.u64 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The <see cref="float"/> value.</param>
    public ComponentValue(float value)
    {
        _val.kind = 9;
        _val.of.f32 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The <see cref="double"/> value.</param>
    public ComponentValue(double value)
    {
        _val.kind = 10;
        _val.of.f64 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="char"/> value.
    /// </summary>
    /// <param name="value">The <see cref="char"/> value.</param>
    public ComponentValue(char value)
    {
        _val.kind = 11;
        _val.of.character = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="string"/> value.
    /// </summary>
    /// <param name="value">The <see cref="string"/> value.</param>
    public ComponentValue(string value)
    {
        _val.kind = 12;
        _val.of.@string = new ByteVector(value).Value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="ListBuilder"/> value.
    /// </summary>
    /// <param name="value">The <see cref="RecordBuilder"/> value.</param>
    public ComponentValue(ListBuilder value)
    {
        _val.kind = 13;
        _val.of.list = value.Value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="RecordBuilder"/> value.
    /// </summary>
    /// <param name="value">The <see cref="RecordBuilder"/> value.</param>
    public ComponentValue(RecordBuilder value)
    {
        _val.kind = 14;
        _val.of.record = value.Value;
    }

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the boolean.</returns>
    public static ComponentValue CreateBoolean(bool value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from an <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="sbyte"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the sbyte.</returns>
    public static ComponentValue CreateSByte(sbyte value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="byte"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the byte.</returns>
    public static ComponentValue CreateByte(byte value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="short"/> value.
    /// </summary>
    /// <param name="value">The <see cref="short"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the short.</returns>
    public static ComponentValue CreateInt16(short value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="ushort"/> value.
    /// </summary>
    /// <param name="value">The <see cref="ushort"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the ushort.</returns>
    public static ComponentValue CreateUInt16(ushort value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from an <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The <see cref="int"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the int.</returns>
    public static ComponentValue CreateInt32(int value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The <see cref="uint"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the uint.</returns>
    public static ComponentValue CreateUInt32(uint value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The <see cref="long"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the long.</returns>
    public static ComponentValue CreateInt64(long value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The <see cref="ulong"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the ulong.</returns>
    public static ComponentValue CreateUInt64(ulong value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The <see cref="float"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the float.</returns>
    public static ComponentValue CreateFloat(float value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The <see cref="double"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the double.</returns>
    public static ComponentValue CreateDouble(double value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="char"/> value.
    /// </summary>
    /// <param name="value">The <see cref="char"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the char.</returns>
    public static ComponentValue CreateChar(char value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="string"/> value.
    /// </summary>
    /// <param name="value">The <see cref="string"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the string.</returns>
    public static ComponentValue CreateString(string value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="RecordBuilder"/> value.
    /// </summary>
    /// <param name="value">The <see cref="RecordBuilder"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the memory.</returns>
    public static ComponentValue CreateRecord(RecordBuilder value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a <see cref="ListBuilder"/> value.
    /// </summary>
    /// <param name="value">The <see cref="ListBuilder"/> value.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the memory.</returns>
    public static ComponentValue CreateList(ListBuilder value) => new(value);

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <param name="toBytes">A function pointer to convert the enum to a <see cref="ByteVector"/>.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the enum.</returns>
    public static unsafe ComponentValue CreateEnum<T>(
        T value,
        delegate* managed<T, ByteVector> toBytes,
        bool copyConstants) where T : struct, Enum
    {
        var bytes = toBytes(value);
        var val = new wasmtime_component_val();
        val.kind = 17;
        val.of.enumeration = copyConstants ? new ByteVector(bytes).Value : bytes.Value;
        return new ComponentValue(val);
    }

    /// <summary>
    /// Creates a <see cref="ComponentValue"/> from a enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <param name="toBytes">A function pointer to convert the enum to a <see cref="ByteVector"/>.</param>
    /// <param name="expand">Expands the enum to its constituent values.</param>
    /// <returns>A <see cref="ComponentValue"/> representing the enum.</returns>
    public static unsafe ComponentValue CreateFlags<T>(
        T value,
        delegate* managed<T, ByteVector> toBytes,
        delegate* managed<T, Span<T>, int> expand,
        bool copyConstants
    ) where T : unmanaged, Enum
    {
        Span<T> values = stackalloc T[64];
        var count = expand(value, values);

        var val = new wasmtime_component_val();
        val.kind = 20;

        if (count != 0)
        {
            var builder = new FlagsBuilder(count, disposeValues: false);
            for (var i = 0; i < count; i++)
            {
                var bytes = toBytes(values[i]);
                builder[i] = copyConstants ? new ByteVector(bytes) : bytes;
            }

            val.of.flags = builder.Value;
        }

        return new ComponentValue(val);
    }

    public readonly bool ToBoolean()
    {
        if (_val.kind != 0) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Boolean.");
        return _val.of.boolean != 0;
    }

    public readonly sbyte ToSByte()
    {
        if (_val.kind != 1) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to SByte.");
        return _val.of.s8;
    }

    public readonly byte ToByte()
    {
        if (_val.kind != 2) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Byte.");
        return _val.of.u8;
    }

    public readonly short ToInt16()
    {
        if (_val.kind != 3) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Int16.");
        return _val.of.s16;
    }

    public readonly ushort ToUInt16()
    {
        if (_val.kind != 4) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to UInt16.");
        return _val.of.u16;
    }

    public readonly int ToInt32()
    {
        if (_val.kind != 5) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Int32.");
        return _val.of.s32;
    }

    public readonly uint ToUInt32()
    {
        if (_val.kind != 6) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to UInt32.");
        return _val.of.u32;
    }

    public readonly long ToInt64()
    {
        if (_val.kind != 7) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Int64.");
        return _val.of.s64;
    }

    public readonly ulong ToUInt64()
    {
        if (_val.kind != 8) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to UInt64.");
        return _val.of.u64;
    }

    public readonly float ToFloat()
    {
        if (_val.kind != 9) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Float.");
        return _val.of.f32;
    }

    public readonly double ToDouble()
    {
        if (_val.kind != 10) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Double.");
        return _val.of.f64;
    }

    public readonly char ToChar()
    {
        if (_val.kind != 11) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Char.");
        return (char)_val.of.character;
    }

    public readonly string ToStringValue()
    {
        if (_val.kind != 12) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to String.");
        return new ByteVector(_val.of.@string).GetString();
    }

    public readonly ListBuilder ToListBuilder()
    {
        if (_val.kind != 13) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to List.");
        return new ListBuilder(_val.of.list);
    }

    public readonly RecordBuilder ToRecordBuilder()
    {
        if (_val.kind != 14) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Record.");
        return new RecordBuilder(_val.of.record);
    }

    public readonly unsafe T ToEnum<T>(delegate* managed<ByteVector, T> toBytes) where T : struct, Enum
    {
        if (_val.kind != 17) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Enum.");
        return toBytes(new ByteVector(_val.of.enumeration));
    }

    public readonly unsafe T ToFlags<T>(
        delegate* managed<ByteVector, T> toEnum,
        delegate* managed<ReadOnlySpan<T>, T> combine) where T : unmanaged, Enum
    {
        if (_val.kind != 20) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Enum.");
        var flags = new FlagsBuilder(_val.of.flags);

        switch (flags.Length)
        {
            case 0:
                return default;
            case 1:
                return toEnum(flags[0]);
            default:
            {
                Span<T> enums = stackalloc T[64];

                for (var i = 0; i < flags.Length; i++)
                {
                    enums[i] = toEnum(new ByteVector(flags.Value.data[i]));
                }

                return combine(enums.Slice(0, flags.Length));
            }
        }
    }

    public unsafe ComponentCallResults ToTuple()
    {
        if (_val.kind != 15) throw new InvalidOperationException($"Cannot convert ComponentValue of kind {_val.kind} to Tuple.");
        var tuple = _val.of.tuple;

        return new ComponentCallResults(tuple.data, (int)tuple.size);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(ref _val);
    }

    internal static void Dispose(ref wasmtime_component_val val)
    {
        switch (val.kind)
        {
            case 0:
                return;
            case 12:
                new ByteVector(val.of.@string).Dispose();
                break;
            case 13:
                new ListBuilder(val.of.list).Dispose();
                break;
            case 14:
                new RecordBuilder(val.of.record).Dispose();
                break;
            case 17:
                // Enums are not disposed since the values are cached and reused (constants).
                break;
            case 20:
                new FlagsBuilder(val.of.flags).Dispose();
                break;
        }

        val = default;
    }
}