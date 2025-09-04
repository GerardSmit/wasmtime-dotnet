namespace Wasmtime.Tests;

internal class TestImportsImpl : Wit.Tests.Component.TestImports
{
    public bool CallbackWasCalled { get; private set; }
    public bool CallbackCombineStringWasCalled { get; private set; }

    public Wit.Tests.Component.Types.Entity Entity { get; set; } = new Wit.Tests.Component.Types.Entity
    {
        Id = 1,
        Name = "Default Entity"
    };

    public override void Callback()
    {
        CallbackWasCalled = true;
    }

    public override string CallbackCombineString(string s1, string s2)
    {
        CallbackCombineStringWasCalled = true;
        return s1 + s2;
    }

    public override Wit.Tests.Component.Types.Entity GetHostEntity()
    {
        return Entity;
    }
}
