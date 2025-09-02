using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Tests;

public class WitFunc
{
    [Fact]
    public void Empty()
    {
        var func = ParseFunc("func()");

        Assert.Empty(func.Parameters);
        Assert.Empty(func.Results);
    }

    [Fact]
    public void Result()
    {
        var func = ParseFunc("func() -> u32");

        Assert.Empty(func.Parameters);

        var result = Assert.Single(func.Results);
        Assert.Equal(WitTypeKind.U32, result.Kind);
    }

    [Fact]
    public void SingleParameter()
    {
        var func = ParseFunc("func(a: u32)");

        var param = Assert.Single(func.Parameters);
        Assert.Equal("a", param.Name);
        Assert.Equal(WitTypeKind.U32, param.Type.Kind);

        Assert.Empty(func.Results);
    }

    [Fact]
    public void MultipleParameters()
    {
        var func = ParseFunc("func(a:u32, b:f64, c:string)");

        Assert.Equal(3, func.Parameters.Length);

        var param1 = func.Parameters[0];
        Assert.Equal("a", param1.Name);
        Assert.Equal(WitTypeKind.U32, param1.Type.Kind);

        var param2 = func.Parameters[1];
        Assert.Equal("b", param2.Name);
        Assert.Equal(WitTypeKind.F64, param2.Type.Kind);

        var param3 = func.Parameters[2];
        Assert.Equal("c", param3.Name);
        Assert.Equal(WitTypeKind.String, param3.Type.Kind);

        Assert.Empty(func.Results);
    }

    [Fact]
    public void MultipleParametersAndResult()
    {
        var func = ParseFunc("func(a:u32, b:f64) -> string");

        Assert.Equal(2, func.Parameters.Length);

        var param1 = func.Parameters[0];
        Assert.Equal("a", param1.Name);
        Assert.Equal(WitTypeKind.U32, param1.Type.Kind);

        var param2 = func.Parameters[1];
        Assert.Equal("b", param2.Name);
        Assert.Equal(WitTypeKind.F64, param2.Type.Kind);

        var result = Assert.Single(func.Results);
        Assert.Equal(WitTypeKind.String, result.Kind);
    }

    private static WitFuncType ParseFunc(string input)
    {
        var file = Wit.Parse(
            $$"""
            package test;

            world World {
                export test: {{input}};
            }
            """);

        var package = Assert.Single(file.Packages.Values);
        var version = Assert.Single(package.Versions.Values);
        var world = Assert.Single(version.Worlds.Values);
        var item = Assert.Single(world.Definitions.Items);
        var export = Assert.IsType<WitWorldExport>(item);

        return Assert.IsType<WitFuncType>(export.Type);
    }
}
