using Xunit;

namespace Wasmtime.Tests;

public class ComponentImportTest(ComponentFixture fixture)
{
    [Fact]
    public void Import_Void()
    {
        using var state = fixture.CreateState();

        state.Exports.HostCallback();

        Assert.True(fixture.Imports.CallbackWasCalled);
    }

    [Fact]
    public void Import_ParametersAndResult()
    {
        using var state = fixture.CreateState();

        var result = state.Exports.HostCombineString("foo", "bar");

        Assert.True(fixture.Imports.CallbackCombineStringWasCalled);
        Assert.Equal("foobar", result);
    }
}
