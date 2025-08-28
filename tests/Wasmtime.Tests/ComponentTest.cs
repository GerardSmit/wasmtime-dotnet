using System;
using System.IO;
using Xunit;

namespace Wasmtime.Tests;

public class ComponentTest
{
    [Fact]
    public void SInt8()
    {
        using var state = new State();

        Assert.Equal((sbyte)42, state.Test.AddS8(40, 2));
    }

    [Fact]
    public void UInt8()
    {
        using var state = new State();

        Assert.Equal((byte)42, state.Test.AddU8(40, 2));
    }

    [Fact]
    public void SInt16()
    {
        using var state = new State();

        Assert.Equal((short)42, state.Test.AddS16(40, 2));
    }

    [Fact]
    public void UInt16()
    {
        using var state = new State();

        Assert.Equal((ushort)42, state.Test.AddU16(40, 2));
    }

    [Fact]
    public void SInt32()
    {
        using var state = new State();

        Assert.Equal(42, state.Test.AddS32(40, 2));
    }

    [Fact]
    public void UInt32()
    {
        using var state = new State();

        Assert.Equal(42u, state.Test.AddU32(40, 2));
    }

    [Fact]
    public void SInt64()
    {
        using var state = new State();

        Assert.Equal(42L, state.Test.AddS64(40L, 2L));
    }

    [Fact]
    public void UInt64()
    {
        using var state = new State();

        Assert.Equal(42UL, state.Test.AddU64(40UL, 2UL));
    }

    [Fact]
    public void Float32()
    {
        using var state = new State();

        Assert.Equal(4.2f, state.Test.AddF32(4.0f, 0.2f), 3);
    }

    [Fact]
    public void Float64()
    {
        using var state = new State();

        Assert.Equal(4.2, state.Test.AddF64(4.0, 0.2), 5);
    }

    [Fact]
    public void String()
    {
        using var state = new State();

        Assert.Equal("UPPERCASE", state.Test.Uppercase("uppercase"));
    }

    [Fact]
    public void Void()
    {
        using var state = new State();

        Assert.False(state.Test.GetFlag());
        state.Test.SetFlag(true);
        Assert.True(state.Test.GetFlag());
    }

    private struct State : IDisposable
    {
        public readonly Engine Engine;
        public readonly Linker Linker;
        public readonly Store Store;
        public readonly Component Component;
        public readonly ComponentInstance Instance;
        public readonly Wit.Tests.Test.Test_ Test;

        public State()
        {
            Engine = new Engine();
            Linker = new Linker(Engine);
            Linker.AddWasiP2();

            Store = new Store(Engine);
            Store.AddWasiP2();

            var bytes = File.ReadAllBytes("component.wasm");
            Component = Component.Compile(Engine, bytes);

            Instance = Component.CreateInstance(Linker, Store);
            Test = new Wit.Tests.Test.Test_(Instance);
        }

        public void Dispose()
        {
            Component.Dispose();
            Store.Dispose();
            Linker.Dispose();
            Engine.Dispose();
        }
    }
}
