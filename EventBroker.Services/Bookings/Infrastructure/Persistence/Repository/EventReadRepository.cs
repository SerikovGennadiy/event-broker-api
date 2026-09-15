using AutoMapper;
using Bookings.Application.Common.DTO;
using Bookings.Application.Contracts.Persistence;
using Bookings.Infrastructure.Persistence.Messaging.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence.Repository;

public class EventReadRepository : RepositoryBase<EventRead>, IEventReadRepository
{
    private readonly IMapper _mapper;

    public EventReadRepository(AppDbContext context, IMapper mapper) : base(context)
    {
        _mapper = mapper;
    }
    public async Task AddAsync(EventReadDTO eventReadDTO, CancellationToken cancellationToken = default)
    {
        var entity = await FindByCondition(x => x.Id == eventReadDTO.Id).FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
        {
            _mapper.Map(eventReadDTO, entity);
            Update(entity);
        }
        else
        {
            entity = _mapper.Map<EventRead>(eventReadDTO);
            Create(entity);
        }
    }

    public async Task<EventReadDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByCondition(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return _mapper.Map<EventReadDTO>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByCondition(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
        {
            Delete(entity);
        }
    }

}
