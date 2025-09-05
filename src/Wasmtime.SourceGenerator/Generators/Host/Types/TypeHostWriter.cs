using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Generators.Host;

public class TypeHostWriter(WitTypeKind kind)
{
    public virtual bool MustBeDisposed => kind is WitTypeKind.String;

    /// <summary>
    /// Writes the C# type that corresponds to this WIT type to the provided <see cref="IndentedStringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="resolver"></param>
    public virtual void WriteCSharpType(IndentedStringBuilder sb, ITypeContainerResolver resolver)
    {
        sb.Append(kind switch
        {
            WitTypeKind.Bool => "bool",
            WitTypeKind.U8 => "byte",
            WitTypeKind.U16 => "ushort",
            WitTypeKind.U32 => "uint",
            WitTypeKind.U64 => "ulong",
            WitTypeKind.S8 => "sbyte",
            WitTypeKind.S16 => "short",
            WitTypeKind.S32 => "int",
            WitTypeKind.S64 => "long",
            WitTypeKind.F32 => "float",
            WitTypeKind.F64 => "double",
            WitTypeKind.Char => "char",
            WitTypeKind.String => "string",
            WitTypeKind.Result => "global::Wasmtime.Result",
            _ => throw new NotSupportedException($"C# type mapping is not supported for WIT type kind '{kind}'"),
        });
    }

    protected virtual void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey, ITypeContainerResolver resolver, bool externallyOwned)
    {
        sb.Append("global::Wasmtime.ComponentValue.");

        sb.Append(kind switch
        {
            WitTypeKind.Bool => "CreateBoolean",
            WitTypeKind.U8 => "CreateByte",
            WitTypeKind.S8 =>  "CreateSByte",
            WitTypeKind.U16 => "CreateUInt16",
            WitTypeKind.S16 => "CreateInt16",
            WitTypeKind.U32 => "CreateUInt32",
            WitTypeKind.S32 => "CreateInt32",
            WitTypeKind.U64 => "CreateUInt64",
            WitTypeKind.S64 => "CreateInt64",
            WitTypeKind.F32 => "CreateFloat",
            WitTypeKind.F64 => "CreateDouble",
            WitTypeKind.Char => "CreateChar",
            WitTypeKind.String => "CreateString",
            WitTypeKind.Borrow => "CreateBorrow",
            _ => throw new NotSupportedException($"Parameter type '{kind}' is not supported.")
        });

        sb.Append('(').Append(paramKey);

        if (kind is WitTypeKind.String)
        {
            sb.Append(", externallyOwned: ").Append(externallyOwned ? "true" : "false");
        }

        sb.Append(")");
    }

    public virtual void WriteResultGetterInitializer(IndentedStringBuilder sb, string paramName, int index, ITypeContainerResolver resolver)
    {

    }

    /// <summary>
    /// Writes the C# code to access the value from a <c>ComponentValue</c> for this WIT type.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="paramName">The name of the parameter variable.</param>
    /// <param name="index">The index of the parameter in the parameter list.</param>
    /// <param name="resolver"></param>
    public virtual void WriteResultGetter(IndentedStringBuilder sb, string paramName, int index, ITypeContainerResolver resolver)
    {
        WriteValueGetter(sb, $"{paramName}[{index}]", $"{paramName}_{index}", resolver);
    }

    /// <summary>
    /// Writes any initialization code needed before accessing the value from a <c>ComponentValue</c> for this WIT type.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="paramName">The name of the parameter variable.</param>
    /// <param name="resolver">The type resolver.</param>
    public virtual void WriteValueGetterInitializer(IndentedStringBuilder sb, string paramName, string uniqueName, ITypeContainerResolver resolver)
    {

    }

    /// <summary>
    /// Writes the C# code to access the value from a <c>ComponentValue</c> for this WIT type.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="paramName">The name of the parameter variable.</param>
    /// <param name="resolver">The type resolver.</param>
    public virtual void WriteValueGetter(IndentedStringBuilder sb, string paramName, string uniqueName, ITypeContainerResolver resolver)
    {
        sb.Append(paramName).Append('.');

        sb.Append(kind switch
        {
            WitTypeKind.Bool => "ToBoolean",
            WitTypeKind.U8 => "ToByte",
            WitTypeKind.S8 => "ToSByte",
            WitTypeKind.U16 => "ToUInt16",
            WitTypeKind.S16 => "ToInt16",
            WitTypeKind.U32 => "ToUInt32",
            WitTypeKind.S32 => "ToInt32",
            WitTypeKind.U64 => "ToUInt64",
            WitTypeKind.S64 => "ToInt64",
            WitTypeKind.F32 => "ToFloat",
            WitTypeKind.F64 => "ToDouble",
            WitTypeKind.Char => "ToChar",
            WitTypeKind.String => "ToStringValue",
            WitTypeKind.Result => "ToResult",
            _ => throw new NotSupportedException($"Return type '{kind}' is not supported.")
        });

        sb.Append("()");
    }

    /// <summary>
    /// Gets the amount of parameters this WIT type will consume when used as a function parameter.
    /// </summary>
    /// <param name="resolver">The type resolver.</param>
    /// <returns>The number of parameters.</returns>
    public virtual int GetParameterSize(ITypeContainerResolver resolver) => 1;

    /// <summary>
    /// Writes the parameter declaration.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="resolver">The type resolver.</param>
    public virtual void WriteParameter(IndentedStringBuilder sb, string name, ITypeContainerResolver resolver)
    {
        WriteCSharpType(sb, resolver);
        sb.Append(' ').Append(name);
    }

    /// <summary>
    /// Writes the parameter initialization code.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="resolver">The type resolver.</param>
    /// <param name="ignoreDispose"></param>
    /// <param name="externallyOwned"></param>
    public virtual void WriteParameterInitializer(IndentedStringBuilder sb, string name,
        ITypeContainerResolver resolver, bool ignoreDispose, bool externallyOwned)
    {
        if (!MustBeDisposed || ignoreDispose)
        {
            return;
        }

        sb.Append("using global::Wasmtime.ComponentValue value_").Append(name.ToSafeVariable()).Append(" = ");
        WriteCreateComponentValue(sb, name, resolver, externallyOwned);
        sb.AppendLine(";");
    }

    /// <summary>
    /// Writes the code to set the parameter value in the parameters <see cref="Span{T}"/>
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="parametersVariable">The name of the parameters variable.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="startIndex">The start index in the parameters span.</param>
    /// <param name="ignoreDispose"></param>
    /// <param name="resolver">The type resolver.</param>
    /// <param name="externallyOwned"></param>
    public virtual void WriteParameterSetter(IndentedStringBuilder sb, string parametersVariable, string name,
        int startIndex, bool ignoreDispose, ITypeContainerResolver resolver, bool externallyOwned)
    {
        sb.Append(parametersVariable).Append("[").Append(startIndex).Append("] = ");
        WriteComponentValue(sb, name, ignoreDispose, resolver, externallyOwned);
        sb.AppendLine(";");
    }

    public virtual void WriteBytes(IndentedStringBuilder sb, string name, string span, ITypeContainerResolver resolver)
    {
        WriteComponentValue(sb, name, false, resolver, externallyOwned: false);
        sb.Append(".WriteBytes(").Append(span).AppendLine(");");
    }

    public virtual void WriteComponentValue(IndentedStringBuilder sb, string name, bool ignoreDispose,
        ITypeContainerResolver resolver, bool externallyOwned)
    {
        if (MustBeDisposed && !ignoreDispose)
        {
            sb.Append("value_").Append(name.ToSafeVariable());
        }
        else
        {
            WriteCreateComponentValue(sb, name, resolver, externallyOwned);
        }
    }
}
