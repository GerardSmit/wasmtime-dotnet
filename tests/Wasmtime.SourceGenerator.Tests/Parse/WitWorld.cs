using System.Collections.Immutable;
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

        var item = Assert.Single(world.Definitions.Items);
        var export = Assert.IsType<WitWorldExport>(item);
        Assert.Equal("foo", export.ExportName);
        Assert.Equal(WitTypeKind.Func, export.Type.Kind);
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

        var item1 = Assert.Single(world1.Definitions.Items);
        var func1 = Assert.IsType<WitWorldExport>(item1);
        Assert.Equal("foo", func1.ExportName);

        var world2 = version.Worlds["World2"];
        Assert.Equal("World2", world2.Name);

        var item2 = Assert.Single(world2.Definitions.Items);
        var func2 = Assert.IsType<WitWorldExport>(item2);
        Assert.Equal("bar", func2.ExportName);
    }


    [Fact]
    public void World_Record()
    {
        var file = Wit.Parse(
            """
            package test;

            world MyWorld {
                record point {
                    x: u32,
                    y: u32,
                }
            };
            """);

        var package = Assert.Single(file.Packages.Values);
        var version = Assert.Single(package.Versions.Values);
        var world = Assert.Single(version.Worlds.Values);
        Assert.Equal("MyWorld", world.Name);

        var item = Assert.Single(world.Definitions.Items);
        var export = Assert.IsType<WitRecord>(item);
        Assert.Equal("point", export.Name);

        var expected = new WitRecordType(
            export.Package,
            "point",
            ImmutableArray.Create(
                new WitField("x", WitType.U32),
                new WitField("y", WitType.U32)
            ));

        Assert.Equal(expected, export.Type);
    }
}
