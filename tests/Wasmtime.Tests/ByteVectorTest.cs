using Xunit;

namespace Wasmtime.Tests;

public class ByteVectorTest
{
    public static TheoryData<int> Lengths => new()
    {
        // Lengths
        0,
        1,
        10,
        32,
        100,
        256,
        1024,
        4096,
    };

    [Theory]
    [MemberData(nameof(Lengths))]
    public void ByteVectors_AreEqual(int length)
    {
        var str = new string('a', length);
        using var left = new ByteVector(str);
        using var right = new ByteVector(str);

        Assert.True(left.Equals(right));
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}
