using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Wasmtime.SourceGenerator.Models;
using Wasmtime.SourceGenerator.Visitors;

namespace Wasmtime.SourceGenerator;

public class Wit
{
    public static WitFile Parse(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new WitLexer(inputStream);

        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new WitParser(commonTokenStream)
        {
            ErrorHandler = new BailErrorStrategy(),
        };

        var file = parser.file();
        var packages = new Dictionary<WitPackageName, Dictionary<string, WitPackageVersion>>();

        if (file.filePackage() is {} filePackage)
        {
            var result = Package(filePackage.packageName(), file.fileDefinition().SelectMany(x => x.children));

            Add(result, packages);
        }
        else
        {
            foreach (var item in file.fileDefinition().SelectMany(x => x.children))
            {
                if (item is not WitParser.PackageContext packageContext)
                {
                    continue;
                }

                var result = Package(
                    packageContext.packageName(),
                    packageContext.packageDefinition().SelectMany(x => x.children));

                Add(result, packages);
            }
        }

        // Flatten versions with the same name into a single package
        var witPackages = packages.Select(x => new WitPackage(
                x.Key,
                new EquatableDictionary<string, WitPackageVersion>(x.Value)
            ))
            .ToDictionary(x => x.PackageName, x => x);

        return new WitFile(witPackages);
    }

    private static void Add(
        (WitPackageName Name, WitPackageVersion Version) result,
        Dictionary<WitPackageName, Dictionary<string, WitPackageVersion>> packages)
    {
        if (!packages.TryGetValue(result.Name, out var versions))
        {
            versions = new Dictionary<string, WitPackageVersion>();
            packages.Add(result.Name, versions);
        }

        if (versions.TryGetValue(result.Version.Version, out var existingVersion))
        {
            if (result.Version.Equals(existingVersion))
            {
                // Same version, same contents, ignore
                return;
            }

            throw new InvalidOperationException($"Duplicate package version: {result.Name} {result.Version.Version}");
        }

        versions.Add(result.Version.Version, result.Version);
    }

    private static (WitPackageName Name, WitPackageVersion Version) Package(WitParser.PackageNameContext nameContext, IEnumerable<IParseTree> items)
    {
        var packages = new Dictionary<string, WitWorld>();

        foreach (var item in items)
        {
            if (item is not WitParser.WorldContext worldContext)
            {
                continue;
            }

            var world = World(
                worldContext.identifier().GetTextWithoutEscape(),
                worldContext.worldDefinition().SelectMany(x => x.children));

            packages.Add(world.Name, world);
        }

        string version;

        if (nameContext.semVersion() is not { } semver)
        {
            version = "0.0.0";
        }
        else
        {
            var core = semver.semVersionCore();
            var major = core.integer(0).GetText().TrimStart('0');
            var minor = core.integer(1)?.GetText().TrimStart('0') ?? "0";
            var patch = core.integer(2)?.GetText().TrimStart('0') ?? "0";

            version = $"{major}.{minor}.{patch}{semver.semversionExtra()?.GetText()}";
        }

        var packageVersion = new WitPackageVersion(version, packages);

        var name = new WitPackageName(
            nameContext.packageNamespace()?.identifier().Select(x => x.GetText()).ToArray() ?? Array.Empty<string>(),
            nameContext.identifier().Select(x => x.GetText()).ToArray()
        );

        return (name, packageVersion);
    }

    private static WitWorld World(string name, IEnumerable<IParseTree> items)
    {
        var exports = new Dictionary<string, WitType>();

        foreach (var item in items)
        {
            if (item is not WitParser.ExportContext exportContext)
            {
                continue;
            }

            var type = new WitTypeVisitor().Visit(exportContext.type());

            exports.Add(
                exportContext.identifier().GetTextWithoutEscape(),
                type
            );
        }

        return new WitWorld(
            name,
            exports
        );
    }
}
