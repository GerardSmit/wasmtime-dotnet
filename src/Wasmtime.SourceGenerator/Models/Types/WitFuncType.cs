namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Represents a function type in WIT.
/// </summary>
public record WitFuncType(
    EquatableArray<WitFuncParameter> Parameters,
    EquatableArray<WitType> Results
) : WitType(WitTypeKind.Func);

/// <summary>
/// Represents a function parameter in WIT.
/// </summary>
public record WitFuncParameter(
    string Name,
    WitType Type
)
{
    public string CSharpName { get; } = StringUtils.GetName(Name);

    public string CSharpVariableName { get; } = StringUtils.GetName(Name, uppercaseFirst: false);
}