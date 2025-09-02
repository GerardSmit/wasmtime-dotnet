namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a function type in WIT.
/// </summary>
public record WitRecordType(WitPackageNameVersion Package, string Name, EquatableArray<WitField> Fields) : WitType(WitTypeKind.Record)
{
    public override bool MustBeDisposed => true;

    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, WorldTypeResolver resolver)
    {
        sb.Append("global::");
        Package.PackageName.WritePath(sb);
        sb.Append('.');
        sb.Append(ComponentSourceGenerator.GetName(Name));
    }

    /// <inheritdoc />
    public override void WriteParameterInitializer(
        IndentedStringBuilder sb,
        string name,
        WorldTypeResolver resolver,
        bool isMemoryInitializer)
    {
        var builderName = $"builder_{name.Replace('.', '_')}";
        var arrName = $"ptr_{name.Replace('.', '_')}";

        foreach (var field in Fields)
        {
            field.Type.WriteParameterInitializer(sb, $"{name}.{field.CSharpName}", resolver, isMemoryInitializer: true);
        }

        sb.Append("// Initialize record ").Append(Name).Append(" (").Append(name).AppendLine(")");
        sb.Append("global::Wasmtime.RecordBuilderItem* ").Append(arrName).Append(" = stackalloc global::Wasmtime.RecordBuilderItem[").Append(Fields.Length).AppendLine("];");
        sb.Append("global::Wasmtime.RecordBuilder ").Append(builderName).Append(" = new global::Wasmtime.RecordBuilder(").Append(arrName).Append(", ").Append(Fields.Length).AppendLine(");");

        for (var i = 0; i < Fields.Length; i++)
        {
            var field = Fields[i];
            sb.Append(builderName).Append(".Set(").Append(i).Append(", global::Wit.Constants.")
                .Append(field.CSharpName)
                .Append(", ");

            field.Type.WriteComponentValue(sb, $"{name}.{field.CSharpName}", resolver);

            sb.AppendLine(");");
        }
    }

    /// <inheritdoc />
    public override void WriteParameterSetter(IndentedStringBuilder sb, string parametersVariable, string name, int startIndex, WorldTypeResolver resolver)
    {
        sb.Append(parametersVariable).Append("[").Append(startIndex).Append("] = global::Wasmtime.ComponentValue.CreateRecord(builder_").Append(name.Replace('.', '_')).AppendLine(");");
    }

    public override void WriteComponentValue(IndentedStringBuilder sb, string name, WorldTypeResolver resolver)
    {
        sb.Append("global::Wasmtime.ComponentValue.CreateRecord(builder_").Append(name.Replace('.', '_')).Append(")");
    }

    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, WorldTypeResolver resolver)
    {
        WriteCSharpType(sb, resolver);
        sb.Append(".Create(").Append(paramName).Append(".ToRecordBuilder())");
    }

    /// <inheritdoc />
    public override void WriteResultGetter(IndentedStringBuilder sb, string paramName, int index, WorldTypeResolver resolver)
    {
        WriteCSharpType(sb, resolver);
        sb.Append(".Create(").Append(paramName).Append(".GetRecordBuilder(").Append(index).Append("))");
    }

    public override int GetMemorySize(WorldTypeResolver resolver)
    {
        return Fields.Sum(f => f.Type.GetMemorySize(resolver));
    }

    public override void WriteBytes(IndentedStringBuilder sb, string name, string baseSpan, WorldTypeResolver resolver)
    {
        var offset = 0;

        var span = $"bytes_{name.Replace('.', '_')}";
        sb.Append("Span<byte> ").Append(span).Append(" = ").Append(baseSpan).AppendLine(";");

        foreach (var field in Fields)
        {
            var size = field.Type.GetMemorySize(resolver);

            field.Type.WriteBytes(
                sb,
                $"{name}.{field.CSharpName}",
                $"{span}.Slice({offset}, {size})",
                resolver);

            offset += size;
        }
    }
}

/// <summary>
/// Represents a function parameter in WIT.
/// </summary>
public readonly record struct WitField(
    string Name,
    WitType Type
)
{
    public string CSharpName { get; } = ComponentSourceGenerator.GetName(Name);
}