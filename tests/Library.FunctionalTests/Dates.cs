using Google.Type;

namespace Library.FunctionalTests;

internal static class Dates
{
    public static Date On(int year, int month, int day) => new() { Year = year, Month = month, Day = day };
}
