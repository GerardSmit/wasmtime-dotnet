using System;
using System.IO;
using Xunit;

namespace Wasmtime.Tests;

public class ComponentTest
{
    [Fact]
    public void UInt32()
    {
        using var state = new State();

        Assert.Equal(42u, state.Test.AddU32(40, 2));
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
