using System;
using System.Threading.Tasks;
using Xunit;

namespace Wasmtime.Tests;

[CollectionDefinition("Memory", DisableParallelization = true)]
public class ComponentCallMemory(ComponentFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public void ReturnString()
    {
        using var state = fixture.CreateState();

        for (var i = 0; i < 500_000; i++)
        {
            state.Exports.ReturnString(65536 * 2);

            if (i % 100 == 0)
            {
                output.WriteLine($"Iteration {i}");
            }
        }
    }
}
