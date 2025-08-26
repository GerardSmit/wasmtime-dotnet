using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wasmtime.Interop;

namespace Wasmtime;

public class WasmtimeException : Exception
{
    public WasmtimeException()
    {
    }

    public WasmtimeException(string message) : base(message)
    {
    }

    public WasmtimeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void ThrowIfError(wasmtime_error* error)
    {
        if (error != null)
        {
            Throw(error);
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe void Throw(wasmtime_error* error)
    {
        string message;

        try
        {
            using var vector = new ByteVector(error);

            message = vector.GetString();
        }
        finally
        {
            wasmtime_error_delete(error);
        }

        throw new WasmtimeException(message);
    }
}