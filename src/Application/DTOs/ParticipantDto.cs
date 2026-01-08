namespace AgendaManager.Application.DTOs;

public class ParticipantDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}
