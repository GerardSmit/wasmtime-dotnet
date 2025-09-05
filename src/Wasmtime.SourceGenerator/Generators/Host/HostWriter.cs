using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SGF;
using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Generators.Host;

public static class HostWriter
{
    [ThreadStatic] private static System.Text.StringBuilder? _stringBuilder;
    [ThreadStatic] private static IndentedStringBuilder? _indentedStringBuilder;

    /// <summary>
    /// Generates the C# constants for WIT names.
    /// </summary>
    /// <param name="ctx">The source production context.</param>
    /// <param name="names">The constant names.</param>
    public static void GenerateConstants(SgfSourceProductionContext ctx, ImmutableArray<string> names)
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
                .Append(StringUtils.GetName(name))
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
    public static void GenerateWitAccessor(
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
            var name = StringUtils.GetName(part);

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

            var className = world.Value.CSharpName;

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
                    WriteImport(sb, name, type, imports, projectResolver);
                }

                sb.AppendLine();

                // Register method
                sb.AppendLine("unsafe void global::Wasmtime.IComponentImports.Register(global::Wasmtime.Linker linker)");
                sb.AppendLine("{");
                sb.IncrementIndent();
                foreach (var (name, _) in imports)
                {
                    var importName = StringUtils.GetName(name);
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

    private static void WriteImport(
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
                    WriteImport(sb, field.Name, field.Type, imports, projectResolver);
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
            if (item is WitWorldExport or WitUse or WitWorldImport or WitTypeAlias)
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
            else if (item is WitEnumBase @enum)
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

    private static void WriteEnum(IndentedStringBuilder sb, WitEnumBase @enum)
    {
        var name = @enum.CSharpName;
        var isFlags = @enum is WitFlags;

        if (isFlags)
        {
            sb.Append("[System.Flags]");
            sb.AppendLine();
        }

        sb.Append("public enum ").AppendLine(name);
        sb.Append("{");
        sb.IncrementIndent();

        var value = isFlags ? 1 : 0;

        for (var i = 0; i < @enum.Values.Length; i++)
        {
            var c = @enum.Values[i].CSharpName;
            sb.AppendLine(i > 0 ? "," : "");
            sb.Append(c).Append(" = ").Append(value);

            if (!isFlags)
            {
                value++;
            }
            else
            {
                value <<= 1;
            }
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
            var c = @enum.Values[i].CSharpName;
            sb.Append("case ").Append(name).Append('.').Append(c).Append(": return global::Wit.Constants.").Append(c).AppendLine(";");
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
            var c = @enum.Values[i].CSharpName;
            sb.Append(i > 0 ? "else " : "").Append("if (value.Equals(global::Wit.Constants.").Append(c).AppendLine("))");
            sb.AppendLine("{");
            sb.IncrementIndent();
            sb.Append("return ").Append(name).Append('.').Append(c).AppendLine(";");
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

        if (isFlags)
        {
            sb.AppendLine();

            // Expand(T, Span<T>) -> Int32
            sb.Append("public static int Expand(").Append(name).Append(" value, global::System.Span<").Append(name).AppendLine("> results)");
            sb.AppendLine("{");
            sb.IncrementIndent();

            sb.AppendLine("int index = 0;");
            sb.AppendLine();
            for (var i = 0; i < @enum.Values.Length; i++)
            {
                var c = @enum.Values[i].CSharpName;

                sb.Append("if ((value & ").Append(name).Append('.').Append(c).Append(") != 0) ");
                sb.Append("results[index++] = ").Append(name).Append('.').Append(c).AppendLine(";");
            }

            sb.AppendLine();
            sb.AppendLine("return index;");

            sb.DecrementIndent();
            sb.AppendLine("}");

            // Combine(ReadOnlySpan<T>) -> T
            sb.AppendLine();
            sb.Append("public static ").Append(name).Append(" Combine(global::System.ReadOnlySpan<").Append(name).AppendLine("> values)");
            sb.AppendLine("{");
            sb.IncrementIndent();
            sb.Append(name).AppendLine(" result = default;");
            sb.AppendLine();
            sb.AppendLine("foreach (var value in values)");
            sb.AppendLine("{");
            sb.IncrementIndent();
            sb.AppendLine("result |= value;");
            sb.DecrementIndent();
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("return result;");
            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        sb.DecrementIndent();
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void WriteInterface(IndentedStringBuilder sb, WitInterface interf, ITypeContainerResolver resolver)
    {
        sb.Append("public class ").AppendLine(interf.CSharpName);
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
        sb.Append("public struct ").AppendLine(record.CSharpName);
        sb.AppendLine("{");
        sb.IncrementIndent();

        foreach (var field in record.Fields)
        {
            sb.Append("public ");
            field.Type.HostWriter.WriteCSharpType(sb, resolver);
            sb.Append(' ').Append(field.CSharpName).AppendLine(";");
        }

        // ToRecordBuilder
        sb.AppendLine();
        sb.Append("public global::Wasmtime.RecordBuilder ToRecordBuilder(bool copyConstants)");
        sb.AppendLine();
        sb.AppendLine("{");
        sb.IncrementIndent();
        sb.Append("var builder = new global::Wasmtime.RecordBuilder(").Append(record.Fields.Length).AppendLine(", disposeNames: false);");
        sb.AppendLine();

        sb.AppendLine("if (copyConstants)");
        sb.AppendLine("{");
        sb.IncrementIndent();
        for (var index = 0; index < record.Fields.Length; index++)
        {
            var field = record.Fields[index];

            field.Type.HostWriter.WriteParameterInitializer(sb, field.CSharpVariableName, resolver, ignoreDispose: true, externallyOwned: true);
            sb.Append("builder.Set(").Append(index).Append(", new global::Wasmtime.ByteVector(global::Wit.Constants.").Append(field.CSharpName).Append("), ");
            field.Type.HostWriter.WriteComponentValue(sb, field.CSharpName, ignoreDispose: true, resolver, externallyOwned: true);
            sb.AppendLine(");");
        }

        sb.DecrementIndent();
        sb.AppendLine("}");
        sb.AppendLine("else");
        sb.AppendLine("{");
        sb.IncrementIndent();

        for (var index = 0; index < record.Fields.Length; index++)
        {
            var field = record.Fields[index];

            field.Type.HostWriter.WriteParameterInitializer(sb, field.CSharpVariableName, resolver, ignoreDispose: true, externallyOwned: false);
            sb.Append("builder.Set(").Append(index).Append(", global::Wit.Constants.").Append(field.CSharpName).Append(", ");
            field.Type.HostWriter.WriteComponentValue(sb, field.CSharpName, ignoreDispose: true, resolver, externallyOwned: false);
            sb.AppendLine(");");
        }

        sb.DecrementIndent();
        sb.AppendLine("}");

        sb.AppendLine();
        sb.AppendLine("return builder;");
        sb.DecrementIndent();
        sb.AppendLine("}");

        // Create
        sb.AppendLine();
        sb.Append("public static ").Append(record.CSharpName).AppendLine(" FromRecordBuilder(global::Wasmtime.RecordBuilder builder)");
        sb.AppendLine("{");
        sb.IncrementIndent();
        sb.Append(record.CSharpName).Append(" result = new ").Append(record.CSharpName).AppendLine("();");
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
            field.Type.HostWriter.WriteValueGetterInitializer(sb, "value", field.CSharpVariableName, resolver);
            sb.Append("result.").Append(field.CSharpName).Append(" = ");
            field.Type.HostWriter.WriteValueGetter(sb, "value", field.CSharpVariableName, resolver);
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
            sb.Append(' ').Append(StringUtils.GetName(name)).Append('(');

            for (var i = 0; i < funcType.Parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var param = funcType.Parameters[i];

                param.Type.HostWriter.WriteParameter(sb, param.CSharpVariableName, resolver);
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
                    param.Type.HostWriter.WriteParameterInitializer(sb, param.CSharpVariableName, resolver, ignoreDispose: false, externallyOwned: false);
                }

                if (sb.Length > length) sb.AppendLine();

                var parameterSize = funcType.Parameters.Sum(p => p.Type.HostWriter.GetParameterSize(resolver));

                sb.Append("global::Wasmtime.ComponentValue* parameters = ")
                    .Append("stackalloc global::Wasmtime.ComponentValue[")
                    .Append(parameterSize)
                    .AppendLine("];");

                for (var i = 0; i < funcType.Parameters.Length;)
                {
                    var param = funcType.Parameters[i];
                    param.Type.HostWriter.WriteParameterSetter(sb, "parameters", param.CSharpVariableName, i, ignoreDispose: false, resolver: resolver, externallyOwned: false);
                    i += param.Type.HostWriter.GetParameterSize(resolver);
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
                    funcType.Results[i].HostWriter.WriteResultGetterInitializer(sb, "result", i, resolver);
                }
            }

            if (funcType.Results.Length == 1)
            {
                sb.Append("return ");
                funcType.Results[0].HostWriter.WriteResultGetter(sb, "result", 0, resolver);
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
                    funcType.Results[i].HostWriter.WriteResultGetter(sb, "result", i, resolver);
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
            var importName = StringUtils.GetName(name);

            sb.Append("public abstract ");
            WriteParameters(sb, resolver, funcType.Results);
            sb.Append(' ').Append(importName).Append('(');

            for (var i = 0; i < funcType.Parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var param = funcType.Parameters[i];

                param.Type.HostWriter.WriteParameter(sb, param.CSharpVariableName, resolver);
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
            var importName = StringUtils.GetName(name);

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

                    param.Type.HostWriter.WriteResultGetterInitializer(sb, "args", i, resolver);
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

                    param.Type.HostWriter.WriteResultGetter(sb, "args", i, resolver);
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
                    param.HostWriter.WriteParameterInitializer(sb, variable, resolver, ignoreDispose: true, externallyOwned: true);
                }

                for (var i = 0; i < funcType.Results.Length; i++)
                {
                    var variable = GetName(funcType, i);
                    var param = funcType.Results[i];
                    param.HostWriter.WriteParameterSetter(sb, "results", variable, i, ignoreDispose: true, resolver, externallyOwned: true);
                    i += param.HostWriter.GetParameterSize(resolver);
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
            items[0].HostWriter.WriteCSharpType(sb, resolver);
        }
        else
        {
            sb.Append('(');
            for (var i = 0; i < items.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                items[i].HostWriter.WriteCSharpType(sb, resolver);
            }

            sb.Append(')');
        }
    }
}
