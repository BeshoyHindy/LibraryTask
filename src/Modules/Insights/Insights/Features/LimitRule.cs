using FluentValidation;

namespace Insights.Features;

// Every ranking shares one rule for how many rows the caller may ask for: absent or 0 takes the
// default, 1..100 is the caller's own, anything else is their mistake.
internal static class LimitRule
{
    private const int Default = 10;

    public static IRuleBuilderOptions<T, int?> IsALimit<T>(this IRuleBuilder<T, int?> rule) =>
        rule.Must(limit => limit is null or (>= 0 and <= 100))
            .WithMessage($"must be between 1 and 100; 0 or absent takes the default of {Default}.");

    public static int OrDefault(int? limit) => limit is null or 0 ? Default : limit.Value;
}
