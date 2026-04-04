using Contracts.Repository;

namespace Repository
{
    internal class RepositoryManager : IRepositoryManager
    {
        private IEventRepository Events { get; }

        public RepositoryManager(RepositoryContext context)
        {
            Events = new EventRepository(context);
        }

        public IEventRepository Event => Events;

        // TODO : хранилище локальное
        public void Save() => throw new NotImplementedException();
    }
}
