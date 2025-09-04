namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a list type in WIT.
/// </summary>
public record WitListType(
    WitType ElementType
) : WitType(WitTypeKind.List)
{
    public override bool MustBeDisposed => true;

    /// <inheritdoc />
    public override void WriteParameter(IndentedStringBuilder sb, string name, ITypeContainerResolver resolver)
    {
        // Use ReadOnlySpan<T> for method parameters to avoid unnecessary allocations.
        sb.Append("global::System.ReadOnlySpan<");
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append("> ").Append(name);
    }

    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append("[]");
    }

    /// <inheritdoc />
    public override void WriteParameterInitializer(
        IndentedStringBuilder sb,
        string name,
        ITypeContainerResolver resolver,
        bool ignoreDispose,
        bool copyConstants)
    {
        var builderName = $"builder_{name.ToSafeVariable()}";
        var indexName = $"i_{name.ToSafeVariable()}";
        var itemName = $"{name}[{indexName}]";

        sb.AppendLine("// Convert array to list builder");

        if (!ignoreDispose)
        {
            sb.Append("using ");
        }

        sb.Append("global::Wasmtime.ListBuilder ").Append(builderName).Append(" = new global::Wasmtime.ListBuilder(").Append(name).AppendLine(".Length);");
        sb.Append("for (int ").Append(indexName).Append(" = 0; ").Append(indexName).Append(" < ").Append(name).Append(".Length; ").Append(indexName).AppendLine("++)");
        sb.AppendLine("{");
        sb.IncrementIndent();
        ElementType.WriteParameterInitializer(sb, itemName, resolver, ignoreDispose: true, copyConstants: copyConstants);

        sb.Append(builderName).Append("[").Append(indexName).Append("] = ");
        ElementType.WriteComponentValue(sb, itemName, ignoreDispose: true, resolver: resolver, copyConstants: copyConstants);

        sb.AppendLine(";");
        sb.DecrementIndent();
        sb.AppendLine("}");
    }

    /// <inheritdoc />
    public override void WriteParameterSetter(IndentedStringBuilder sb, string parametersVariable, string name,
        int startIndex, bool ignoreDispose, ITypeContainerResolver resolver, bool copyConstants)
    {
        sb.Append(parametersVariable).Append("[").Append(startIndex).Append("] = global::Wasmtime.ComponentValue.CreateList(builder_").Append(name.ToSafeVariable()).AppendLine(");");
    }

    /// <inheritdoc />
    public override void WriteComponentValue(IndentedStringBuilder sb, string name, bool ignoreDispose,
        ITypeContainerResolver resolver, bool copyConstants)
    {
        sb.Append("global::Wasmtime.ComponentValue.CreateList(builder_").Append(name.ToSafeVariable()).Append(")");
    }

    /// <inheritdoc />
    public override void WriteResultGetterInitializer(IndentedStringBuilder sb, string paramName, int index, ITypeContainerResolver resolver)
    {
        var parameterName = $"{paramName}_{index}";
        var builderName = $"{parameterName}_b";
        var indexName = $"{parameterName}_i";

        sb.AppendLine("// Convert list builder to array");
        sb.Append("global::Wasmtime.ListBuilder ").Append(builderName).Append(" = ")
            .Append(paramName).Append("[").Append(index).AppendLine("].ToListBuilder();");

        WriteToArray(sb, resolver, parameterName, builderName, indexName);
    }

    /// <inheritdoc />
    public override void WriteResultGetter(IndentedStringBuilder sb, string paramName, int index, ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append('_').Append(index);
    }

    public override void WriteValueGetterInitializer(IndentedStringBuilder sb, string paramName, string uniqueName, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.ListBuilder builder_").Append(uniqueName).Append(" = ").Append(paramName).Append(".ToListBuilder();").AppendLine();

        WriteToArray(sb, resolver, uniqueName, "builder_" + uniqueName, "i");
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName, ITypeContainerResolver resolver)
    {
        sb.Append(uniqueName);
    }

    private void WriteToArray(
        IndentedStringBuilder sb,
        ITypeContainerResolver resolver,
        string parameterName,
        string builderName,
        string indexName)
    {
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append("[] ").Append(parameterName).Append(" = new ");
        ElementType.WriteCSharpType(sb, resolver);
        sb.Append("[").Append(builderName).AppendLine(".Length];");

        sb.Append("for (int ").Append(indexName).Append(" = 0; ").Append(indexName).Append(" < ").Append(parameterName).Append(".Length; ").Append(indexName).AppendLine("++)");
        sb.AppendLine("{");
        sb.IncrementIndent();
        sb.Append(parameterName).Append("[").Append(indexName).Append("] = ");
        ElementType.WriteValueGetter(sb, $"{builderName}[{indexName}]", parameterName + "_i", resolver);
        sb.AppendLine(";");
        sb.DecrementIndent();
        sb.AppendLine("}");
    }
}