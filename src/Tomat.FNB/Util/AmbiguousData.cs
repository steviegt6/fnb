using System;
using System.Runtime.InteropServices;

namespace Tomat.FNB.Util;

/// <summary>
///     Represents a reference to an ambiguous representation of memory.
/// </summary>
public unsafe class AmbiguousData<T> where T : unmanaged {
    private readonly Memory<T>? memory;
    private T* pointer;
    private T[]? array;

    public int Length { get; }

    public T* Reference {
        get {
            if (pointer is not null)
                return pointer;

            if (array is not null) {
                fixed (T* pArray = array) {
                    return pointer = pArray;
                }
            }

            if (memory.HasValue) {
                if (MemoryMarshal.TryGetArray<T>(memory.Value, out var segment)) {
                    fixed (T* pMemory = segment.Array) {
                        return pointer = pMemory + segment.Offset;
                    }
                }

                throw new InvalidOperationException("Cannot get array from memory.");
            }

            return pointer;
        }
    }

    public AmbiguousData(T[] array) {
        this.array = array;
        memory = null;
        pointer = null;
        Length = array.Length;
    }

    public AmbiguousData(Memory<T> memory) {
        array = null;
        this.memory = memory;
        pointer = null;
        Length = memory.Length;
        ToArray();
    }

    public AmbiguousData(T* pointer, int length) {
        array = null;
        memory = null;
        this.pointer = pointer;
        Length = length;
        ToArray();
    }

    public T[] ToArray() {
        if (array is not null)
            return array;
        
        if (memory.HasValue) {
            array = memory.Value.ToArray();
            return array;
        }
        
        if (pointer is not null) {
            return array = new Span<T>(pointer, Length).ToArray();
        }
        
        throw new InvalidOperationException("No data to convert to array.");
    }
}
