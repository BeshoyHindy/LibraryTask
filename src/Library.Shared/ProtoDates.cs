using Date = Google.Type.Date;

namespace Library.Shared;

// google.type.Date allows a zero year, month or day for partial dates; every date on this wire is
// a whole calendar day. An absent date arrives as null.
public static class ProtoDates
{
    public static Result<DateOnly> Required(Date? date, string field) =>
        date is not null && IsCalendarDate(date)
            ? new DateOnly(date.Year, date.Month, date.Day)
            : Error.Validation([new FieldError(field, "must be a date with a year, a month and a day.")]);

    public static Result<DateOnly?> Optional(Date? date, string field)
    {
        if (date is null)
        {
            return Result<DateOnly?>.Success(null);
        }

        var required = Required(date, field);

        return required.Error is { } error
            ? Result<DateOnly?>.Failure(error)
            : Result<DateOnly?>.Success(required.Value);
    }

    public static Date ToDate(DateOnly date) => new() { Year = date.Year, Month = date.Month, Day = date.Day };

    private static bool IsCalendarDate(Date date) =>
        date.Year is >= 1 and <= 9999
        && date.Month is >= 1 and <= 12
        && date.Day >= 1
        && date.Day <= DateTime.DaysInMonth(date.Year, date.Month);
}
