using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Represents the results of a component call.
/// After use, the instance must be disposed. Otherwise, the function cannot be called again.
/// </summary>
/// <remarks>
/// This struct wraps around <see cref="ComponentCallResultsInternal"/> to ensure that the instance
/// is not used in async (the instance is thread-static).
/// </remarks>
public readonly unsafe ref struct ComponentCallResults : IDisposable
{
    private readonly ComponentValue* _val;
    private readonly SemaphoreSlim? _semaphore;
    private readonly ComponentCallResultsInternal? _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentCallResults"/> struct.
    /// </summary>
    /// <param name="result">The internal result structure to wrap.</param>
    /// <param name="semaphore">The semaphore to release when disposed.</param>
    internal ComponentCallResults(SemaphoreSlim? semaphore, ComponentCallResultsInternal result)
    {
        _semaphore = semaphore;
        _result = result;
        Length = result.Length;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentCallResults"/> struct.
    /// </summary>
    /// <param name="val">The pointer to the array of result values.</param>
    /// <param name="length">The number of result values.</param>
    internal ComponentCallResults(wasmtime_component_val* val, int length)
    {
        _val = (ComponentValue*)val;
        Length = length;
    }

    /// <summary>
    /// Gets the number of results returned by the component call.
    /// </summary>
    public int Length { get; }

    public ref readonly ComponentValue this[int index]
    {
        get
        {
            if (index < 0 || index >= Length) ThrowOutOfRange();

            if (_result == null) return ref _val[index];
            return ref _result.Array[index];
        }
    }

    /// <summary>
    /// Releases resources associated with this instance.
    /// </summary>
    public void Dispose()
    {
        _result?.Dispose();
        _semaphore?.Release();
    }

    private static void ThrowOutOfRange() => throw new ArgumentOutOfRangeException();
}