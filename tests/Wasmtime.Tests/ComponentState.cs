using System;
using System.IO;

namespace Wasmtime.Tests;

internal readonly struct ComponentState : IDisposable
{
    public readonly Engine Engine;
    public readonly Linker Linker;
    public readonly Store Store;
    public readonly Component Component;
    public readonly ComponentInstance Instance;
    public readonly Wit.Tests.Component.TestExports Exports;
    public readonly TestImportsImpl Imports;

    public ComponentState()
    {
        Engine = new Engine();
        Linker = new Linker(Engine);
        Linker.AddWasiP2();

        Imports = new TestImportsImpl();
        Linker.Define(Imports);

        Store = new Store(Engine);
        Store.AddWasiP2();

        var bytes = File.ReadAllBytes("component.wasm");
        Component = Component.Compile(Engine, bytes);

        Instance = Store.GetComponentInstance(Component, Linker);
        Exports = new Wit.Tests.Component.TestExports(Instance);
    }

    public void Dispose()
    {
        Component.Dispose();
        Store.Dispose();
        Linker.Dispose();
        Engine.Dispose();
    }
}
