namespace Wasmtime.SourceGenerator.Models;

public record struct SemVer(
    int Major,
    int Minor,
    int Patch,
    string PreRelease,
    string BuildMetadata
) : IComparable<SemVer>
{
    public bool IsDefault => Major == 0 && Minor == 0 && Patch == 0 && string.IsNullOrEmpty(PreRelease) && string.IsNullOrEmpty(BuildMetadata);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Major);
        hashCode.Add(Minor);
        hashCode.Add(Patch);
        hashCode.Add(PreRelease);
        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public bool Equals(SemVer other)
    {
        return Major == other.Major &&
               Minor == other.Minor &&
               Patch == other.Patch &&
               PreRelease == other.PreRelease;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (string.IsNullOrEmpty(PreRelease) && string.IsNullOrEmpty(BuildMetadata))
            return $"{Major}.{Minor}.{Patch}";

        if (string.IsNullOrEmpty(BuildMetadata))
            return $"{Major}.{Minor}.{Patch}-{PreRelease}";

        if (string.IsNullOrEmpty(PreRelease))
            return $"{Major}.{Minor}.{Patch}+{BuildMetadata}";

        return $"{Major}.{Minor}.{Patch}-{PreRelease}+{BuildMetadata}";
    }

    /// <inheritdoc />
    public int CompareTo(SemVer other)
    {
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;

        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0) return minorComparison;

        var patchComparison = Patch.CompareTo(other.Patch);
        if (patchComparison != 0) return patchComparison;

        // Handle pre-release comparison
        var thisHasPreRelease = !string.IsNullOrEmpty(PreRelease);
        var otherHasPreRelease = !string.IsNullOrEmpty(other.PreRelease);

        if (thisHasPreRelease && !otherHasPreRelease) return -1; // Pre-release is lower precedence
        if (!thisHasPreRelease && otherHasPreRelease) return 1;  // No pre-release is higher precedence

        if (thisHasPreRelease && otherHasPreRelease)
        {
            var thisIdentifiers = PreRelease.Split('.');
            var otherIdentifiers = other.PreRelease.Split('.');

            for (int i = 0; i < Math.Min(thisIdentifiers.Length, otherIdentifiers.Length); i++)
            {
                var thisIdentifier = thisIdentifiers[i];
                var otherIdentifier = otherIdentifiers[i];

                var thisIsNumeric = int.TryParse(thisIdentifier, out var thisNumeric);
                var otherIsNumeric = int.TryParse(otherIdentifier, out var otherNumeric);

                if (thisIsNumeric && otherIsNumeric)
                {
                    var numericComparison = thisNumeric.CompareTo(otherNumeric);
                    if (numericComparison != 0) return numericComparison;
                }
                else if (thisIsNumeric)
                {
                    return -1; // Numeric identifiers have lower precedence than non-numeric
                }
                else if (otherIsNumeric)
                {
                    return 1; // Non-numeric identifiers have higher precedence than numeric
                }
                else
                {
                    var stringComparison = string.Compare(thisIdentifier, otherIdentifier, StringComparison.Ordinal);
                    if (stringComparison != 0) return stringComparison;
                }
            }

            return thisIdentifiers.Length.CompareTo(otherIdentifiers.Length);
        }

        // Build metadata does not affect precedence
        return 0;
    }
}
