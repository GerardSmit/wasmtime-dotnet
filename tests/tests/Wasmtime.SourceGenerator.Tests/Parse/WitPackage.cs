namespace Wasmtime.SourceGenerator.Tests;

public class WitPackage
{
    public static TheoryData<string, string[], string[], string> PackageVersionTestData => new()
    {
        // Input, ExpectedNamespaces, ExpectedName, ExpectedVersion
        { "ns:name", ["ns"], ["name"], "0.0.0" },
        { "ns:name@1.2.3", ["ns"], ["name"], "1.2.3" },
        { "name", [], ["name"], "0.0.0" },
        { "name@1.2.3", [], ["name"], "1.2.3" },
        { "ns1:ns2:name@1.2.3", ["ns1", "ns2"], ["name"], "1.2.3" },
        { "p1/p2", [], ["p1", "p2"], "0.0.0" },
        { "ns1:p1/p2", ["ns1"], ["p1", "p2"], "0.0.0" },
        { "ns1:ns2:p1/p2@1.2.3", ["ns1", "ns2"], ["p1", "p2"], "1.2.3" },
        { "ns1:ns2:p1/p2@1", ["ns1", "ns2"], ["p1", "p2"], "1.0.0" }
    };

    [Theory]
    [MemberData(nameof(PackageVersionTestData))]
    public void Parse_PackageName(string input, string[] expectedNamespaces, string[] expectedName, string expectedVersion)
    {
        var file = Wit.Parse($"package {input};");

        var package = Assert.Single(file.Packages.Values);
        Assert.Equal(expectedNamespaces, package.PackageName.Namespace);
        Assert.Equal(expectedName, package.PackageName.Name);

        var version = Assert.Single(package.Versions.Keys);
        Assert.Equal(expectedVersion, version);
    }

    [Theory]
    [MemberData(nameof(PackageVersionTestData))]
    public void Parse_PackageBlock(string input, string[] expectedNamespaces, string[] expectedName, string expectedVersion)
    {
        var file = Wit.Parse($"package {input} {{}};");

        var package = Assert.Single(file.Packages.Values);
        Assert.Equal(expectedNamespaces, package.PackageName.Namespace);
        Assert.Equal(expectedName, package.PackageName.Name);

        var version = Assert.Single(package.Versions.Keys);
        Assert.Equal(expectedVersion, version);
    }
}
