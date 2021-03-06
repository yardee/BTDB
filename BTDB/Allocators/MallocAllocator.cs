using System;
using Mono.Unix;

namespace BTDB.Allocators
{
    public class MallocAllocator : IOffHeapAllocator
    {
        public IntPtr Allocate(IntPtr size)
        {
            return UnixMarshal.AllocHeap(size.ToInt64());
        }

        public void Deallocate(IntPtr ptr)
        {
            UnixMarshal.FreeHeap(ptr);
        }
    }
}