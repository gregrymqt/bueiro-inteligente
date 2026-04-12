using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace backend.Infrastructure.Persistence.Converters;

internal sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value =>
                value.Kind == DateTimeKind.Utc ? value
                : value.Kind == DateTimeKind.Local ? value.ToUniversalTime()
                : DateTime.SpecifyKind(value, DateTimeKind.Utc),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        ) { }
}
