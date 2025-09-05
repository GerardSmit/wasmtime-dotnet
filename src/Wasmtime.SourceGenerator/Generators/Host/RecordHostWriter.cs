using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Generators.Host;

public class RecordHostWriter(WitPackageNameVersion package, string name) : HostWriter(WitTypeKind.Record)
{
    public override bool MustBeDisposed => true;

    /// <inheritdoc />
    public override void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append("global::");
        package.PackageName.WritePath(sb);
        sb.Append('.');
        sb.Append(ComponentSourceGenerator.GetName(name));
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
