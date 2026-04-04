using Contracts.Service;

namespace Service;

internal class ServiceManager : IServiceManager
{

    public IEventService EventService => throw new NotImplementedException();

}
