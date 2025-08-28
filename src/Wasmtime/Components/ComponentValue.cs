using System;
using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Represents a value used in component calls for Wasmtime.
/// </summary>
public unsafe struct ComponentValue : IDisposable
{
    private wasmtime_component_val* _val;

    /// <summary>
    /// Gets or sets the internal handle to the native component value.
    /// </summary>
    internal wasmtime_component_val* Handle
    {
        get => _val;
        set => _val = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with an existing native handle.
    /// </summary>
    /// <param name="val">Pointer to the native component value.</param>
    internal ComponentValue(wasmtime_component_val* val)
    {
        _val = val;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public ComponentValue(bool value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 0;
        _val->of.boolean = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with an <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="sbyte"/> value.</param>
    public ComponentValue(sbyte value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 1;
        _val->of.s8 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="byte"/> value.</param>
    public ComponentValue(byte value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 2;
        _val->of.u8 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="short"/> value.
    /// </summary>
    /// <param name="value">The <see cref="short"/> value.</param>
    public ComponentValue(short value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 3;
        _val->of.s16 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="ushort"/> value.
    /// </summary>
    /// <param name="value">The <see cref="ushort"/> value.</param>
    public ComponentValue(ushort value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 4;
        _val->of.u16 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with an <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The <see cref="int"/> value.</param>
    public ComponentValue(int value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 5;
        _val->of.s32 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The <see cref="uint"/> value.</param>
    public ComponentValue(uint value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 6;
        _val->of.u32 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The <see cref="long"/> value.</param>
    public ComponentValue(long value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 7;
        _val->of.s64 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The <see cref="ulong"/> value.</param>
    public ComponentValue(ulong value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 8;
        _val->of.u64 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The <see cref="float"/> value.</param>
    public ComponentValue(float value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 9;
        _val->of.f32 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The <see cref="double"/> value.</param>
    public ComponentValue(double value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 10;
        _val->of.f64 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="char"/> value.
    /// </summary>
    /// <param name="value">The <see cref="char"/> value.</param>
    public ComponentValue(char value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 11;
        _val->of.character = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentValue"/> struct with a <see cref="string"/> value.
    /// </summary>
    /// <param name="value">The <see cref="string"/> value.</param>
    public ComponentValue(string value)
    {
        _val = wasmtime_component_val_new();
        _val->kind = 12;
        _val->of.@string = new ByteVector(value).Handle;
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

    /// <inheritdoc />
    public void Dispose()
    {
        if (_val->kind == 12)
        {
            wasm_byte_vec_delete(&_val->of.@string);
        }

        if (_val != null)
        {
            wasmtime_component_val_delete(_val);
            _val = null;
        }
    }
}