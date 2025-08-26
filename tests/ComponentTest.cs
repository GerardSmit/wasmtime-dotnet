using System.IO;
using Xunit;

namespace Wasmtime.Tests;

public class ComponentTest
{
    [Fact]
    public void Compile()
    {
        const string addIntModule =
            """
            (component
              (core module $AddModule
                (func (export "return") (param i32) (param i32) (result i32)
                  local.get 0
                  local.get 1
                  i32.add
                )
              )

              (core instance $add_instance (instantiate $AddModule))

              (func (export "return") (param "a" s32) (param "b" s32) (result s32)
                (canon lift
                  (core func $add_instance "return")
                )
              )
            )
            """;

        using var engine = new Engine();
        using var linker = new Linker(engine);
        using var store = new Store(engine);

        using var component = Component.Compile(engine, addIntModule);

        var instance = component.CreateInstance(linker, store);

        using var a = ComponentValue.CreateInt32(40);
        using var b = ComponentValue.CreateInt32(2);

        for (var i = 0; i < 10; i++)
        {
            using var result = instance.Call("return", [a, b]);

            Assert.Equal(42, result.GetInt32(0));
        }
    }

    [Fact]
    public void From_NativeAOT_LLVM()
    {
        using var engine = new Engine();
        using var linker = new Linker(engine);
        linker.AddWasiP2();

        using var store = new Store(engine);
        store.AddWasiP2();

        var bytes = File.ReadAllBytes("Wasm/Adder.wasm");
        using var component = Component.Compile(engine, bytes);

        var instance = component.CreateInstance(linker, store);

        using var a = ComponentValue.CreateUInt32(40);
        using var b = ComponentValue.CreateUInt32(2);

        for (var i = 0; i < 10; i++)
        {
            using var result = instance.Call("add", [a, b]);

            Assert.Equal((uint)42, result.GetUInt32(0));
        }
    }
}
