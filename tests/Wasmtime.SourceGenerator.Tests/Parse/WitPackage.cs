using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Wasmtime.SourceGenerator.Models;

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
    public void Parse_PackageName(string input, string[] expectedNamespaces, string[] expectedName,
        string expectedVersion)
    {
        var file = Wit.Parse($"package {input};");

        var package = Assert.Single(file.Packages.Values);
        Assert.Equal(expectedNamespaces, package.PackageName.Namespace);
        Assert.Equal(expectedName, package.PackageName.Name);

        var version = Assert.Single(package.Versions.Keys);
        Assert.Equal(expectedVersion, version.ToString());
    }

    [Theory]
    [MemberData(nameof(PackageVersionTestData))]
    public void Parse_PackageBlock(string input, string[] expectedNamespaces, string[] expectedName,
        string expectedVersion)
    {
        var file = Wit.Parse($"package {input} {{}}");

        var package = Assert.Single(file.Packages.Values);
        Assert.Equal(expectedNamespaces, package.PackageName.Namespace);
        Assert.Equal(expectedName, package.PackageName.Name);

        var version = Assert.Single(package.Versions.Keys);
        Assert.Equal(expectedVersion, version.ToString());
    }

    public static TheoryData<string[], string> LastPackageVersionTestData => new()
    {
        // Versions, ExpectedLastVersion
        { ["1.0.0", "1.2.3", "2.0.0"], "2.0.0" },
        { ["0.1.0", "0.1.1", "0.2.0"], "0.2.0" },
        { ["1.0.0", "1.0.0-beta", "1.0.1"], "1.0.1" },
        { ["1.0.0", "1.0.0-alpha", "1.0.0-beta"], "1.0.0" },
        { ["1.0.0", "1.0.1-alpha", "1.0.1-beta"], "1.0.1-beta" },
        { ["1.0.0", "1.0.0+build"], "1.0.0" },
        { ["1.0.0", "1.0.1+build1", "1.0.1+build2"], "1.0.1" }
    };

    [Theory]
    [MemberData(nameof(LastPackageVersionTestData))]
    public void Parse_LastPackageVersion(string[] versions, string expectedLastVersion)
    {
        var packageDefinitions = string.Join("\n", versions.Select(v => $"package test@{v} {{}}"));
        var file = Wit.Parse(packageDefinitions);

        var package = Assert.Single(file.Packages.Values);
        var lastVersion = package.LastVersion;

        Assert.Equal(expectedLastVersion, lastVersion.ToString());
        Assert.True(package.Versions.ContainsKey(lastVersion));
    }

    [Fact]
    public void DisallowMultipleFilePackagesInDirectory()
    {
        const string path = @"C:\path\to\directory";
        const string firstPackage = "test@1.0.0";
        const string secondPackage = "test@2.0.0";

        var expectedMessage = string.Format(
            DiagnosticMessages.MultipleFilePackagesInDirectory.MessageFormat.ToString(),
            firstPackage,
            secondPackage,
            path);

        var result = Wit.Parse(new WitRawDirectory(
            Path: path,
            Files: ImmutableArray.Create(
                $"package {firstPackage};",
                $"package {secondPackage};"
            )));

        var diagnostic = (Diagnostic) Assert.Single(result.Diagnostics);

        Assert.Empty(result.Packages);
        Assert.Equal(DiagnosticMessages.MultipleFilePackagesInDirectory.Id, diagnostic.Descriptor.Id);
        Assert.Equal(expectedMessage, diagnostic.GetMessage());
    }
}
