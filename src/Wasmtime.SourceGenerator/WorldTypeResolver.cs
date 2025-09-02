using Wasmtime.SourceGenerator.Models;

namespace Wasmtime.SourceGenerator;

public class WorldTypeResolver(WitPackageVersion package, WitWorld world)
{
    public WitType Resolve(string name)
    {
        foreach (var typeDef in world.Items.Concat(package.Items))
        {
            if (typeDef is WitRecord record && record.Name == name)
            {
                return record.Type;
            }
        }

        throw new InvalidOperationException($"Could not resolve type '{name}'.");
    }
}
