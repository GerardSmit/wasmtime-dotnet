using System;
using System.Threading.Tasks;
using Xunit;

namespace Wasmtime.Tests;

[CollectionDefinition("Memory", DisableParallelization = true)]
public class ComponentCallMemory(ComponentFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public async Task StressTest()
    {
        using var state = fixture.CreateState();

        var largeString = new string('a', 16 * 1024);
        var result = largeString + largeString;

        var hostMemory = GetMemoryUsage();
        var guestMemory = state.Exports.GetMemoryUsage();

        output.WriteLine("Initial memory:");
        output.WriteLine($"   HOST:  {hostMemory:0.00} MB");
        output.WriteLine($"   GUEST: {guestMemory:0.00} MB");

        for (var i = 0; i < 100; i++)
        {
            await ExecuteConcurrent(
                () => state.Exports,
                s => Assert.Equal(result, s.HostCombineString(largeString, largeString)));

            var newHostMemory = GetMemoryUsage();
            var newGuestMemory = state.Exports.GetMemoryUsage();

            output.WriteLine("");
            output.WriteLine($"Iterations {i + 1}:");
            output.WriteLine($"   HOST:  {newHostMemory:0.00} MB (diff: {newHostMemory - hostMemory:0.00} MB), active values: {ComponentValue.ActiveCount}");
            output.WriteLine($"   GUEST: {newGuestMemory:0.00} MB (diff: {newGuestMemory - guestMemory:0.00} MB)");
            hostMemory = newHostMemory;
            guestMemory = newGuestMemory;

            ForceGc();
            state.Exports.ForceGc();
        }
    }

    private static double GetMemoryUsage() => GC.GetTotalMemory(true) / 1024.0 / 1024.0;

    private static void ForceGc()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
