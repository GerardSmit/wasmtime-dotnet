namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a WIT type.
/// </summary>
/// <param name="Kind">The kind of WIT type.</param>
public record WitType(WitTypeKind Kind)
{
    public static WitType Bool { get; } = new(WitTypeKind.Bool);
    public static WitType U8 { get; } = new(WitTypeKind.U8);
    public static WitType U16 { get; } = new(WitTypeKind.U16);
    public static WitType U32 { get; } = new(WitTypeKind.U32);
    public static WitType U64 { get; } = new(WitTypeKind.U64);
    public static WitType S8 { get; } = new(WitTypeKind.S8);
    public static WitType S16 { get; } = new(WitTypeKind.S16);
    public static WitType S32 { get; } = new(WitTypeKind.S32);
    public static WitType S64 { get; } = new(WitTypeKind.S64);
    public static WitType F32 { get; } = new(WitTypeKind.F32);
    public static WitType F64 { get; } = new(WitTypeKind.F64);
    public static WitType Char { get; } = new(WitTypeKind.Char);
    public static WitType String { get; } = new(WitTypeKind.String);
    public static WitType EmptyResult { get; } = new(WitTypeKind.Result);

    public virtual bool MustBeDisposed => Kind is WitTypeKind.String;

    /// <summary>
    /// Gets the C# type that corresponds to this WIT type.
    /// </summary>
    /// <param name="resolver"></param>
    /// <returns>The C# type as a string.</returns>
    public virtual string GetCSharpType(WorldTypeResolver resolver)
    {
        return Kind switch
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
            _ => throw new NotSupportedException($"C# type mapping is not supported for WIT type kind '{Kind}'"),
        };
    }

    /// <summary>
    /// Writes the C# type that corresponds to this WIT type to the provided <see cref="IndentedStringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="resolver"></param>
    public virtual void WriteCSharpType(IndentedStringBuilder sb, WorldTypeResolver resolver)
    {
        sb.Append(GetCSharpType(resolver));
    }

    private void WriteCreateComponentValue(IndentedStringBuilder sb, string paramKey, WorldTypeResolver resolver)
    {
        sb.Append("global::Wasmtime.ComponentValue.");


        sb.Append(Kind switch
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
            _ => throw new NotSupportedException($"Parameter type '{Kind}' is not supported.")
        });

        sb.Append('(').Append(paramKey).Append(")");
    }

    /// <summary>
    /// Writes the C# code to access the value from a <c>ComponentValue</c> for this WIT type.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="paramName">The name of the parameter variable.</param>
    /// <param name="index">The index of the parameter in the parameter list.</param>
    /// <param name="resolver"></param>
    public virtual void WriteResultGetter(IndentedStringBuilder sb, string paramName, int index, WorldTypeResolver resolver)
    {
        sb.Append(paramName).Append('.');

        sb.Append(Kind switch
        {
            WitTypeKind.Bool => "GetBoolean",
            WitTypeKind.U8 => "GetByte",
            WitTypeKind.S8 => "GetSByte",
            WitTypeKind.U16 => "GetUInt16",
            WitTypeKind.S16 => "GetInt16",
            WitTypeKind.U32 => "GetUInt32",
            WitTypeKind.S32 => "GetInt32",
            WitTypeKind.U64 => "GetUInt64",
            WitTypeKind.S64 => "GetInt64",
            WitTypeKind.F32 => "GetFloat",
            WitTypeKind.F64 => "GetDouble",
            WitTypeKind.Char => "GetChar",
            WitTypeKind.String => "GetString",
            WitTypeKind.Record => "GetRecordBuilder",
            _ => throw new NotSupportedException($"Return type '{Kind}' is not supported.")
        });

        sb.Append('(').Append(index).Append(')');
    }

    /// <summary>
    /// Writes the C# code to access the value from a <c>ComponentValue</c> for this WIT type.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="paramName">The name of the parameter variable.</param>
    /// <param name="index">The index of the parameter in the parameter list.</param>
    /// <param name="resolver"></param>
    public virtual void WriteValueGetter(IndentedStringBuilder sb, string paramName, WorldTypeResolver resolver)
    {
        sb.Append(paramName).Append('.');

        sb.Append(Kind switch
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
            _ => throw new NotSupportedException($"Return type '{Kind}' is not supported.")
        });

        sb.Append("()");
    }

    /// <summary>
    /// Gets the amount of parameters this WIT type will consume when used as a function parameter.
    /// </summary>
    /// <param name="resolver">The type resolver.</param>
    /// <returns>The number of parameters.</returns>
    public virtual int GetParameterSize(WorldTypeResolver resolver) => 1;

    /// <summary>
    /// Writes the parameter declaration.
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="resolver">The type resolver.</param>
    public virtual void WriteParameter(IndentedStringBuilder sb, string name, WorldTypeResolver resolver)
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
    /// <param name="isMemoryInitializer"></param>
    public virtual void WriteParameterInitializer(IndentedStringBuilder sb, string name, WorldTypeResolver resolver, bool isMemoryInitializer)
    {
        if (!MustBeDisposed) return;

        sb.Append("using global::Wasmtime.ComponentValue value_").Append(name.Replace('.', '_')).Append(" = ");
        WriteCreateComponentValue(sb, name, resolver);
        sb.AppendLine(";");
    }

    /// <summary>
    /// Writes the code to set the parameter value in the parameters <see cref="Span{T}"/>
    /// </summary>
    /// <param name="sb">The <see cref="IndentedStringBuilder"/> to write to.</param>
    /// <param name="parametersVariable">The name of the parameters variable.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="startIndex">The start index in the parameters span.</param>
    /// <param name="resolver">The type resolver.</param>
    public virtual void WriteParameterSetter(IndentedStringBuilder sb, string parametersVariable, string name, int startIndex, WorldTypeResolver resolver)
    {
        sb.Append(parametersVariable).Append("[").Append(startIndex).Append("] = ");
        WriteComponentValue(sb, name, resolver);
        sb.AppendLine(";");
    }

    public virtual void WriteBytes(IndentedStringBuilder sb, string name, string span, WorldTypeResolver resolver)
    {
        WriteComponentValue(sb, name, resolver);
        sb.Append(".WriteBytes(").Append(span).AppendLine(");");
    }

    public virtual void WriteComponentValue(IndentedStringBuilder sb, string name, WorldTypeResolver resolver)
    {
        if (MustBeDisposed)
        {
            sb.Append("value_").Append(name.Replace('.', '_'));
        }
        else
        {
            WriteCreateComponentValue(sb, name, resolver);
        }
    }

    public virtual int GetMemorySize(WorldTypeResolver resolver)
    {
        return Kind switch
        {
            WitTypeKind.Bool => 1,
            WitTypeKind.U8 => 1,
            WitTypeKind.S8 => 1,
            WitTypeKind.U16 => 2,
            WitTypeKind.S16 => 2,
            WitTypeKind.U32 => 4,
            WitTypeKind.S32 => 4,
            WitTypeKind.F32 => 4,
            WitTypeKind.U64 => 8,
            WitTypeKind.S64 => 8,
            WitTypeKind.F64 => 8,
            WitTypeKind.Char => 4,
            WitTypeKind.String => 8, // pointer + length
            _ => throw new NotSupportedException($"Memory size calculation is not supported for WIT type kind '{Kind}'"),
        };
    }
}