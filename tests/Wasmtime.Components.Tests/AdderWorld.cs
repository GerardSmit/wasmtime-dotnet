namespace TestWorld;

public class TestWorldImpl
{
    public static uint AddU32(uint x, uint y)
    {
        return x + y;
    }

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

