using System.Diagnostics.CodeAnalysis;

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
    WitTypeDefinitions Definitions
) : ITypeContainer
{
    public bool TryGetType(string name, [NotNullWhen(true)] out WitType? type)
    {
        return Definitions.TryGetType(name, out type);
    }

    public bool TryGetContainer(string name, [NotNullWhen(true)] out ITypeContainer? container)
    {
        if (Worlds.TryGetValue(name, out var world))
        {
            container = world;
            return true;
        }

        return Definitions.TryGetContainer(name, out container);
    }

    public WitPackageVersion Merge(WitPackageVersion result)
    {
        var worlds = new Dictionary<string, WitWorld>(Worlds.Count + result.Worlds.Count);

        foreach (var kv in Worlds)
        {
            worlds.Add(kv.Key, kv.Value);
        }

        foreach (var kv in result.Worlds)
        {
            worlds.Add(kv.Key, kv.Value);
        }

        var definitions = EquatableArray.Combine(Definitions.Items, result.Definitions.Items);

        return new WitPackageVersion(
            worlds,
            new WitTypeDefinitions(definitions));
    }
}

public class MutableWitPackageVersion
{
    public SemVer SemVer { get; set; }

    public Dictionary<string, WitWorld> Worlds { get; } = new();
    public List<WitTypeDef> Items { get; } = new();

    public WitPackageVersion ToImmutable() => new(Worlds, new WitTypeDefinitions(Items.ToArray()));

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