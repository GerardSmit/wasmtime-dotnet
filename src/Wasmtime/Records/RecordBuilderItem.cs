using Wasmtime.Interop;

namespace Wasmtime;

/// <summary>
/// Represents a single entry in a record builder.
/// </summary>
public struct RecordBuilderItem
{
    private wasmtime_component_valrecord_entry _entry;
}
