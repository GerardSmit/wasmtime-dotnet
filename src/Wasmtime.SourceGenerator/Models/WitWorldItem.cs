namespace Wasmtime.SourceGenerator.Models;

/// <summary>
/// Base class for items that can appear in a WIT world (interfaces, types, etc.).
/// </summary>
public abstract record WitWorldItem : WitTypeDef;