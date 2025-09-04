using System;
using System.IO;
using Wasmtime.Tests;
using Xunit;

[assembly: AssemblyFixture(typeof(ComponentFixture))]

namespace Wasmtime.Tests;

public class ComponentFixture : IDisposable
{
    public readonly Engine Engine;
    public readonly Linker Linker;
    public readonly Component Component;
    internal readonly TestImportsImpl Imports;

    public ComponentFixture()
    {
        Engine = new Engine();
        Linker = new Linker(Engine);
        Linker.AddWasiP2();

        Imports = new TestImportsImpl();
        Linker.Define(Imports);

        var bytes = File.ReadAllBytes("component.wasm");
        Component = Component.Compile(Engine, bytes);
    }

    public ComponentState CreateState()
    {
        return new ComponentState(Engine, Linker, Component);
    }

    public void Dispose()
    {
        Component.Dispose();
        Linker.Dispose();
        Engine.Dispose();
    }

    public readonly struct ComponentState : IDisposable
    {
        public readonly Store Store;
        internal readonly Wit.Tests.Component.TestExports Exports;
        public readonly ComponentInstance Instance;

        public ComponentState(Engine engine, Linker linker, Component component)
        {
            Store = new Store(engine);
            Store.AddWasiP2();

            Instance = Store.GetComponentInstance(component, linker);
            Exports = new Wit.Tests.Component.TestExports(Instance);
        }

        public void Dispose()
        {
            Store.Dispose();
        }
    }
}