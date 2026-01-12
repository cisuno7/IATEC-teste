using AgendaManager.Application.Commands.Events;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Events;

public class ActivateEventCommandHandler : BaseHandler, ICommandHandler<ActivateEventCommand, bool>
{
    public ActivateEventCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        : base(unitOfWork, dateTimeProvider)
    {
    }

    public async Task<bool> Handle(ActivateEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);
        if (eventEntity is null)
            throw new InvalidOperationException("Event not found");

        if (!await _unitOfWork.Events.CanUserEditEventAsync(request.EventId, request.UserId))
            throw new UnauthorizedAccessException("User cannot edit this event");

        eventEntity.Activate();

        await _unitOfWork.Events.UpdateAsync(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
