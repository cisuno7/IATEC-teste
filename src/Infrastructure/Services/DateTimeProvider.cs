using AgendaManager.Application.Interfaces;

namespace AgendaManager.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime ToUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;
            
        if (dateTime.Kind == DateTimeKind.Local)
            return dateTime.ToUniversalTime();
            
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
    
    public DateTime? ToUtc(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return null;
            
        return ToUtc(dateTime.Value);
    }
    
    public DateTime ToUtcDateOnly(DateTime dateTime)
    {
        var utc = ToUtc(dateTime);
        return new DateTime(utc.Year, utc.Month, utc.Day, 0, 0, 0, DateTimeKind.Utc);
    }
    
    public DateTime ToUtcDateOnly(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            throw new ArgumentNullException(nameof(dateTime));
            
        return ToUtcDateOnly(dateTime.Value);
    }
}
