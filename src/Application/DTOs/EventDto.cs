using AgendaManager.Domain.Enums;

namespace AgendaManager.Application.DTOs;
public class EventDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime Date { get; set; }
    public required string Location { get; set; }
    public Domain.Enums.EventType Type { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatorId { get; set; }
    public required string CreatorName { get; set; }
    public required List<ParticipantDto> Participants { get; set; }
}