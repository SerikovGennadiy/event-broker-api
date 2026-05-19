namespace Contracts.Service;

[Obsolete("Сервис менеджер по для API пока излишний. Прямой композиции API c их сервисами на данном этапе вполне достаточно")]
public interface IServiceManager
{
    IEventService EventService { get; }
    IBookingService BookingService { get; }
}

