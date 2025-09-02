namespace Wasmtime.SourceGenerator.Models;

public record WitPackage(
    WitPackageName PackageName,
    EquatableDictionary<SemVer, WitPackageVersion> Versions
)
{
    public SemVer LastVersion { get; } = Versions.Keys.Max();
}

public record WitPackageVersion(
    EquatableDictionary<string, WitWorld> Worlds,
    EquatableArray<WitTypeDef> Items
);

public class MutableWitPackageVersion
{
    public SemVer SemVer { get; set; }

    public Dictionary<string, WitWorld> Worlds { get; } = new();
    public List<WitTypeDef> Items { get; } = new();

    public WitPackageVersion ToImmutable() => new(Worlds, Items.ToArray());

    public void Merge(MutableWitPackageVersion other)
    {
        foreach (var kv in other.Worlds)
        {
            Worlds[kv.Key] = kv.Value;
        }

        foreach (var item in other.Items)
        {
            if (!Items.Contains(item))
            {
                Items.Add(item);
            }
        }
    }
}