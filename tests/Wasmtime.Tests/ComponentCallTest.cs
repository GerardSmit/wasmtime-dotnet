using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Wasmtime.Tests;

public class ComponentCallTest
{
    [Fact]
    public void Dispose_Store()
    {
        using var state = new ComponentState();

        state.Store.Dispose();

        Assert.Throws<ObjectDisposedException>(() => state.Test.Uppercase("uppercase"));
    }

    [Fact]
    public void GetFunction_CallUnsafe()
    {
        using var state = new ComponentState();

        var function = state.Instance.GetFunction("uppercase");

        using var a = ComponentValue.CreateString("uppercase");
        using var results = state.Instance.CallUnsafe(function, 1, [a]);

        Assert.Equal("UPPERCASE", results.GetString(0));
    }

    [Fact]
    public void MultipleCalls()
    {
        using var state = new ComponentState();

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal("UPPERCASE", state.Test.Uppercase("uppercase"));
        }
    }

    [Fact]
    public async Task ConcurrentCalls_FunctionName()
    {
        using var state = new ComponentState();

        await ExecuteConcurrent(
            () => state.Test,
            s => Assert.Equal("UPPERCASE", s.Uppercase("uppercase")));
    }

    [Fact]
    public async Task ConcurrentCalls_FunctionInstance()
    {
        using var state = new ComponentState();

        await ExecuteConcurrent(
            () => (
                Wit: state.Test,
                Function: state.Instance.GetFunction("uppercase")
            ),
            s => Assert.Equal("UPPERCASE", s.Wit.Uppercase(s.Function, "uppercase")));
    }

    private static async Task ExecuteConcurrent<TState>(Func<TState> createState, Action<TState> action)
    {
        var cpuCount = Environment.ProcessorCount;
        var ready = new ManualResetEventSlim(initialState: false);
        var threads = new (Thread Thread, TaskCompletionSource CompletionSource)[cpuCount];
        var calls = 0;

        const int callsPerThread = 100;
        var expectedCalls = cpuCount * callsPerThread;

        // Set up threads
        for (var i = 0; i < cpuCount; i++)
        {
            var taskCompletionSource = new TaskCompletionSource();
            var thread = new Thread(_ =>
            {
                ready.Wait();

                var state = createState();

                // ReSharper disable once AccessToDisposedClosure
                for (var j = 0; j < callsPerThread; j++)
                {
                    action(state);
                    Interlocked.Increment(ref calls);
                }

                taskCompletionSource.SetResult();
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
