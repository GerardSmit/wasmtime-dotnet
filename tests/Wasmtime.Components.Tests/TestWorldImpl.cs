using TestWorld.wit.imports.tests.component.v0_1_0;
using static TestWorld.wit.imports.tests.common.v0_1_0.ITypes;

namespace TestWorld;

public sealed class TestWorldImpl : ITestWorld
{
    public static Dictionary<int, ITypes.Entity> Entities { get; } = new();

    public static byte AddU8(byte x, byte y) => (byte)(x + y);
    public static sbyte AddS8(sbyte x, sbyte y) => (sbyte)(x + y);

    public static ushort AddU16(ushort x, ushort y) => (ushort)(x + y);
    public static short AddS16(short x, short y) => (short)(x + y);

    public static uint AddU32(uint x, uint y) => x + y;
    public static int AddS32(int x, int y) => x + y;

    public static ulong AddU64(ulong x, ulong y) => x + y;
    public static long AddS64(long x, long y) => x + y;

    public static double AddF64(double x, double y) => x + y;

    public static float AddF32(float x, float y) => x + y;

    public static Point AddPoint(Point p1, Point p2) => new(p1.x + p2.x, p1.y + p2.y);

    public static void RegisterEntity(ITypes.Entity e) => Entities[e.id] = e;
    public static ITypes.Entity GetEntity(int id) => Entities.TryGetValue(id, out var entity) ? entity : new ITypes.Entity(-1, "", new Point(0, 0));

    public static string Uppercase(string input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        return input.ToUpperInvariant();
    }

    private static bool _flag;

    public static void SetFlag(bool flag)
    {
        _flag = flag;
    }

    public static bool GetFlag()
    {
        return _flag;
    }

    public static void HostCallback()
    {
        exports.TestWorld.Callback();
    }

    public static string HostCombineString(string s1, string s2)
    {
        return exports.TestWorld.CallbackCombineString(s1, s2);
    }
}

