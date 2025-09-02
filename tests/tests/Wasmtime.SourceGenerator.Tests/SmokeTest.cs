using System.Linq;
using Xunit.Abstractions;

namespace Wasmtime.SourceGenerator.Tests;

public class SmokeTest(ITestOutputHelper output)
{
    [Fact]
    public void Parse()
    {
        const string source =
            """
            package tests:test@0.1.0;

            world test {
                record point {
                    x: s32,
                    y: s32,
                }

                record entity {
                    id: s32,
                    name: string,
                    position: point,
                }

                export register-entity: func(e: entity);
                export get-entity: func(id: s32) -> entity;
            }
            """;

        var file = Wit.Parse(source);

        Assert.NotEmpty(file.Packages);

        var (_, content) = ComponentSourceGenerator.GenerateWitAccessor(file.Packages.First());
        Assert.NotEmpty(content);

        output.WriteLine(content);
    }
}
