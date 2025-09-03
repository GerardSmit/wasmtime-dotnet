using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator.Visitors;

/// <summary>
/// Visits a type context and produces a corresponding <see cref="WitType"/>.
/// </summary>
public class WitTypeVisitor(WitPackageNameVersion package) : WitParserBaseVisitor<WitType>
{
    public override WitType VisitS8Type(WitParser.S8TypeContext context)
    {
        return WitType.S8;
    }

    public override WitType VisitF64Type(WitParser.F64TypeContext context)
    {
        return WitType.F64;
    }

    public override WitType VisitCustomType(WitParser.CustomTypeContext context)
    {
        var name = context.identifier().GetTextWithoutEscape();

        return new WitCustomType(package, name);
    }

    public override WitType VisitExternalType(WitParser.ExternalTypeContext context)
    {
        var fullName = WitPackageNameVersion.Parse(context.packageName());
        var (name, externalPackage) = fullName.WithoutLastNamePart();

        return new WitCustomType(externalPackage, name);
    }

    public override WitType VisitExternType(WitParser.ExternTypeContext context)
    {
        if (context.func() is {} func)
        {
            return VisitFunc(func);
        }

        if (context.@interface() is {} interf)
        {
            return VisitInterface(interf);
        }

        throw new NotSupportedException("Unknown extern type");
    }

    public override WitType VisitBoolType(WitParser.BoolTypeContext context)
    {
        return WitType.Bool;
    }

    public override WitType VisitStreamType(WitParser.StreamTypeContext context)
    {
        return new WitStreamType(
            Visit(context.type())
        );
    }

    public override WitType VisitResultNoErrorType(WitParser.ResultNoErrorTypeContext context)
    {
        return new WitResultNoErrorType(
            Visit(context.type())
        );
    }

    public override WitType VisitU16Type(WitParser.U16TypeContext context)
    {
        return WitType.U16;
    }

    public override WitType VisitS64Type(WitParser.S64TypeContext context)
    {
        return WitType.S64;
    }

    public override WitType VisitFuncType(WitParser.FuncTypeContext context)
    {
        return Visit(context.func());
    }

    public override WitType VisitFunc(WitParser.FuncContext context)
    {
        var parameters = context.funcParamList().funcParam()
            .Select(x => new WitFuncParameter(x.identifier().GetTextWithoutEscape(), Visit(x.type())))
            .ToArray();

        var results = context.funcResult()
            .type()
            .Select(Visit)
            .ToArray();

        return new WitFuncType(
            parameters,
            results
        );
    }

    public override WitType VisitResultType(WitParser.ResultTypeContext context)
    {
        return new WitResultType(
            Visit(context.type(0)),
            Visit(context.type(1))
        );
    }

    public override WitType VisitResultEmptyType(WitParser.ResultEmptyTypeContext context)
    {
        return WitType.EmptyResult;
    }

    public override WitType VisitU32Type(WitParser.U32TypeContext context)
    {
        return WitType.U32;
    }

    public override WitType VisitU64Type(WitParser.U64TypeContext context)
    {
        return WitType.U64;
    }

    public override WitType VisitS16Type(WitParser.S16TypeContext context)
    {
        return WitType.S16;
    }

    public override WitType VisitU8Type(WitParser.U8TypeContext context)
    {
        return WitType.U8;
    }

    public override WitType VisitResultNoResultType(WitParser.ResultNoResultTypeContext context)
    {
        return new WitResultNoResultType(
            Visit(context.type())
        );
    }

    public override WitType VisitStringTypeType(WitParser.StringTypeTypeContext context)
    {
        return WitType.String;
    }

    public override WitType VisitListType(WitParser.ListTypeContext context)
    {
        return new WitListType(
            Visit(context.type())
        );
    }

    public override WitType VisitBorrowType(WitParser.BorrowTypeContext context)
    {
        return new WitBorrowType(
            Visit(context.type())
        );
    }

    public override WitType VisitS32Type(WitParser.S32TypeContext context)
    {
        return WitType.S32;
    }

    public override WitType VisitTupleType(WitParser.TupleTypeContext context)
    {
        var elementTypes = context.type()
            .Select(Visit)
            .ToArray();

        return new WitTupleType(
            elementTypes
        );
    }

    public override WitType VisitCharType(WitParser.CharTypeContext context)
    {
        return WitType.Char;
    }

    public override WitType VisitF32Type(WitParser.F32TypeContext context)
    {
        return WitType.F32;
    }

    public override WitType VisitOptionType(WitParser.OptionTypeContext context)
    {
        return new WitOptionType(
            Visit(context.type())
        );
    }
}
