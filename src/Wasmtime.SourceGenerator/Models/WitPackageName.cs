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
            builder.Append(StringUtils.GetName(part));

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