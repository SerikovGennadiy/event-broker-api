namespace Contracts;

internal interface IRepositoryManager
{
    IEventRepository Event { get; }
    void Save();
}
