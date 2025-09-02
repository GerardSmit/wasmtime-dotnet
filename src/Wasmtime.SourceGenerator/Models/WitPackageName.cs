namespace Wasmtime.SourceGenerator.Models;

public readonly record struct WitPackageName(
    EquatableArray<string> Namespace,
    EquatableArray<string> Name
)
{
    public EquatableArray<string> AllParts { get; } = EquatableArray.Combine(Namespace, Name);

    public string FullName { get; } = BuildName(Namespace, Name);

    public (string, WitPackageName) WithoutLastNamePart()
    {
        if (Name.Length == 0)
        {
            return ("", this);
        }

        var lastPart = Name[Name.Length - 1];
        var packageName = new WitPackageName(
            Namespace,
            new EquatableArray<string>(Name.AsSpan().Slice(0, Name.Length - 1).ToArray())
        );

        return (lastPart, packageName);
    }

    public WitPackageName AddLastName(string name)
    {
        return new WitPackageName(Namespace, EquatableArray.Combine(Name, name));
    }

    public override string ToString()
    {
        return FullName;
    }

    private static string BuildName(EquatableArray<string> namespaces, EquatableArray<string> names)
    {
        var name = names.Length switch
        {
            1 => names[0],
            0 => string.Empty,
            _ => string.Join("/", names)
        };

        return namespaces.Length switch
        {
            0 => name,
            1 => namespaces[0] + ":" + name,
            _ => string.Join(":", namespaces) + ":" + name
        };
    }

    /// <inheritdoc />
    public bool Equals(WitPackageName other)
    {
        return FullName == other.FullName;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return FullName.GetHashCode();
    }

    public void WritePath(IndentedStringBuilder builder)
    {
        builder.Append("Wit");

        string? lastPart = null;

        foreach (var part in AllParts)
        {
            builder.Append('.');
            builder.Append(ComponentSourceGenerator.GetName(part));

            if (part == lastPart)
            {
                // It's not possible to have two nested classes with the same name, so use an underscore
                // to differentiate them.
                builder.Append('_');
            }
            else
            {
                lastPart = part;
            }
        }
    }
}

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

public readonly record struct WitPackageNameVersion(
    WitPackageName PackageName,
    SemVer Version
)
{
    public bool IsIdentifierOnly => Version.IsDefault && PackageName.AllParts.Length == 1;

    public (string, WitPackageNameVersion) WithoutLastNamePart()
    {
        var (lastPart, packageName) = PackageName.WithoutLastNamePart();
        return (lastPart, this with { PackageName = packageName });
    }

    public WitPackageNameVersion AddLastName(string name)
    {
        return this with { PackageName = PackageName.AddLastName(name) };
    }

    public override string ToString()
    {
        return Version.IsDefault
            ? PackageName.ToString()
            : $"{PackageName}@{Version}";
    }

    public static WitPackageNameVersion Parse(WitParser.PackageNameContext nameContext)
    {
        int major;
        int minor;
        int patch;
        string preRelease;
        string build;

        if (nameContext.semVersion() is not { } semver)
        {
            major = 0;
            minor = 0;
            patch = 0;
            preRelease = string.Empty;
            build = string.Empty;
        }
        else
        {
            var core = semver.semVersionCore();
            major = int.Parse(core.integer(0)?.GetText() ?? "0");
            minor = int.Parse(core.integer(1)?.GetText() ?? "0");
            patch = int.Parse(core.integer(2)?.GetText() ?? "0");
            preRelease = semver.semversionExtra().semVersionPreRelase()?.GetText() ?? string.Empty;
            build = semver.semversionExtra().semVersionBuild()?.GetText() ?? string.Empty;
        }

        var name = new WitPackageName(
            nameContext.packageNamespace()?.identifier().Select(x => x.GetText()).ToArray() ?? [],
            nameContext.identifier().Select(x => x.GetText()).ToArray()
        );

        var semVer = new SemVer(major, minor, patch, preRelease, build);

        return new WitPackageNameVersion(name, semVer);
    }
}