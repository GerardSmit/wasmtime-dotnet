namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a function type in WIT.
/// </summary>
public record WitRecordType(WitPackageNameVersion Package, string Name, EquatableArray<WitField> Fields) : WitType(WitTypeKind.Record)
{
    public override bool MustBeDisposed => true;

    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::");
        Package.PackageName.WritePath(sb);
        sb.Append('.');
        sb.Append(ComponentSourceGenerator.GetName(Name));
    }

    protected override void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey,
        ITypeContainerResolver resolver, bool externallyOwned)
    {
        sb.Append("global::Wasmtime.ComponentValue.CreateRecord(")
            .Append(paramKey)
            .Append(".ToRecordBuilder(copyConstants: ")
            .Append(externallyOwned ? "true" : "false")
            .Append("), externallyOwned: ")
            .Append(externallyOwned ? "true" : "false")
            .Append(")");
    }

    public override void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName, ITypeContainerResolver resolver)
    {
        WriteCSharpType(sb, resolver);
        sb.Append(".FromRecordBuilder(").Append(paramName).Append(".ToRecordBuilder())");
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