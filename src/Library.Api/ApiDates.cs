using Date = Google.Type.Date;

namespace Library.Api;

// The JSON surface is DateOnly; google.type.Date is the wire shape. A date the service sends back
// is always a whole calendar day, so only the outbound direction has an absent case.
internal static class ApiDates
{
    public static Date ToDate(DateOnly date) => new() { Year = date.Year, Month = date.Month, Day = date.Day };

    // An absent bound stays absent on the wire; whether that is allowed is the service's rule.
    public static Date? ToDateOrNull(DateOnly? date) => date is { } value ? ToDate(value) : null;

    public static DateOnly ToDateOnly(Date date) => new(date.Year, date.Month, date.Day);
}
