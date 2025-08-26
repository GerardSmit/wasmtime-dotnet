namespace Wasmtime;

public enum WasmExternKind : byte
{
    Func = 0,
    Global = 1,
    Table = 2,
    Memory = 3,
}
