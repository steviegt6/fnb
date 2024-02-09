using System;

namespace Tomat.FNB.Util;

/// <summary>
///     Represents a reference to an ambiguous, immutable block of memory. This
///     may be an array, a <see cref="Memory{T}"/>, or a pointer.
///     <br />
///     It is handleable as a pointer or span.
/// </summary>
public sealed unsafe class AmbiguousData<T> where T :  unmanaged {
    private readonly Memory<T>? memory;
    private T[]? array;
    private T* pointer;

    public T* Pointer {
        get {
            if (pointer != null)
                return pointer;

            if (array != null) {
                fixed (T* p = array) {
                    return pointer = p;
                }
            }

            if (memory.HasValue) {
                fixed (T* p = memory.Value.Span)
                    return pointer = p;
            }

            throw new InvalidOperationException("No pointer available.");
        }
    }

    public Span<T> Span {
        get {
            if (array != null)
                return new Span<T>(array);

            if (memory.HasValue)
                return memory.Value.Span;

            return new Span<T>(pointer, Length);
        }
    }

    // TODO: I don't like this.
    public T[] Array => array ??= memory.HasValue ? memory.Value.ToArray() : Span.ToArray();

    public int Length { get; }

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
    }

    public AmbiguousData(T* pointer, int length) {
        array = null;
        memory = null;
        this.pointer = pointer;
        Length = length;
    }
}
