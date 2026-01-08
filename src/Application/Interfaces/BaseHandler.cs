using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Interfaces;

public abstract class BaseHandler
{
    protected readonly IUnitOfWork _unitOfWork;

    protected BaseHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }
}
