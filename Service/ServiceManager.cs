using AutoMapper;
using Contracts.Repository;
using Contracts.Service;

namespace Service;

public class ServiceManager : IServiceManager
{
    private IEventService _evenService;
    public ServiceManager(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _evenService = new EventService(repositoryManager, mapper);
    }
    public IEventService EventService => _evenService;

}
