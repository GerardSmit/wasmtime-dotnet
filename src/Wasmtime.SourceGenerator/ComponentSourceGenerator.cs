using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using SGF;
using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator;

[IncrementalGenerator]
public class ComponentSourceGenerator : IncrementalGenerator
{
    public ComponentSourceGenerator() : base("ComponentSourceGenerator")
    {
    }

    public override void OnInitialize(SgfInitializationContext context)
    {
        // WIT files
        var witFiles = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith(".wit", StringComparison.OrdinalIgnoreCase))
            .Select((text, cancellationToken) => (path: text.Path, content: text.GetText(cancellationToken)?.ToString() ?? ""))
            .Select((file, _) =>
            {
                try
                {
                    return Wit.Parse(file.content);
                }
                catch
                {
                    return WitFile.Empty;
                }
            })
            .Collect();

        // Generate the source
        context.RegisterSourceOutput(witFiles, static (ctx, witFiles) =>
        {
            var sb = new IndentedStringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using Wasmtime;");
            sb.AppendLine();

            sb.AppendLine("internal static partial class Wit");
            sb.AppendLine("{");

            sb.IncrementIndent();
            foreach (var file in witFiles)
            {
                foreach (var package in file.Packages)
                {
                    string? lastPart = null;

                    foreach (var part in package.Key.AllParts)
                    {
                        sb.AppendLine($"public static partial class {GetName(part)}");

                        if (part == lastPart)
                        {
                            // It's not possible to have two nested classes with the same name, so use an underscore
                            // to differentiate them.
                            sb.Append('_');
                        }
                        else
                        {
                            lastPart = part;
                        }

                        sb.AppendLine("{");
                        sb.IncrementIndent();
                    }

                    var version = package.Value.Versions.Values.First();

                    foreach (var world in version.Worlds)
                    {
                        var className = GetName(world.Value.Name);

                        if (world.Value.Name == lastPart)
                        {
                            className += "_";
                        }

                        sb.Append("public readonly partial struct ").AppendLine(className);


                        sb.AppendLine("{");
                        sb.IncrementIndent();

                        // Fields
                        sb.AppendLine("private readonly ComponentInstance _instance;");
                        sb.AppendLine();

                        // Constructor
                        sb.Append("public ").Append(className).AppendLine("(ComponentInstance instance)");
                        sb.AppendLine("{");
                        sb.IncrementIndent();
                        sb.AppendLine("_instance = instance;");
                        sb.DecrementIndent();
                        sb.AppendLine("}");
                        sb.AppendLine();

                        // Exports
                        foreach (var export in world.Value.Exports)
                        {
                            if (export.Value.Kind != WitTypeKind.Func ||
                                export.Value is not WitFuncType funcType)
                            {
                                // Only functions are supported for now
                                continue;
                            }

                            WriteFunction(sb, funcType, export);
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
                }
            }

            sb.DecrementIndent();
            sb.AppendLine("}");

            ctx.AddSource("WitComponentBindings.g.cs", sb.ToString());
        });
    }

    private static Regex DashRegex { get; } = new(@"-(.)?", RegexOptions.Compiled);

    /// <summary>
    /// Change the name to a valid C# name.
    /// </summary>
    /// <example>
    /// 'foo-bar' becomes 'FooBar'
    /// </example>
    /// <param name="name">WIT name</param>
    /// <param name="uppercaseFirst">If <c>true</c>, the first character will be uppercased.</param>
    /// <returns>C# name</returns>
    private static string GetName(string name, bool uppercaseFirst = true)
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

    private static void WriteFunction(IndentedStringBuilder sb, WitFuncType funcType, KeyValuePair<string, WitType> export)
    {
        sb.Append("");
        var position = sb.Length;

        try
        {
            sb.Append("public ");

            if (funcType.Results.Length == 0)
            {
                sb.Append("void");
            }
            else if (funcType.Results.Length == 1)
            {
                funcType.Results[0].WriteCSharpType(sb);
            }
            else
            {
                sb.Append('(');
                for (var i = 0; i < funcType.Results.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    funcType.Results[i].WriteCSharpType(sb);
                }

                sb.Append(')');
            }

            sb.Append(' ').Append(GetName(export.Key)).Append('(');

            for (var i = 0; i < funcType.Parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var param = funcType.Parameters.ElementAt(i);
                param.Type.WriteCSharpType(sb);
                sb.Append(' ').Append(GetName(param.Name, false));
            }

            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.IncrementIndent();

            if (funcType.Parameters.Length > 0)
            {
                for (var index = 0; index < funcType.Parameters.Length; index++)
                {
                    var param = funcType.Parameters[index];
                    sb.Append("using var p").Append(index).Append(" = ");
                    param.Type.WriteCSharpValueCreation(sb, param.Name);
                    sb.Append('(').Append(GetName(param.Name, false)).AppendLine(");");
                }

                sb.AppendLine();

                sb.Append("Span<ComponentValue> parameters = ")
                    .Append("stackalloc ComponentValue[")
                    .Append(funcType.Parameters.Length)
                    .AppendLine("];");

                for (var i = 0; i < funcType.Parameters.Length; i++)
                {
                    sb.AppendLine($"parameters[{i}] = p{i};");
                }

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("Span<ComponentValue> parameters = default;");
            }

            sb.Append("using var result = _instance.Call(\"")
                .Append(export.Key)
                .Append("\", ")
                .Append(funcType.Results.Length)
                .AppendLine(", parameters);");

            if (funcType.Results.Length == 1)
            {
                sb.Append("return result.");
                funcType.Results[0].WriteCSharpValueAccessor(sb);
                sb.AppendLine("(0);");
            }
            else if (funcType.Results.Length > 1)
            {
                sb.AppendLine();
                sb.AppendLine("return (");
                sb.IncrementIndent();
                for (var i = 0; i < funcType.Results.Length; i++)
                {
                    if (i > 0) sb.AppendLine(",");
                    sb.Append("result.");
                    funcType.Results[i].WriteCSharpValueAccessor(sb);
                    sb.Append('(').Append(i).Append(')');
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
            sb.AppendLine($"// Failed to generate function '{export.Key}': {e.Message}");
            sb.AppendLine();
        }
    }
}
