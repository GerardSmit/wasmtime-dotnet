using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace TestWorld
{
    // Hack to get allocation statistics from NativeMemory and GCHandle
    // since wit-gen doesn't use 'global::', we can place these classes in the 'exports' namespace
    // and they will be used instead of the ones from System.Runtime.InteropServices
    namespace exports
    {
        public static class NativeMemory
        {
            public static uint OpenAllocations;
            public static uint Allocations;
            private static readonly ConcurrentDictionary<IntPtr, UIntPtr> _allocations = new();

            public static unsafe void* Alloc(UIntPtr length)
            {
                Interlocked.Increment(ref OpenAllocations);
                Interlocked.Increment(ref Allocations);
                var ptr = System.Runtime.InteropServices.NativeMemory.Alloc(length);
                _allocations[new IntPtr(ptr)] = length;
                return ptr;
            }

            public static unsafe void* AlignedAlloc(UIntPtr length, UIntPtr alignment)
            {
                Interlocked.Increment(ref OpenAllocations);
                Interlocked.Increment(ref Allocations);
                var ptr = System.Runtime.InteropServices.NativeMemory.AlignedAlloc(length, alignment);
                _allocations[new IntPtr(ptr)] = length;
                return ptr;
            }

            public static unsafe void Free(void* ptr)
            {
                if (!_allocations.TryRemove(new IntPtr(ptr), out _))
                {
                    throw new InvalidOperationException("Cannot free memory that was not allocated");
                }

                Interlocked.Decrement(ref OpenAllocations);
                System.Runtime.InteropServices.NativeMemory.Free(ptr);
            }
        }

        public class GCHandle
        {
            public static uint OpenGcHandles;
            public static uint GcHandles;

            public static GCHandleWrapper Alloc(object value, GCHandleType type)
            {
                Interlocked.Increment(ref GcHandles);
                Interlocked.Increment(ref OpenGcHandles);
                return new GCHandleWrapper(value, type);
            }

            public class GCHandleWrapper(object value, GCHandleType type)
            {
                private System.Runtime.InteropServices.GCHandle _handle = System.Runtime.InteropServices.GCHandle.Alloc(value, type);

                public IntPtr AddrOfPinnedObject()
                {
                    return _handle.AddrOfPinnedObject();
                }

                public void Free()
                {
                    _handle.Free();
                    Interlocked.Decrement(ref OpenGcHandles);
                }
            }
        }
    }

}