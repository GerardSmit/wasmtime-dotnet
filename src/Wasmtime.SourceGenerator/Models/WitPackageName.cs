namespace Wasmtime.SourceGenerator.Models;

public record struct WitPackageName(
    EquatableArray<string> Namespace,
    EquatableArray<string> Name
)
{
    public EquatableArray<string> AllParts { get; } = Combine(Namespace, Name);

    private static EquatableArray<string> Combine(EquatableArray<string> equatableArray, EquatableArray<string> name)
    {
        var result = new string[equatableArray.Length + name.Length];
        equatableArray.AsSpan().CopyTo(result);
        name.AsSpan().CopyTo(result.AsSpan(equatableArray.Length));
        return new EquatableArray<string>(result);
    }
}