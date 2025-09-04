using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wasmtime.Tests;

public static class Methods
{
    public static async Task ExecuteConcurrent<TState>(
        Func<TState> createState,
        Action<TState> action,
        Action<TState> cleanup = null)
    {
        using var ready = new ManualResetEventSlim(initialState: false);
        var cpuCount = Environment.ProcessorCount;
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

                try
                {
                    for (var j = 0; j < callsPerThread; j++)
                    {
                        action(state);
                        Interlocked.Increment(ref calls);
                    }

                    taskCompletionSource.SetResult(true);
                }
                finally
                {
                    cleanup?.Invoke(state);
                }
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
