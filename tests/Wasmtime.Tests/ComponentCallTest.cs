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
    public void GetFunction_CallUnsafe()
    {
        using var state = fixture.CreateState();

        var function = state.Instance.GetFunction("uppercase");

        using var a = ComponentValue.CreateString("uppercase");
        using var results = state.Instance.CallUnsafe(function, 1, [a]);

        Assert.Equal("UPPERCASE", results.GetString(0));
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
    public void GetFunction_CallUnsafe_InvalidIndex()
    {
        using var state = fixture.CreateState();

        var function = state.Instance.GetFunction("uppercase");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var a = ComponentValue.CreateString("uppercase");
            using var results = state.Instance.CallUnsafe(function, 1, [a]);
            results.GetString(-1);
        });
    }

    private static async Task ExecuteConcurrent<TState>(Func<TState> createState, Action<TState> action)
    {
        var cpuCount = Environment.ProcessorCount;
        var ready = new ManualResetEventSlim(initialState: false);
        var threads = new (Thread Thread, TaskCompletionSource<bool> CompletionSource)[cpuCount];
        var calls = 0;

        const int callsPerThread = 100;
        var expectedCalls = cpuCount * callsPerThread;

        // Set up threads
        for (var i = 0; i < cpuCount; i++)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var thread = new Thread(_ =>
            {
                ready.Wait();

                var state = createState();

                for (var j = 0; j < callsPerThread; j++)
                {
                    action(state);
                    Interlocked.Increment(ref calls);
                }

                taskCompletionSource.SetResult(true);
            });

            threads[i] = (thread, taskCompletionSource);

            thread.Start();
        }

        // Start calls
        ready.Set();

        // Wait for calls to finish.
        await Task.WhenAll(threads.Select(x => x.CompletionSource.Task));

        // Check call count
        Assert.Equal(expectedCalls, calls);
    }
}
