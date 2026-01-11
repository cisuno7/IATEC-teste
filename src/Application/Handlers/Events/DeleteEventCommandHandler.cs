using AgendaManager.Application.Commands.Events;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Events;

public class DeleteEventCommandHandler : BaseHandler, ICommandHandler<DeleteEventCommand, bool>
{
    public DeleteEventCommandHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }

    public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);
        if (eventEntity is null)
            throw new InvalidOperationException("Event not found");

        if (!await _unitOfWork.Events.CanUserEditEventAsync(request.EventId, request.UserId))
            throw new UnauthorizedAccessException("User cannot delete this event");

        await _unitOfWork.Events.RemoveAsync(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
