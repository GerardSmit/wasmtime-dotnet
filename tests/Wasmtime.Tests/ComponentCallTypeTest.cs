using Xunit;

namespace Wasmtime.Tests;

public class ComponentCallTypeTest
{
    [Fact]
    public void SInt8()
    {
        using var state = new ComponentState();

        Assert.Equal((sbyte)42, state.Exports.AddS8(40, 2));
    }

    [Fact]
    public void UInt8()
    {
        using var state = new ComponentState();

        Assert.Equal((byte)42, state.Exports.AddU8(40, 2));
    }

    [Fact]
    public void SInt16()
    {
        using var state = new ComponentState();

        Assert.Equal((short)42, state.Exports.AddS16(40, 2));
    }

    [Fact]
    public void UInt16()
    {
        using var state = new ComponentState();

        Assert.Equal((ushort)42, state.Exports.AddU16(40, 2));
    }

    [Fact]
    public void SInt32()
    {
        using var state = new ComponentState();

        Assert.Equal(42, state.Exports.AddS32(40, 2));
    }

    [Fact]
    public void UInt32()
    {
        using var state = new ComponentState();

        Assert.Equal(42u, state.Exports.AddU32(40, 2));
    }

    [Fact]
    public void SInt64()
    {
        using var state = new ComponentState();

        Assert.Equal(42L, state.Exports.AddS64(40L, 2L));
    }

    [Fact]
    public void UInt64()
    {
        using var state = new ComponentState();

        Assert.Equal(42UL, state.Exports.AddU64(40UL, 2UL));
    }

    [Fact]
    public void Float32()
    {
        using var state = new ComponentState();

        Assert.Equal(4.2f, state.Exports.AddF32(4.0f, 0.2f), 3);
    }

    [Fact]
    public void Float64()
    {
        using var state = new ComponentState();

        Assert.Equal(4.2, state.Exports.AddF64(4.0, 0.2), 5);
    }

    [Fact]
    public void String()
    {
        using var state = new ComponentState();

        Assert.Equal("UPPERCASE", state.Exports.Uppercase("uppercase"));
    }

    [Fact]
    public void Void()
    {
        using var state = new ComponentState();

        Assert.False(state.Exports.GetFlag());
        state.Exports.SetFlag(true);
        Assert.True(state.Exports.GetFlag());
    }

    [Fact]
    public void Record()
    {
        using var state = new ComponentState();

        var result = state.Exports.AddPoint(
            new() { X = 1, Y = 3 },
            new() { X = 2, Y = 4 });

        Assert.Equal(3, result.X);
        Assert.Equal(7, result.Y);
    }

    [Fact]
    public void Record_Nested()
    {
        using var state = new ComponentState();

        state.Exports.RegisterEntity(new()
        {
            Id = 1,
            Name = "Entity",
            Position = new()
            {
                X = 10,
                Y = 20
            }
        });

        var entity = state.Exports.GetEntity(1);
        Assert.Equal(1, entity.Id);
        Assert.Equal("Entity", entity.Name);
        Assert.Equal(10, entity.Position.X);
        Assert.Equal(20, entity.Position.Y);
    }
}