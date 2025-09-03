namespace Wasmtime.SourceGenerator.Models;

public record WitEnumType(
    WitPackageNameVersion Package,
    string Name
) : WitEnumBaseType(Package, Name, WitTypeKind.Enum)
{
    protected override string TypeName => "Enum";
}

public record WitFlagsType(
    WitPackageNameVersion Package,
    string Name
) : WitEnumBaseType(Package, Name, WitTypeKind.Flags)
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

public abstract record WitEnumBaseType(WitPackageNameVersion Package, string Name, WitTypeKind Kind) : WitType(Kind)
{
    protected abstract string TypeName { get; }

    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::");
        Package.PackageName.WritePath(sb);
        sb.Append('.');
        sb.Append(ComponentSourceGenerator.GetName(Name));
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
    protected override void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey, ITypeContainerResolver resolver)
    {
        sb.Append("global::Wasmtime.ComponentValue.Create").Append(TypeName).Append("<");
        WriteCSharpType(sb, resolver);
        sb.Append(">(").Append(paramKey);
        sb.Append(", &");
        WriteCSharpType(sb, resolver);
        sb.Append("Helper.ToByteVector");
        AddWriteCreateComponentValueArguments(sb, resolver);
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