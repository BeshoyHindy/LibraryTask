namespace Library.Warmup;

public static class BookIds
{
    public static bool IsPowerOfTwo(int id) => id > 0 && (id & (id - 1)) == 0;

    public static IEnumerable<int> OddIds()
    {
        for (var id = 1; id < 100; id += 2)
        {
            yield return id;
        }
    }

    public static void PrintOddIds(TextWriter writer)
    {
        foreach (var id in OddIds())
        {
            writer.WriteLine(id);
        }
    }
}
