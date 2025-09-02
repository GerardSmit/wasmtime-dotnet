using System.Collections.Immutable;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.CodeAnalysis;
using Wasmtime.SourceGenerator.Models;
using Wasmtime.SourceGenerator.Visitors;

namespace Wasmtime.SourceGenerator;

public class Wit
{
    public static WitDirectory Parse(string input)
    {
        return Parse(new WitRawDirectory(
            Path: string.Empty,
            Files: new[] { input }));
    }

    public static WitDirectory Parse(WitRawDirectory directory)
    {
        var packages = new Dictionary<WitPackageName, Dictionary<SemVer, MutableWitPackageVersion>>();

        WitPackageNameVersion? globalPackageName = null;
        var files = new List<WitParser.FileContext>();

        foreach (var content in directory.Files)
        {
            var inputStream = new AntlrInputStream(content);
            var lexer = new WitLexer(inputStream);

            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new WitParser(commonTokenStream)
            {
                ErrorHandler = new BailErrorStrategy(),
            };

            var file = parser.file();

            if (file.filePackage()?.packageName() is { } packageName)
            {
                var name = WitPackageNameVersion.Parse(packageName);

                if (globalPackageName is null)
                {
                    globalPackageName = name;
                }
                else if (!globalPackageName.Equals(name))
                {
                    // Multiple different packages in the same directory is not allowed
                    return new WitDirectory(
                        Packages: default,
                        Diagnostics: ImmutableArray.Create(
                            ReportedDiagnostic.Create(
                                DiagnosticMessages.MultipleFilePackagesInDirectory,
                                Location.None, // TODO: Get real location from 'packageName'
                                ImmutableArray.Create<object>(globalPackageName.ToString(), name.ToString(), directory.Path)
                            )
                        ));
                }
            }

            files.Add(file);
        }

        foreach (var file in files)
        {
            VisitPackage(
                packages,
                globalPackageName,
                file.fileDefinition().SelectMany(x => x.children));
        }

        // Flatten versions with the same name into a single package
        var witPackages = packages.Select(x => new WitPackage(
                x.Key,
                x.Value.ToDictionary(v => v.Key, v => v.Value.ToImmutable())
            ))
            .ToDictionary(x => x.PackageName, x => x);

        return new WitDirectory(
            witPackages,
            Diagnostics: default
        );
    }

    private static void Add(
        WitPackageNameVersion nameVersion,
        MutableWitPackageVersion package,
        Dictionary<WitPackageName, Dictionary<SemVer, MutableWitPackageVersion>> packages)
    {
        var (name, version) = nameVersion;
        var versionKey = version with { BuildMetadata = string.Empty };

        if (!packages.TryGetValue(name, out var versions))
        {
            versions = new Dictionary<SemVer, MutableWitPackageVersion>();
            packages.Add(name, versions);
        }

        if (!versions.TryGetValue(versionKey, out var existingVersion))
        {
            existingVersion = new MutableWitPackageVersion { SemVer = versionKey };
            versions.Add(versionKey, existingVersion);
        }

        existingVersion.Merge(package);
    }

    private static void VisitPackage(
        Dictionary<WitPackageName, Dictionary<SemVer, MutableWitPackageVersion>> allPackages,
        WitPackageNameVersion? name,
        IEnumerable<IParseTree> items)
    {
        var version = new MutableWitPackageVersion();

        foreach (var item in items)
        {
            if (item is WitParser.PackageContext packageContext)
            {
                VisitPackage(
                    allPackages,
                    WitPackageNameVersion.Parse(packageContext.packageName()),
                    packageContext.packageDefinition().SelectMany(x => x.children));

                continue;
            }

            if (item is WitParser.TypeDefContext typeDefContext)
            {
                var typeDef = TypeDef(name, typeDefContext);
                version.Items.Add(typeDef);
                continue;
            }

            if (item is not WitParser.WorldContext worldContext)
            {
                continue;
            }

            if (name.HasValue)
            {
                var world = World(
                    name.Value,
                    worldContext.identifier().GetTextWithoutEscape(),
                    worldContext.worldItem().Select(x => x.worldDefinition()).SelectMany(x => x.children));

                version.Worlds.Add(world.Name, world);
            }
        }

        if (name is null)
        {
            return;
        }

        Add(
            name.Value,
            version,
            allPackages
        );
    }

    private static WitWorld World(
        WitPackageNameVersion packageName,
        string worldName,
        IEnumerable<IParseTree> items)
    {
        var worldItems = new List<WitTypeDef>();

        var packagePrefix = packageName.AddLastName(worldName);

        foreach (var item in items)
        {
            if (item is WitParser.ExportContext exportContext)
            {
                var name = exportContext.identifier().GetTextWithoutEscape();
                var type = new WitTypeVisitor().Visit(exportContext.type());

                worldItems.Add(new WitWorldExport(name, type));
            }

            if (item is WitParser.IncludeContext includeContext)
            {
                if (includeContext.identifier() is { } identifier)
                {
                    var name = identifier.GetTextWithoutEscape();
                    worldItems.Add(new WitWorldInclude(packageName, name));
                }
                else if (includeContext.packageName() is { } fullOtherPackageName)
                {
                    var (name, otherPackageName) = WitPackageNameVersion.Parse(fullOtherPackageName).WithoutLastNamePart();
                    worldItems.Add(new WitWorldInclude(otherPackageName, name));
                }
            }

            if (item is WitParser.TypeDefContext typeDefContext)
            {
                worldItems.Add(TypeDef(packagePrefix, typeDefContext));
            }
        }

        return new WitWorld(
            worldName,
            worldItems.ToArray()
        );
    }

    private static WitTypeDef TypeDef(
        WitPackageNameVersion? packageName,
        WitParser.TypeDefContext context)
    {
        if (!packageName.HasValue)
        {
            throw new InvalidOperationException("Type definitions at the top level must be within a package.");
        }

        if (context.record() is { } recordContext)
        {
            return new WitRecord(
                packageName.Value,
                recordContext.identifier().GetTextWithoutEscape(),
                recordContext.recordDefinition().Select(x => new WitField(
                    x.identifier().GetTextWithoutEscape(),
                    new WitTypeVisitor().Visit(x.type())
                )).ToArray()
            );
        }

        throw new NotSupportedException($"Type definition of kind '{context.GetType().Name}' is not supported.");
    }
}
