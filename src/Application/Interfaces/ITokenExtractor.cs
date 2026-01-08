namespace AgendaManager.Application.Interfaces;

public interface ITokenExtractor
{
    string? ExtractToken();
    Guid? ExtractUserId();
}

