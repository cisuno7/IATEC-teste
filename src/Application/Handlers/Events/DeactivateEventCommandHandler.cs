using AgendaManager.Application.Commands.Events;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Events;

public class DeactivateEventCommandHandler : BaseHandler, ICommandHandler<DeactivateEventCommand, bool>
{
    public DeactivateEventCommandHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }

    public async Task<bool> Handle(DeactivateEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);
        if (eventEntity is null)
            throw new InvalidOperationException("Event not found");

        if (!await _unitOfWork.Events.CanUserEditEventAsync(request.EventId, request.UserId))
            throw new UnauthorizedAccessException("User cannot edit this event");

        eventEntity.Deactivate();

        await _unitOfWork.Events.UpdateAsync(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
