using System;
using System.Threading.Tasks;
using Xunit;

namespace Wasmtime.Tests;

[CollectionDefinition("Memory", DisableParallelization = true)]
public class ComponentCallMemory(ComponentFixture fixture, ITestOutputHelper output)
{
    // WASM in 32-bit mode has a 4 GB memory limit
    // We call 5 GB in total to validate that the memory is properly released between calls.

    private const int Size = 100 * 1024 * 1024; // 100 MB
    private const int Iterations = 50; // 100 * 100 MB = 5 GB total

    [Fact]
    public void Host_To_Guest()
    {
#if SUITE_CSHARP
        Assert.Skip("Open issue in wit-bindgen: https://github.com/bytecodealliance/wit-bindgen/issues/1377");
#endif

        using var state = fixture.CreateState();

        var str = new string('a', Size);

        for (var i = 1; i <= Iterations; i++)
        {
            state.Exports.AcceptString(str);

            if (i % 10 == 0)
            {
                output.WriteLine($"Iteration {i}");
            }
        }
    }

    [Fact]
    public void Guest_To_Host()
    {
        using var state = fixture.CreateState();

        for (var i = 1; i <= Iterations; i++)
        {
            state.Exports.ReturnString(Size);

            if (i % 10 == 0)
            {
                output.WriteLine($"Iteration {i}");
            }
        }
    }
}
