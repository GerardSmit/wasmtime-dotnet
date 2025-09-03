using System.Collections.Immutable;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.CodeAnalysis;
using Wasmtime.SourceGenerator.Models;
using Wasmtime.SourceGenerator.Visitors;
using System.Runtime.CompilerServices;

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

            WitParser.FileContext? file;

            try
            {
                file = parser.file();
            }
            catch
            {
                throw;
            }

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

        Add(name.Value, version, allPackages);
    }

    private static WitWorld World(
        WitPackageNameVersion packageName,
        string worldName,
        IEnumerable<IParseTree> items)
    {
        var worldItems = new List<WitTypeDef>();
        var packagePrefix = packageName.AddLastName(worldName);
        var typeVisitor = new WitTypeVisitor(packagePrefix);

        foreach (var item in items)
        {
            if (item is WitParser.ExportContext exportContext)
            {
                var (name, type) = ImportExport(packageName, exportContext.importExport(), typeVisitor);
                worldItems.Add(new WitWorldExport(name, type));
            }

            if (item is WitParser.Import_Context importContext)
            {
                var (name, type) = ImportExport(packageName, importContext.importExport(), typeVisitor);
                worldItems.Add(new WitWorldImport(name, type));
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
            new WitTypeDefinitions(worldItems.ToArray())
        );
    }

    private static (string name, WitType type) ImportExport(WitPackageNameVersion packageName,
        WitParser.ImportExportContext importExport, WitTypeVisitor typeVisitor)
    {
        string name;
        WitType type;


        if (importExport.externType() is {} externType)
        {
            name = importExport.identifier().GetTextWithoutEscape();
            type = typeVisitor.Visit(externType);
        }
        else
        {
            var otherPackage = WitPackageNameVersion.Parse(importExport.packageName());

            if (otherPackage.IsIdentifierOnly)
            {
                name = otherPackage.PackageName.AllParts[0];
                type = new WitCustomType(packageName, name);
            }
            else
            {
                (name, otherPackage) = otherPackage.WithoutLastNamePart();
                type = new WitCustomType(otherPackage, name);
            }
        }

        return (name, type);
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
                    new WitTypeVisitor(packageName.Value).Visit(x.type())
                )).ToArray()
            );
        }

        if (context.@interface() is { } interfaceContext)
        {
            var name = interfaceContext.identifier().GetTextWithoutEscape();
            var path = packageName.Value.AddLastName(name);

            return new WitInterface(
                name,
                new WitTypeDefinitions(interfaceContext.interfaceDefinition()
                    .Where(x => x.typeDef() is not null)
                    .Select(x => TypeDef(path, x.typeDef()))
                    .ToArray()),
                interfaceContext.interfaceDefinition()
                    .Where(x => x.identifier() is not null)
                    .Select(x => new WitField(
                        x.identifier().GetTextWithoutEscape(),
                        new WitTypeVisitor(path).Visit(x.type())
                    ))
                    .ToArray()
            );
        }

        if (context.use() is { } useContext)
        {
            var items = new WitUseItem[useContext.useItem().Length];

            for (var i = 0; i < useContext.useItem().Length; i++)
            {
                var useItem = useContext.useItem(i);
                var name = useItem.identifier(0).GetTextWithoutEscape();

                items[i] = new WitUseItem(
                    name,
                    useItem.identifier().Length > 1 ? useItem.identifier(1).GetTextWithoutEscape() : name
                );
            }

            var container = WitPackageNameVersion.Parse(useContext.packageName());

            if (container.IsIdentifierOnly)
            {
                return new WitUse(
                    packageName.Value,
                    container.PackageName.AllParts[0],
                    items
                );
            }

            var (otherName, otherPackage) = container.WithoutLastNamePart();

            return new WitUse(
                otherPackage,
                otherName,
                items
            );
        }

        if (context.resource() is {} resourceContext)
        {
            var name = resourceContext.identifier().GetTextWithoutEscape();
            var typeVisitor = new WitTypeVisitor(packageName.Value);

            var constructors = resourceContext.resourceMethod()
                .OfType<WitParser.ResourceConstructorContext>()
                .Select(x => new WitResourceConstructor(
                    x.funcParamList().funcParam()
                        .Select(y => new WitFuncParameter(y.identifier().GetTextWithoutEscape(), typeVisitor.Visit(y.type())))
                        .ToArray()
                ))
                .ToArray();

            var methods = resourceContext.resourceMethod()
                .OfType<WitParser.ResourceFunctionContext>()
                .Select(x => new WitField(
                    x.identifier().GetTextWithoutEscape(),
                    typeVisitor.Visit(x.type())
                ))
                .ToArray();

            return new WitResource(name, constructors, methods);
        }

        if (context.typeAlias() is { } typeAliasContext)
        {
            return new WitTypeAlias(
                typeAliasContext.identifier().GetTextWithoutEscape(),
                new WitTypeVisitor(packageName.Value).Visit(typeAliasContext.type())
            );
        }

        if (context.@enum() is { } enumContext)
        {
            return new WitEnum(
                packageName.Value,
                enumContext.identifier().GetTextWithoutEscape(),
                enumContext.enumItem().Select(x => x.identifier().GetTextWithoutEscape()).ToArray()
            );
        }

        if (context.flags() is { } flagsContext)
        {
            return new WitFlags(
                flagsContext.identifier().GetTextWithoutEscape(),
                flagsContext.flagsItem().Select(x => x.identifier().GetTextWithoutEscape()).ToArray()
            );
        }

        if (context.variant() is { } variantContext)
        {
            var name = variantContext.identifier().GetTextWithoutEscape();
            var typeVisitor = new WitTypeVisitor(packageName.Value);

            var cases = variantContext.variantDefinition()
                .Select(x => new WitVariantCase(
                    x.identifier().GetTextWithoutEscape(),
                    x.type() is null ? null : typeVisitor.Visit(x.type())
                ))
                .ToArray();

            return new WitVariant(packageName.Value, name, cases);
        }

        throw new NotSupportedException($"Type definition of kind '{context.children.First().GetType().Name}' is not supported.");
    }
}
