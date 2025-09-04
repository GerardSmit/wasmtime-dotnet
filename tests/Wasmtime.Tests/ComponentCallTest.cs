using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wasmtime.Tests;

public class ComponentCallTest(ComponentFixture fixture)
{
    [Fact]
    public void Dispose_Store()
    {
        using var state = fixture.CreateState();

        state.Store.Dispose();

        Assert.Throws<ObjectDisposedException>(() => state.Exports.Uppercase("uppercase"));
    }

    [Fact]
    public void MultipleCalls()
    {
        using var state = fixture.CreateState();

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal("UPPERCASE", state.Exports.Uppercase("uppercase"));
        }
    }

    [Fact]
    public async Task ConcurrentCalls_FunctionName()
    {
        using var state = fixture.CreateState();

        await ExecuteConcurrent(
            () => state.Exports,
            static s => Assert.Equal("UPPERCASE", s.Uppercase("uppercase")));
    }

    [Fact]
    public void GetFunction_InvalidIndex()
    {
        using var state = fixture.CreateState();

        var function = state.Instance.GetFunction("uppercase");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var a = ComponentValue.CreateString("uppercase", externallyOwned: false);
            using var results = state.Instance.Call(function, 1, [a]);
            results[-1].ToStringValue();
        });
    }

    [Fact]
    public async Task StressTest()
    {
        using var stateA = fixture.CreateState();
        using var stateB = fixture.CreateState();

        var entity = fixture.Imports.Entity;
        var expectedEntityDescription = $"Entity {entity.Id}: {entity.Name}";

        var upperCase = ExecuteConcurrent(
            () => stateA.Exports,
            static s => Assert.Equal("UPPERCASE", s.Uppercase("uppercase")));

        var lowerCase = ExecuteConcurrent(
            () => stateA.Exports,
            static s => Assert.Equal("foobar", s.HostCombineString("foo", "bar")));

        var entityDescription = ExecuteConcurrent(
            () => stateB.Exports,
            s => Assert.Equal(expectedEntityDescription, s.GetHostEntityDescription()));

        var sumNestedLists = ExecuteConcurrent(
            () => stateB.Exports,
            static s => Assert.Equal(6u, s.SumNestedList([[1, 2], [3]])));

        await Task.WhenAll(upperCase, lowerCase, entityDescription, sumNestedLists);
    }

    [Fact]
    public async Task StressTest_StatePerThread()
    {
        var entity = fixture.Imports.Entity;
        var expectedEntityDescription = $"Entity {entity.Id}: {entity.Name}";

        await ExecuteConcurrent(
            fixture.CreateState,
            s => Assert.Equal(expectedEntityDescription, s.Exports.GetHostEntityDescription()),
            s => s.Dispose());
    }
}
