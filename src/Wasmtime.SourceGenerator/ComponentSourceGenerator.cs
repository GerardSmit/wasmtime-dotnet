using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using SGF;
using Wasmtime.SourceGenerator.Generators.Host;
using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator;

[IncrementalGenerator]
public class ComponentSourceGenerator() : IncrementalGenerator("ComponentSourceGenerator")
{
    /// <inheritdoc/>
    public override void OnInitialize(SgfInitializationContext context)
    {
        // WIT files
        var rawWitFiles = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith(".wit", StringComparison.OrdinalIgnoreCase))
            .Select((text, cancellationToken) => (path: text.Path, content: text.GetText(cancellationToken)?.ToString() ?? ""))
            .Collect()
            .SelectMany(GetRawDirectories);

        var witFiles = rawWitFiles
            .Select(ParseWitDirectory);

        var packages = witFiles
            .SelectMany((x, _) => x.Packages)
            .Collect();

        var constants = packages
            .Select(GetAllConstants);

        // Generate the source
        context.RegisterSourceOutput(packages, HostWriter.GenerateWitAccessor);
        context.RegisterSourceOutput(constants, HostWriter.GenerateConstants);
    }

    /// <summary>
    /// Groups all files by their directory.
    /// </summary>
    /// <param name="array">Array of file paths and contents.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Enumerable of <see cref="WitRawDirectory"/>.</returns>
    private static IEnumerable<WitRawDirectory> GetRawDirectories(ImmutableArray<(string path, string content)> array, CancellationToken ct)
    {
        var dictionary = new Dictionary<string, ImmutableArray<string>.Builder>(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, content) in array)
        {
            var directory = Path.GetDirectoryName(path) ?? string.Empty;

            if (!dictionary.TryGetValue(directory, out var includes))
            {
                includes = ImmutableArray.CreateBuilder<string>();
                dictionary[directory] = includes;
            }

            includes.Add(content);
        }

        var results = ImmutableArray.CreateBuilder<WitRawDirectory>(dictionary.Count);

        foreach (var kv in dictionary)
        {
            results.Add(new WitRawDirectory(kv.Key, kv.Value.ToImmutable()));
        }

        return results;
    }

    /// <summary>
    /// Parses a WIT file and returns a <see cref="WitDirectory"/> instance.
    /// </summary>
    /// <param name="directory">The raw WIT file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The parsed <see cref="WitDirectory"/> or <see cref="WitDirectory.Empty"/> if parsing failed.</returns>
    private static WitDirectory ParseWitDirectory(WitRawDirectory directory, CancellationToken ct)
    {
        try
        {
            return Wit.Parse(directory);
        }
        catch
        {
            return WitDirectory.Empty;
        }
    }

    /// <summary>
    /// Gets all constant names from the WIT packages.
    /// </summary>
    /// <param name="packages">The WIT packages.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of constant names.</returns>
    private static ImmutableArray<string> GetAllConstants(ImmutableArray<KeyValuePair<WitPackageName, WitPackage>> packages, CancellationToken ct)
    {
        var constants = new HashSet<string>();

        foreach (var kv in packages)
        {
            var package = kv.Value;

            foreach (var version in package.Versions)
            {
                var items = version.Value.Definitions.Items;

                VisitConstants(items, constants);

                foreach (var world in version.Value.Worlds.Values)
                {
                    VisitConstants(world.Definitions.Items, constants);
                }
            }
        }

        var array = constants.ToArray();
        Array.Sort(array, StringComparer.Ordinal);
        return Unsafe.As<string[], ImmutableArray<string>>(ref array);

        static void VisitConstants(EquatableArray<WitTypeDef> items, HashSet<string> constants)
        {
            foreach (var item in items)
            {
                if (item is WitRecord record)
                {
                    constants.Add(record.Name);

                    foreach (var field in record.Fields)
                    {
                        constants.Add(field.Name);
                    }
                }

                if (item is WitEnumBase @enum)
                {
                    foreach (var value in @enum.Values)
                    {
                        constants.Add(value.Name);
                    }
                }

                if (item is WitInterface interf)
                {
                    VisitConstants(interf.Definitions.Items, constants);
                }
            }
        }
    }
}
