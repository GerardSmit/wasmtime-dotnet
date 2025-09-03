using Xunit;

namespace Wasmtime.Tests;

public class ComponentImportTest
{
    [Fact]
    public void Import_Void()
    {
        using var state = new ComponentState();

        state.Exports.HostCallback();

        Assert.True(state.Imports.WasCalled);
    }

    [Fact]
    public void Import_ParametersAndResult()
    {
        using var state = new ComponentState();

        var result = state.Exports.HostCombineString("foo", "bar");

        Assert.True(state.Imports.WasCalled);
        Assert.Equal("foobar", result);
    }
}
