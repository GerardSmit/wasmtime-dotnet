using System.Collections;

namespace Wasmtime.SourceGenerator;

public readonly struct EquatableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEquatable<EquatableDictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly IReadOnlyDictionary<TKey, TValue>? _dictionary;

    public EquatableDictionary(IReadOnlyDictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary?.GetEnumerator() ?? Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _dictionary?.Count ?? 0;

    public bool ContainsKey(TKey key)
    {
        return _dictionary?.ContainsKey(key) ?? false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_dictionary is not null)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        value = default!;
        return false;
    }

    public TValue this[TKey key] => _dictionary is null ? throw new KeyNotFoundException() : _dictionary[key];

    public IEnumerable<TKey> Keys => _dictionary?.Keys ?? Enumerable.Empty<TKey>();

    public IEnumerable<TValue> Values => _dictionary?.Values ?? Enumerable.Empty<TValue>();

    public bool Equals(EquatableDictionary<TKey, TValue> other)
    {
        if (Count != other.Count) return false;

        // Use ._dictionary? or Enumerable.Empty if null
        foreach (var kvp in _dictionary ?? Enumerable.Empty<KeyValuePair<TKey, TValue>>())
        {
            if (!other.TryGetValue(kvp.Key, out var otherValue)) return false;

            var value = kvp.Value;

            if (value is IEquatable<TValue> equatable)
            {
                if (!equatable.Equals(otherValue))
                {
                    return false;
                }
            }
            else if (value is null)
            {
                if (otherValue is not null)
                {
                    return false;
                }
            }
            else if (!value.Equals(otherValue))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableDictionary<TKey, TValue> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _dictionary is null ? 0 : _dictionary.GetHashCode();
    }

    public static bool operator ==(EquatableDictionary<TKey, TValue>? left, EquatableDictionary<TKey, TValue>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EquatableDictionary<TKey, TValue>? left, EquatableDictionary<TKey, TValue>? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator EquatableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary) => new(dictionary);
}