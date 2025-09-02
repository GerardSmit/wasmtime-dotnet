using System;
using System.IO;
using Xunit;

namespace Wasmtime.Tests;

public class ComponentCallTypeTest
{
    [Fact]
    public void SInt8()
    {
        using var state = new ComponentState();

        Assert.Equal((sbyte)42, state.Test.AddS8(40, 2));
    }

    [Fact]
    public void UInt8()
    {
        using var state = new ComponentState();

        Assert.Equal((byte)42, state.Test.AddU8(40, 2));
    }

    [Fact]
    public void SInt16()
    {
        using var state = new ComponentState();

        Assert.Equal((short)42, state.Test.AddS16(40, 2));
    }

    [Fact]
    public void UInt16()
    {
        using var state = new ComponentState();

        Assert.Equal((ushort)42, state.Test.AddU16(40, 2));
    }

    [Fact]
    public void SInt32()
    {
        using var state = new ComponentState();

        Assert.Equal(42, state.Test.AddS32(40, 2));
    }

    [Fact]
    public void UInt32()
    {
        using var state = new ComponentState();

        Assert.Equal(42u, state.Test.AddU32(40, 2));
    }

    [Fact]
    public void SInt64()
    {
        using var state = new ComponentState();

        Assert.Equal(42L, state.Test.AddS64(40L, 2L));
    }

    [Fact]
    public void UInt64()
    {
        using var state = new ComponentState();

        Assert.Equal(42UL, state.Test.AddU64(40UL, 2UL));
    }

    [Fact]
    public void Float32()
    {
        using var state = new ComponentState();

        Assert.Equal(4.2f, state.Test.AddF32(4.0f, 0.2f), 3);
    }

    [Fact]
    public void Float64()
    {
        using var state = new ComponentState();

        Assert.Equal(4.2, state.Test.AddF64(4.0, 0.2), 5);
    }

    [Fact]
    public void String()
    {
        using var state = new ComponentState();

        Assert.Equal("UPPERCASE", state.Test.Uppercase("uppercase"));
    }

    [Fact]
    public void Void()
    {
        using var state = new ComponentState();

        Assert.False(state.Test.GetFlag());
        state.Test.SetFlag(true);
        Assert.True(state.Test.GetFlag());
    }

    [Fact]
    public void Record()
    {
        using var state = new ComponentState();

        var result = state.Test.AddPoint(
            new() { X = 1, Y = 3 },
            new() { X = 2, Y = 4 });

        Assert.Equal(3, result.X);
        Assert.Equal(7, result.Y);
    }

    [Fact]
    public void Record_Nested()
    {
        using var state = new ComponentState();

        state.Test.RegisterEntity(new()
        {
            Id = 1,
            Name = "Entity",
            Position = new()
            {
                X = 10,
                Y = 20
            }
        });

        var entity = state.Test.GetEntity(1);
        Assert.Equal(1, entity.Id);
        Assert.Equal("Entity", entity.Name);
        Assert.Equal(10, entity.Position.X);
        Assert.Equal(20, entity.Position.Y);
    }
}


internal readonly struct ComponentState : IDisposable
{
    public readonly Engine Engine;
    public readonly Linker Linker;
    public readonly Store Store;
    public readonly Component Component;
    public readonly ComponentInstance Instance;
    public readonly Wit.Tests.Component.Test Test;

    public ComponentState()
    {
        Engine = new Engine();
        Linker = new Linker(Engine);
        Linker.AddWasiP2();

        Store = new Store(Engine);
        Store.AddWasiP2();

        var bytes = File.ReadAllBytes("component.wasm");
        Component = Component.Compile(Engine, bytes);

        Instance = Store.GetComponentInstance(Component, Linker);
        Test = new Wit.Tests.Component.Test(Instance);
    }

    public void Dispose()
    {
        Component.Dispose();
        Store.Dispose();
        Linker.Dispose();
        Engine.Dispose();
    }
}
