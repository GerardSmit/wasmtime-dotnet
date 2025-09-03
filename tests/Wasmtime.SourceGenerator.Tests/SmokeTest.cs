using System.IO;
using System.Linq;
using Wasmtime.SourceGenerator.Models;
using Xunit.Abstractions;

namespace Wasmtime.SourceGenerator.Tests;

public class SmokeTest(ITestOutputHelper output)
{
    [Fact]
    public void Parse()
    {
        var directory = Path.GetFullPath("wit");
        Assert.True(Directory.Exists(directory));

        var files = Directory.GetFiles(directory, "*.wit", SearchOption.AllDirectories);
        Assert.NotEmpty(files);

        var witRawDirectories = files
            .GroupBy(Path.GetDirectoryName)
            .ToDictionary(
                g => g.Key ?? string.Empty,
                g => new WitRawDirectory(
                    g.Key,
                    g.Select(File.ReadAllText).ToArray()));

        var witDirectories = witRawDirectories
            .ToDictionary(kv => kv.Key, kv => Wit.Parse(kv.Value));

        var allPackages = witDirectories
            .SelectMany(d => d.Value.Packages)
            .Select(p => p.Value);

        var projectResolver = new ProjectTypeContainerResolver(allPackages);

        foreach (var kv in projectResolver.Packages)
        {
            var (_, content) = ComponentSourceGenerator.GenerateWitAccessor(kv, projectResolver);
            Assert.NotEmpty(content);

            output.WriteLine($"--- {kv.Key} ---");
            output.WriteLine(content);
        }
    }
}
