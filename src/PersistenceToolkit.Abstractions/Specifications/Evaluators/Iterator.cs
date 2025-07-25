﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace PersistenceToolkit.Abstractions.Specifications;

internal abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
{
    private readonly int _threadId = Environment.CurrentManagedThreadId;

    private protected int _state;
    private protected TSource _current = default!;

    public Iterator<TSource> GetEnumerator()
    {
        var enumerator = _state == 0 && _threadId == Environment.CurrentManagedThreadId ? this : Clone();
        enumerator._state = 1;
        return enumerator;
    }

    public abstract Iterator<TSource> Clone();
    public abstract bool MoveNext();

    public TSource Current => _current;
    IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator() => GetEnumerator();

    [ExcludeFromCodeCoverage]
    object? IEnumerator.Current => Current;

    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [ExcludeFromCodeCoverage]
    void IEnumerator.Reset() => throw new NotSupportedException();

    public virtual void Dispose()
    {
        _current = default!;
        _state = -1;
    }
}
