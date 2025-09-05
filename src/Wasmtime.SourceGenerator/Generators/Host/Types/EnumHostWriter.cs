using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Generators.Host;

public class EnumHostWriter(
    WitPackageNameVersion package,
    string name
) : EnumHostWriterBase(WitTypeKind.Enum, package, name)
{
    protected override string TypeName => "Enum";
}

public class FlagsHostWriter(
    WitPackageNameVersion package,
    string name
) : EnumHostWriterBase(WitTypeKind.Enum, package, name)
{
    protected override string TypeName => "Flags";

    protected override void AddWriteCreateComponentValueArguments(IndentedStringBuilder sb,
        ITypeContainerResolver resolver)
    {
        sb.Append(", &");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.Expand");
    }

    protected override void AddWriteResultGetterArguments(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append(", &");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.Combine");
    }

    protected override void AddFromByteVectorArguments(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append(", &");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.Combine");
    }
}

public abstract class EnumHostWriterBase(
    WitTypeKind kind,
    WitPackageNameVersion package,
    string name
) : TypeHostWriter(kind)
{
    protected abstract string TypeName { get; }

    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::");
        package.PackageName.WritePath(sb);
        sb.Append('.');
        sb.Append(name);
    }

    /// <inheritdoc />
    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName,
        ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append(".To").Append(TypeName).Append("<");
        WriteCSharpType(sb, resolver);
        sb.Append(">(&");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.FromByteVector");
        AddFromByteVectorArguments(sb, resolver);
        sb.Append(')');
    }

    /// <inheritdoc />
    protected override void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey,
        ITypeContainerResolver resolver, bool externallyOwned)
    {
        sb.Append("global::Wasmtime.ComponentValue.Create").Append(TypeName).Append("<");
        WriteCSharpType(sb, resolver);
        sb.Append(">(").Append(paramKey);
        sb.Append(", &");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.ToByteVector");
        AddWriteCreateComponentValueArguments(sb, resolver);
        sb.Append(", copyConstants: ");
        sb.Append(externallyOwned ? "true" : "false");
        sb.Append(')');
    }

    protected virtual void AddWriteCreateComponentValueArguments(IndentedStringBuilder sb,
        ITypeContainerResolver resolver)
    {
    }

    protected virtual void AddWriteResultGetterArguments(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
    }

    protected virtual void AddFromByteVectorArguments(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
    }
}
