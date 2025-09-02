using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using SGF;
using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator;

[IncrementalGenerator]
public class ComponentSourceGenerator() : IncrementalGenerator("ComponentSourceGenerator")
{
    [ThreadStatic] private static System.Text.StringBuilder? _stringBuilder;
    [ThreadStatic] private static IndentedStringBuilder? _indentedStringBuilder;

    private static readonly Regex DashRegex = new("-(.)?", RegexOptions.Compiled);

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

        // Generate the source
        var packages = witFiles
            .SelectMany((x, _) => x.Packages);

        context.RegisterSourceOutput(packages, GenerateWitAccessor);

        var constants = witFiles
            .SelectMany((x, _) => x.Packages.Values)
            .SelectMany((x, _) =>
            {
                var constants = new HashSet<string>();

                foreach (var version in x.Versions)
                {
                    var items = version.Value.Items;

                    VisitConstants(items, constants);

                    foreach (var world in version.Value.Worlds.Values)
                    {
                        VisitConstants(world.Items, constants);
                    }
                }

                var array = constants.ToArray();
                Array.Sort(array, StringComparer.Ordinal);
                return Unsafe.As<string[], ImmutableArray<string>>(ref array);
            })
            .Collect();

        context.RegisterSourceOutput(constants, GenerateConstants);
    }

    private static void VisitConstants(EquatableArray<WitTypeDef> items, HashSet<string> constants)
    {
        foreach (var item in items)
        {
            if (item is not WitRecord record) continue;

            constants.Add(record.Name);

            foreach (var field in record.Fields)
            {
                constants.Add(field.Name);
            }
        }
    }

    private void GenerateConstants(SgfSourceProductionContext ctx, ImmutableArray<string> names)
    {
        var sb = _indentedStringBuilder ??= new IndentedStringBuilder();

        sb.Clear();

        sb.AppendLine("internal static partial class Wit");
        sb.AppendLine("{");
        sb.IncrementIndent();

        sb.AppendLine("public static partial class Constants");
        sb.AppendLine("{");
        sb.IncrementIndent();

        foreach (var name in names)
        {
            sb.Append("public static readonly global::Wasmtime.ByteVector ")
                .Append(GetName(name))
                .Append(" = new global::Wasmtime.ByteVector(\"")
                .Append(name)
                .AppendLine("\");");
        }

        sb.DecrementIndent();
        sb.AppendLine("}");

        sb.DecrementIndent();
        sb.AppendLine("}");

        ctx.AddSource("Wit.Constants.g.cs", sb.ToString());

        sb.Clear();
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
                dictionary[path] = includes;
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
    /// <param name="file">The raw WIT file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The parsed <see cref="WitDirectory"/> or <see cref="WitDirectory.Empty"/> if parsing failed.</returns>
    private static WitDirectory ParseWitDirectory(WitRawDirectory file, CancellationToken ct)
    {
        try
        {
            return Wit.Parse(file);
        }
        catch
        {
            return WitDirectory.Empty;
        }
    }

    /// <summary>
    /// Generates the C# accessor for a WIT package.
    /// </summary>
    /// <param name="ctx">The source production context.</param>
    /// <param name="package">The WIT package to generate the accessor for.</param>
    private static void GenerateWitAccessor(
        SgfSourceProductionContext ctx,
        KeyValuePair<WitPackageName, WitPackage> package)
    {
        var (name, content) = GenerateWitAccessor(package);

        ctx.AddSource(name, content);
    }

    /// <summary>
    /// Generates the C# accessor for a WIT package.
    /// </summary>
    /// <param name="package">The WIT package to generate the accessor for.</param>
    public static (string Path, string Content) GenerateWitAccessor(KeyValuePair<WitPackageName, WitPackage> package)
    {
        var nameBuilder = _stringBuilder ??= new System.Text.StringBuilder(256);
        var sb = _indentedStringBuilder ??= new IndentedStringBuilder();

        // Ensure the builders are empty
        nameBuilder.Clear();
        sb.Clear();

        sb.AppendLine("internal static partial class Wit");
        sb.AppendLine("{");

        sb.IncrementIndent();

        string? lastPart = null;

        nameBuilder.Append("Wit.");

        foreach (var part in package.Key.AllParts)
        {
            var name = GetName(part);

            sb.AppendLine($"public static partial class {name}");
            nameBuilder.Append(name);

            if (part == lastPart)
            {
                // It's not possible to have two nested classes with the same name, so use an underscore
                // to differentiate them.
                sb.Append('_');
                nameBuilder.Append('_');
            }
            else
            {
                lastPart = part;
            }

            nameBuilder.Append('.');
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        var version = package.Value.Versions[package.Value.LastVersion];

        foreach (var world in version.Worlds)
        {
            var resolver = new WorldTypeResolver(version, world.Value);
            var className = GetName(world.Value.Name);

            if (world.Value.Name == lastPart)
            {
                className += "_";
            }

            nameBuilder.Append(className);
            sb.Append("public partial class ").AppendLine(className);

            sb.AppendLine("{");
            sb.IncrementIndent();

            // Fields
            sb.AppendLine("private readonly global::Wasmtime.ComponentInstance _instance;");
            sb.AppendLine();

            // Constructor
            sb.Append("public ").Append(className).AppendLine("(global::Wasmtime.ComponentInstance instance)");
            sb.AppendLine("{");
            sb.IncrementIndent();
            sb.AppendLine("_instance = instance;");
            sb.DecrementIndent();
            sb.AppendLine("}");
            sb.AppendLine();

            // Exports
            foreach (var export in world.Value.Items.OfType<WitWorldExport>())
            {
                if (export.Type.Kind != WitTypeKind.Func || export.Type is not WitFuncType funcType)
                {
                    sb.AppendLine($"// Unsupported export '{export.ExportName}' of type '{export.Type.Kind}'");
                    continue;
                }

                try
                {
                    WriteFunction(sb, funcType, export, resolver);
                }
                catch (Exception e)
                {
                    sb.AppendLine($"// Failed to generate function '{export.ExportName}': {e.Message}");
                }
            }

            if (world.Value.Items.Length > 0)
            {
                WriteItems(sb, world.Value.Items, resolver);
            }

            sb.DecrementIndent();
            sb.AppendLine("}");
            sb.AppendLine();
        }

        foreach (var unused in package.Key.AllParts)
        {
            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        sb.DecrementIndent();
        sb.AppendLine("}");

        nameBuilder.Append(".g.cs");

        var result = (Path: nameBuilder.ToString(), Content: sb.ToString());

        // Clean up
        nameBuilder.Clear();
        sb.Clear();

        return result;
    }

    private static void WriteItems(IndentedStringBuilder sb, EquatableArray<WitTypeDef> valueItems, WorldTypeResolver resolver)
    {
        foreach (var item in valueItems)
        {
            if (item is WitWorldExport)
            {
                // Ignore world exports: they are handled in 'GenerateWitAccessor'.
                continue;
            }

            if (item is WitRecord record)
            {
                WriteRecord(sb, record, resolver);
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"// Unsupported item of type '{item.GetType().Name}'");
            }
        }
    }

    private static void WriteRecord(IndentedStringBuilder sb, WitRecord record, WorldTypeResolver resolver)
    {
        var name = GetName(record.Name);

        sb.Append("public struct ").AppendLine(name);
        sb.AppendLine("{");
        sb.IncrementIndent();

        foreach (var field in record.Fields)
        {
            sb.Append("public ");
            field.Type.WriteCSharpType(sb, resolver);
            sb.Append(' ').Append(GetName(field.Name)).AppendLine(";");
        }

        sb.AppendLine();
        sb.Append("public static ").Append(name).AppendLine(" Create(global::Wasmtime.RecordBuilder builder)");
        sb.AppendLine("{");
        sb.IncrementIndent();
        sb.Append(name).Append(" result = new ").Append(name).AppendLine("();");
        sb.AppendLine();
        sb.AppendLine("foreach (var (name, value) in builder)");
        sb.AppendLine("{");
        sb.IncrementIndent();

        for (var index = 0; index < record.Fields.Length; index++)
        {
            if (index > 0) sb.AppendLine();

            var field = record.Fields[index];
            sb.Append("if (name.Equals(global::Wit.Constants.").Append(field.CSharpName).AppendLine("))");
            sb.AppendLine("{");
            sb.IncrementIndent();
            sb.Append("result.").Append(field.CSharpName).Append(" = ");
            field.Type.WriteValueGetter(sb, "value", resolver);
            sb.AppendLine(";");
            sb.AppendLine("continue;");
            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        sb.DecrementIndent();
        sb.AppendLine("}");

        sb.AppendLine();
        sb.AppendLine("return result;");

        sb.DecrementIndent();
        sb.AppendLine("}");

        sb.DecrementIndent();
        sb.AppendLine("}");
    }

    private static void WriteFunction(
        IndentedStringBuilder sb,
        WitFuncType funcType,
        WitWorldExport export,
        WorldTypeResolver resolver)
    {
        sb.Append("");
        var position = sb.Length;
        var indent = sb.IndentCount;

        try
        {
            sb.Append("public unsafe ");

            if (funcType.Results.Length == 0)
            {
                sb.Append("void");
            }
            else if (funcType.Results.Length == 1)
            {
                funcType.Results[0].WriteCSharpType(sb, resolver);
            }
            else
            {
                sb.Append('(');
                for (var i = 0; i < funcType.Results.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    funcType.Results[i].WriteCSharpType(sb, resolver);
                }

                sb.Append(')');
            }

            sb.Append(' ').Append(GetName(export.ExportName)).Append('(');

            for (var i = 0; i < funcType.Parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var param = funcType.Parameters[i];

                param.Type.WriteParameter(sb, GetName(param.Name, false), resolver);
            }

            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.IncrementIndent();

            if (funcType.Parameters.Length > 0)
            {
                var length = sb.Length;

                for (var index = 0; index < funcType.Parameters.Length; index++)
                {
                    var param = funcType.Parameters[index];
                    param.Type.WriteParameterInitializer(sb, GetName(param.Name, false), resolver, isMemoryInitializer: false);
                }

                if (sb.Length > length) sb.AppendLine();

                var parameterSize = funcType.Parameters.Sum(p => p.Type.GetParameterSize(resolver));

                sb.Append("global::Wasmtime.ComponentValue* parameters = ")
                    .Append("stackalloc global::Wasmtime.ComponentValue[")
                    .Append(parameterSize)
                    .AppendLine("];");

                for (var i = 0; i < funcType.Parameters.Length;)
                {
                    var param = funcType.Parameters[i];
                    param.Type.WriteParameterSetter(sb, "parameters", GetName(param.Name, false), i, resolver);
                    i += param.Type.GetParameterSize(resolver);
                }

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("global::Wasmtime.ComponentValue* parameters = null;");
            }

            sb.Append("using global::Wasmtime.ComponentCallResults result = _instance.Call(\"")
                .Append(export.ExportName)
                .Append("\", ")
                .Append(funcType.Results.Length)
                .Append(", parameters, ")
                .Append(funcType.Parameters.Length)
                .AppendLine(");");

            if (funcType.Results.Length == 1)
            {
                sb.Append("return ");
                funcType.Results[0].WriteResultGetter(sb, "result", 0, resolver);
                sb.AppendLine(";");
            }
            else if (funcType.Results.Length > 1)
            {
                sb.AppendLine();
                sb.AppendLine("return (");
                sb.IncrementIndent();
                for (var i = 0; i < funcType.Results.Length; i++)
                {
                    if (i > 0) sb.AppendLine(",");
                    funcType.Results[i].WriteResultGetter(sb, "result", i, resolver);
                }

                sb.AppendLine();
                sb.DecrementIndent();
                sb.AppendLine(");");
            }

            sb.DecrementIndent();
            sb.AppendLine("}");
            sb.AppendLine();
        }
        catch (Exception e)
        {
            sb.Length = position;
            sb.IndentCount = indent;
            sb.AppendLine($"// Failed to generate function '{export.ExportName}': {e.Message}");
            sb.AppendLine();
        }
    }

    /// <summary>
    /// Change the name to a valid C# name.
    /// </summary>
    /// <example>
    /// 'foo-bar' becomes 'FooBar'
    /// </example>
    /// <param name="name">WIT name</param>
    /// <param name="uppercaseFirst">If <c>true</c>, the first character will be uppercased.</param>
    /// <returns>C# name</returns>
    public static string GetName(string name, bool uppercaseFirst = true)
    {
        var result = DashRegex.Replace(name, static m =>
        {
            var g = m.Groups[1];
            return g.Success ? g.Value.ToUpperInvariant() : string.Empty;
        });

        if (string.IsNullOrEmpty(result))
        {
            return "_";
        }

        if (!uppercaseFirst)
        {
            return result;
        }

        return char.ToUpperInvariant(result[0]) + result.Substring(1);
    }
}
