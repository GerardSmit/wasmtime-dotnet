namespace Wasmtime.Tests;

internal class TestImportsImpl : Wit.Tests.Component.TestImports
{
    public bool WasCalled { get; private set; }

    public override void Callback()
    {
        WasCalled = true;
    }

    public override string CallbackCombineString(string s1, string s2)
    {
        WasCalled = true;
        return s1 + s2;
    }
}
