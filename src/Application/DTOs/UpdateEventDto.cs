using AgendaManager.Domain.Enums;

namespace AgendaManager.Application.DTOs;

public class UpdateEventDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime Date { get; set; }
    public required string Location { get; set; }
    public Domain.Enums.EventType Type { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}
