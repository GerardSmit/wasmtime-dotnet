namespace Wasmtime;

public enum ValueKind
{
    Int32 = 0,
    Int64 = 1,
    Float32 = 2,
    Float64 = 3,
    V128 = 4,
    FuncRef = 5,
    ExternRef = 6,
}
