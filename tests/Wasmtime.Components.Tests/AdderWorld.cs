namespace TestWorld;

public sealed class TestWorldImpl : ITestWorld
{
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
}

