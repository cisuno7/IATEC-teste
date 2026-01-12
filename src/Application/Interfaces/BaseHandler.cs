using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Interfaces;

public abstract class BaseHandler
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IDateTimeProvider _dateTimeProvider;

    protected BaseHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }
}
