using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Tests;

public class WitWorld
{
    [Fact]
    public void Parse_Single_World()
    {
        var file = Wit.Parse(
            """
            package test;

            world MyWorld {
                export foo: func();
            };
            """);

        var package = Assert.Single(file.Packages.Values);
        var version = Assert.Single(package.Versions.Values);
        var world = Assert.Single(version.Worlds.Values);
        Assert.Equal("MyWorld", world.Name);

        var func = Assert.Single(world.Exports);
        Assert.Equal("foo", func.Key);
        Assert.Equal(WitTypeKind.Func, func.Value.Kind);
    }

    [Fact]
    public void Parse_Multiple_Worlds()
    {
        var file = Wit.Parse(
            """
            package test;

            world World1 {
                export foo: func();
            }

            world World2 {
                export bar: func();
            }
            """);

        var package = Assert.Single(file.Packages.Values);
        var version = Assert.Single(package.Versions.Values);
        Assert.Equal(2, version.Worlds.Count);

        var world1 = version.Worlds["World1"];
        Assert.Equal("World1", world1.Name);

        var func1 = Assert.Single(world1.Exports);
        Assert.Equal("foo", func1.Key);

        var world2 = version.Worlds["World2"];
        Assert.Equal("World2", world2.Name);

        var func2 = Assert.Single(world2.Exports);
        Assert.Equal("bar", func2.Key);
    }
}
