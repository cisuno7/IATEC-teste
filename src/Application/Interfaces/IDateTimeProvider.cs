namespace AgendaManager.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime ToUtc(DateTime dateTime);
    DateTime? ToUtc(DateTime? dateTime);
    DateTime ToUtcDateOnly(DateTime dateTime);
    DateTime ToUtcDateOnly(DateTime? dateTime);
    DateTime UtcNow { get; }
}
