namespace Wasmtime.SourceGenerator.Models;

public record WitType(
    WitTypeKind Kind
)
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

    public virtual string GetCSharpType()
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

    public virtual void WriteCSharpType(IndentedStringBuilder sb)
    {
        sb.Append(GetCSharpType());
    }

    public virtual void WriteCSharpValueCreation(IndentedStringBuilder sb, string paramKey)
    {
        sb.Append(Kind switch
        {
            WitTypeKind.Bool => "ComponentValue.CreateBoolean",
            WitTypeKind.U8 => "ComponentValue.CreateByte",
            WitTypeKind.S8 =>  "ComponentValue.CreateSByte",
            WitTypeKind.U16 => "ComponentValue.CreateUInt16",
            WitTypeKind.S16 => "ComponentValue.CreateInt16",
            WitTypeKind.U32 => "ComponentValue.CreateUInt32",
            WitTypeKind.S32 => "ComponentValue.CreateInt32",
            WitTypeKind.U64 => "ComponentValue.CreateUInt64",
            WitTypeKind.S64 => "ComponentValue.CreateInt64",
            WitTypeKind.F32 => "ComponentValue.CreateFloat",
            WitTypeKind.F64 => "ComponentValue.CreateDouble",
            WitTypeKind.Char => "ComponentValue.CreateChar",
            WitTypeKind.String => "ComponentValue.CreateString",
            _ => throw new NotSupportedException($"Parameter type '{Kind}' is not supported.")
        });
    }

    public virtual void WriteCSharpValueAccessor(IndentedStringBuilder sb)
    {
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
            _ => throw new NotSupportedException($"Return type '{Kind}' is not supported.")
        });
    }
}