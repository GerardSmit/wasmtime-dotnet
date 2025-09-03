using System.Collections.Immutable;
using System.Diagnostics;
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
        context.RegisterSourceOutput(packages, GenerateWitAccessor);
        context.RegisterSourceOutput(constants, GenerateConstants);
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

                if (item is WitEnum @enum)
                {
                    foreach (var value in @enum.Values)
                    {
                        constants.Add(value);
                    }
                }

                if (item is WitInterface interf)
                {
                    VisitConstants(interf.Definitions.Items, constants);
                }
            }
        }
    }

    /// <summary>
    /// Generates the C# constants for WIT names.
    /// </summary>
    /// <param name="ctx">The source production context.</param>
    /// <param name="names">The constant names.</param>
    private static void GenerateConstants(SgfSourceProductionContext ctx, ImmutableArray<string> names)
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
    /// Generates the C# accessor for a WIT package.
    /// </summary>
    /// <param name="ctx">The source production context.</param>
    /// <param name="packages">All WIT packages.</param>
    private static void GenerateWitAccessor(
        SgfSourceProductionContext ctx,
        ImmutableArray<KeyValuePair<WitPackageName, WitPackage>> packages)
    {
        var projectTypeResolver = new ProjectTypeContainerResolver(packages.Select(x => x.Value));

        foreach (var kv in projectTypeResolver.Packages)
        {
            try
            {
                var (name, content) = GenerateWitAccessor(kv, projectTypeResolver);

                ctx.AddSource(name, content);
            }
            catch (Exception e)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticMessages.Error,
                    Location.None,
                    kv.Key,
                    e.Message));
            }
        }
    }

    /// <summary>
    /// Generates the C# accessor for a WIT package.
    /// </summary>
    /// <param name="package">The WIT package to generate the accessor for.</param>
    public static (string Path, string Content) GenerateWitAccessor(KeyValuePair<WitPackageName, WitPackage> package, ProjectTypeContainerResolver projectResolver)
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

        nameBuilder.Append("Wit");

        foreach (var part in package.Key.AllParts)
        {
            var name = GetName(part);

            sb.AppendLine($"public static partial class {name}");
            nameBuilder.Append('.');
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

            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        var version = package.Value.Versions[package.Value.LastVersion];

        foreach (var world in version.Worlds)
        {
            var allImports = world.Value.Definitions.FindAll<WitWorldImport>(projectResolver);
            var allExports = world.Value.Definitions.FindAll<WitWorldExport>(projectResolver);

            var className = GetName(world.Value.Name);

            if (world.Value.Definitions.Items.Any(i => i is not (WitWorldExport or WitUse or WitWorldImport)))
            {
                sb.Append("public partial class ").Append(className).AppendLine();

                sb.AppendLine("{");
                sb.IncrementIndent();

                WriteItems(sb, world.Value.Definitions.Items, projectResolver);

                sb.DecrementIndent();
                sb.AppendLine("}");
                sb.AppendLine();
            }

            if (allExports.Length > 0)
            {
                sb.Append("public partial class ").Append(className).AppendLine("Exports");

                sb.AppendLine("{");
                sb.IncrementIndent();

                // Fields
                sb.AppendLine("private readonly global::Wasmtime.ComponentInstance _instance;");
                sb.AppendLine();

                // Constructor
                sb.Append("public ").Append(className).AppendLine("Exports(global::Wasmtime.ComponentInstance instance)");
                sb.AppendLine("{");
                sb.IncrementIndent();
                sb.AppendLine("_instance = instance;");
                sb.DecrementIndent();
                sb.AppendLine("}");
                sb.AppendLine();

                // Exports
                foreach (var (name, type) in allExports)
                {
                    WriteExport(sb, name, type, projectResolver);
                }

                sb.DecrementIndent();
                sb.AppendLine("}");
                sb.AppendLine();
            }

            if (allImports.Length > 0)
            {
                sb.Append("public abstract partial class ").Append(className).AppendLine("Imports : global::Wasmtime.IComponentImports");

                sb.AppendLine("{");
                sb.IncrementIndent();

                var imports = new List<(string, WitFuncType)>();

                // Imports
                foreach (var (name, type) in allImports)
                {
                    WriteExport(sb, name, type, imports, projectResolver);
                }

                sb.AppendLine();

                // Register method
                sb.AppendLine("unsafe void global::Wasmtime.IComponentImports.Register(global::Wasmtime.Linker linker)");
                sb.AppendLine("{");
                sb.IncrementIndent();
                foreach (var (name, _) in imports)
                {
                    var importName = GetName(name);
                    sb.Append("linker.DefineFunction(\"").Append(name).Append("\", ").Append("Invoke").Append(importName).AppendLine(", this);");
                }

                sb.DecrementIndent();
                sb.AppendLine("}");
                sb.AppendLine();

                // Import invokers
                foreach (var (name, type) in imports)
                {
                    var resetter = sb.CreateResetter();

                    try
                    {
                        WriteImportRegistration(sb, className, type, name, projectResolver);
                    }
                    catch (Exception e)
                    {
                        resetter.Reset();
                        sb.AppendLine($"// Failed to generate function '{name}': {e.Message}");
                    }
                }

                sb.DecrementIndent();
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        WriteItems(sb, version.Definitions.Items, projectResolver);

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

    private static void WriteExport(
        IndentedStringBuilder sb,
        string name,
        WitType type,
        List<(string, WitFuncType)> imports,
        ProjectTypeContainerResolver projectResolver)
    {
        if (type is WitCustomType customType)
        {
            type = customType.Resolve(projectResolver);
        }

        var resetter = sb.CreateResetter();

        try
        {
            if (type is WitFuncType funcType)
            {
                WriteImport(sb, funcType, name, projectResolver);
                imports.Add((name, funcType));
            }
            else if (type is WitInterfaceType interfaceType)
            {
                foreach (var field in interfaceType.Fields)
                {
                    WriteExport(sb, field.Name, field.Type, imports, projectResolver);
                }
            }
            else
            {
                sb.AppendLine($"// Unsupported export '{name}' of type '{type.Kind}'");
            }
        }
        catch (Exception e)
        {
            resetter.Reset();
            sb.AppendLine($"// Failed to generate function '{name}': {e.Message}");
        }
    }

    private static void WriteExport(IndentedStringBuilder sb, string name, WitType type, ProjectTypeContainerResolver projectResolver)
    {
        if (type is WitCustomType customType)
        {
            type = customType.Resolve(projectResolver);
        }

        var resetter = sb.CreateResetter();

        try
        {
            if (type is WitFuncType funcType)
            {
                WriteExport(sb, funcType, name, projectResolver);
            }
            else if (type is WitInterfaceType interfaceType)
            {
                foreach (var field in interfaceType.Fields)
                {
                    WriteExport(sb, field.Name, field.Type, projectResolver);
                }
            }
            else
            {
                sb.AppendLine($"// Unsupported export '{name}' of type '{type.Kind}'");
            }
        }
        catch (Exception e)
        {
            resetter.Reset();
            sb.AppendLine($"// Failed to generate function '{name}': {e.Message}");
        }
    }

    private static void WriteItems(IndentedStringBuilder sb, EquatableArray<WitTypeDef> valueItems, ITypeContainerResolver resolver)
    {
        foreach (var item in valueItems)
        {
            if (item is WitWorldExport or WitUse or WitWorldImport)
            {
                // Ignore world exports: they are handled in 'GenerateWitAccessor'.
                continue;
            }

            if (item is WitRecord record)
            {
                WriteRecord(sb, record, resolver);
                sb.AppendLine();
            }
            else if (item is WitInterface interf)
            {
                WriteInterface(sb, interf, resolver);
            }
            else if (item is WitEnum @enum)
            {
                WriteEnum(sb, @enum);
            }
            else if (item is WitWorldInclude include)
            {
                if (resolver.Resolve(include.Package) is WitPackageVersion version &&
                    version.Worlds.TryGetValue(include.WorldName, out var world))
                {
                    WriteItems(sb, world.Definitions.Items, resolver);
                }
            }
            else
            {
                sb.AppendLine($"// Unsupported item of type '{item.GetType().Name}'");
            }
        }
    }

    private static void WriteEnum(IndentedStringBuilder sb, WitEnum @enum)
    {
        var name = GetName(@enum.Name);

        sb.Append("public enum ").AppendLine(name);
        sb.Append("{");
        sb.IncrementIndent();

        for (var i = 0; i < @enum.Values.Length; i++)
        {
            sb.AppendLine(i > 0 ? "," : "");
            var c = @enum.Values[i];
            sb.Append(GetName(c)).Append(" = ").Append(i);
        }

        sb.DecrementIndent();
        sb.AppendLine();
        sb.AppendLine("}");
        sb.AppendLine();

        sb.Append("public static class ").Append(name).AppendLine("Helper");
        sb.AppendLine("{");
        sb.IncrementIndent();

        // ToByteVector
        sb.Append("public static global::Wasmtime.ByteVector ToByteVector(").Append(name).AppendLine(" value)");
        sb.AppendLine("{");
        sb.IncrementIndent();
        sb.AppendLine("switch (value)");
        sb.AppendLine("{");
        sb.IncrementIndent();
        for (var i = 0; i < @enum.Values.Length; i++)
        {
            var c = @enum.Values[i];
            sb.Append("case ").Append(name).Append('.').Append(GetName(c)).Append(": return global::Wit.Constants.").Append(GetName(c)).AppendLine(";");
        }
        sb.AppendLine("default: throw new global::System.InvalidOperationException($\"Invalid enum value: {value}\");");
        sb.DecrementIndent();
        sb.AppendLine("}");
        sb.DecrementIndent();
        sb.AppendLine("}");

        // FromByteVector
        sb.AppendLine();
        sb.Append("public static ").Append(name).AppendLine(" FromByteVector(global::Wasmtime.ByteVector value)");
        sb.AppendLine("{");
        sb.IncrementIndent();
        for (var i = 0; i < @enum.Values.Length; i++)
        {
            var c = @enum.Values[i];
            sb.Append(i > 0 ? "else " : "").Append("if (value.Equals(global::Wit.Constants.").Append(GetName(c)).AppendLine("))");
            sb.AppendLine("{");
            sb.IncrementIndent();
            sb.Append("return ").Append(name).Append('.').Append(GetName(c)).AppendLine(";");
            sb.DecrementIndent();
            sb.AppendLine("}");
        }
        sb.AppendLine("else");
        sb.AppendLine("{");
        sb.IncrementIndent();
        sb.AppendLine("throw new global::System.InvalidOperationException($\"Invalid enum value: {value}\");");
        sb.DecrementIndent();
        sb.AppendLine("}");
        sb.DecrementIndent();
        sb.AppendLine("}");

        sb.DecrementIndent();
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void WriteInterface(IndentedStringBuilder sb, WitInterface interf, ITypeContainerResolver resolver)
    {
        var name = GetName(interf.Name);

        sb.Append("public class ").AppendLine(name);
        sb.AppendLine("{");
        sb.IncrementIndent();

        if (interf.Definitions.Items.Length > 0)
        {
            WriteItems(sb, interf.Definitions.Items, resolver);
        }

        sb.DecrementIndent();
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void WriteRecord(IndentedStringBuilder sb, WitRecord record, ITypeContainerResolver resolver)
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

        // ToRecordBuilder
        sb.AppendLine();
        sb.Append("public global::Wasmtime.RecordBuilder ToRecordBuilder()");
        sb.AppendLine();
        sb.AppendLine("{");
        sb.IncrementIndent();
        sb.Append("var builder = new global::Wasmtime.RecordBuilder(").Append(record.Fields.Length).AppendLine(");");
        sb.AppendLine();
        for (var index = 0; index < record.Fields.Length; index++)
        {
            var field = record.Fields[index];
            var uniqueName = GetName(field.Name, uppercaseFirst: false);

            field.Type.WriteParameterInitializer(sb, uniqueName, resolver, ignoreDispose: true, isMemoryInitializer: false);
            sb.Append("builder.Set(").Append(index).Append(", global::Wit.Constants.").Append(field.CSharpName).Append(", ");
            field.Type.WriteComponentValue(sb, field.CSharpName, ignoreDispose: true, resolver);
            sb.AppendLine(");");
        }
        sb.AppendLine();
        sb.AppendLine("return builder;");
        sb.DecrementIndent();
        sb.AppendLine("}");

        // Create
        sb.AppendLine();
        sb.Append("public static ").Append(name).AppendLine(" FromRecordBuilder(global::Wasmtime.RecordBuilder builder)");
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
            var uniqueName = GetName(field.Name, uppercaseFirst: false);

            sb.Append("if (name.Equals(global::Wit.Constants.").Append(field.CSharpName).AppendLine("))");
            sb.AppendLine("{");
            sb.IncrementIndent();
            field.Type.WriteValueGetterInitializer(sb, "value", uniqueName, resolver);
            sb.Append("result.").Append(field.CSharpName).Append(" = ");
            field.Type.WriteValueGetter(sb, "value", uniqueName, resolver);
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

    private static void WriteExport(
        IndentedStringBuilder sb,
        WitFuncType funcType,
        string name,
        ITypeContainerResolver resolver)
    {
        sb.Append("");
        var resetter = sb.CreateResetter();

        try
        {
            sb.Append("public unsafe ");
            WriteParameters(sb, resolver, funcType.Results);
            sb.Append(' ').Append(GetName(name)).Append('(');

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
                    param.Type.WriteParameterInitializer(sb, GetName(param.Name, false), resolver, ignoreDispose: false, isMemoryInitializer: false);
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
                    param.Type.WriteParameterSetter(sb, "parameters", GetName(param.Name, false), i, ignoreDispose: false, resolver);
                    i += param.Type.GetParameterSize(resolver);
                }

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("global::Wasmtime.ComponentValue* parameters = null;");
            }

            sb.Append("using global::Wasmtime.ComponentCallResults result = _instance.Call(\"")
                .Append(name)
                .Append("\", ")
                .Append(funcType.Results.Length)
                .Append(", parameters, ")
                .Append(funcType.Parameters.Length)
                .AppendLine(");");

            if (funcType.Results.Length > 0)
            {
                for (var i = 0; i < funcType.Results.Length; i++)
                {
                    funcType.Results[i].WriteResultGetterInitializer(sb, "result", i, resolver);
                }
            }

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
            resetter.Reset();
            sb.AppendLine($"// Failed to generate function '{name}': {e.Message}");
            sb.AppendLine();
        }
    }

    private static void WriteImport(
        IndentedStringBuilder sb,
        WitFuncType funcType,
        string name,
        ITypeContainerResolver resolver)
    {
        sb.Append("");
        var resetter = sb.CreateResetter();

        try
        {
            var importName = GetName(name);

            sb.Append("public abstract ");
            WriteParameters(sb, resolver, funcType.Results);
            sb.Append(' ').Append(importName).Append('(');

            for (var i = 0; i < funcType.Parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var param = funcType.Parameters[i];

                param.Type.WriteParameter(sb, GetName(param.Name, false), resolver);
            }

            sb.AppendLine(");");
        }
        catch (Exception e)
        {
            resetter.Reset();
            sb.AppendLine($"// Failed to generate function '{name}': {e.Message}");
        }
    }

    private static void WriteImportRegistration(IndentedStringBuilder sb,
        string className,
        WitFuncType funcType,
        string name,
        ITypeContainerResolver resolver)
    {
        sb.Append("");

        var resetter = sb.CreateResetter();

        try
        {
            var importName = GetName(name);

            sb.Append("private unsafe static void Invoke").Append(importName);
            sb.AppendLine("(object state, global::Wasmtime.ComponentCallResults args, global::Wasmtime.ComponentValue* results)");
            sb.AppendLine("{");

            sb.IncrementIndent();
            sb.Append("var @this = (").Append(className).Append("Imports").AppendLine(")state;");
            sb.AppendLine();

            if (funcType.Parameters.Length > 0)
            {
                for (var i = 0; i < funcType.Parameters.Length; i++)
                {
                    var param = funcType.Parameters[i];

                    param.Type.WriteResultGetterInitializer(sb, "args", i, resolver);
                }
            }

            if (funcType.Results.Length > 0)
            {
                WriteParameters(sb, resolver, funcType.Results);
                sb.Append(" result = ");
            }

            sb.Append("@this.").Append(importName);

            if (funcType.Parameters.Length > 0)
            {
                sb.Append('(');
                sb.IncrementIndent();

                for (var i = 0; i < funcType.Parameters.Length; i++)
                {
                    sb.AppendLine(i > 0 ? "," : "");

                    var param = funcType.Parameters[i];

                    param.Type.WriteResultGetter(sb, "args", i, resolver);
                }

                sb.DecrementIndent();
                sb.AppendLine();
                sb.AppendLine(");");
            }
            else
            {
                sb.AppendLine("();");
            }

            if (funcType.Results.Length > 0)
            {
                sb.AppendLine();

                for (var i = 0; i < funcType.Results.Length; i++)
                {
                    var param = funcType.Results[i];
                    var variable = GetName(funcType, i);
                    param.WriteParameterInitializer(sb, variable, resolver, ignoreDispose: true, isMemoryInitializer: false);
                }

                for (var i = 0; i < funcType.Parameters.Length; i++)
                {
                    var variable = GetName(funcType, i);
                    var param = funcType.Parameters[i];
                    param.Type.WriteParameterSetter(sb, "results", variable, i, ignoreDispose: true, resolver);
                    i += param.Type.GetParameterSize(resolver);
                }
            }

            sb.DecrementIndent();
            sb.AppendLine("}");
            sb.AppendLine();
        }
        catch (Exception e)
        {
            resetter.Reset();
            sb.AppendLine($"// Failed to generate function '{name}': {e.Message}");
            sb.AppendLine();

            if (e is not NotSupportedException)
            {
            }
        }
    }

    private static string GetName(WitFuncType funcType, int i)
    {
        var variable = "result";
        if (funcType.Results.Length > 1)
        {
            variable += '.' + (i switch
            {
                0 => "Item1",
                1 => "Item2",
                2 => "Item3",
                3 => "Item4",
                4 => "Item5",
                5 => "Item6",
                6 => "Item7",
                _ => throw new InvalidOperationException("Too many return values.")
            });
        }

        return variable;
    }

    private static void WriteParameters(IndentedStringBuilder sb, ITypeContainerResolver resolver, EquatableArray<WitType> items)
    {
        if (items.Length == 0)
        {
            sb.Append("void");
        }
        else if (items.Length == 1)
        {
            items[0].WriteCSharpType(sb, resolver);
        }
        else
        {
            sb.Append('(');
            for (var i = 0; i < items.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                items[i].WriteCSharpType(sb, resolver);
            }

            sb.Append(')');
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
