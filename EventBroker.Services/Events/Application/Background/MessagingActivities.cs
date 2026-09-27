using System.Diagnostics;

namespace Events.Application.Background;

/// <summary>Источники активностей для ручной инструментации Kafka-саг.</summary>
/// <remarks>
/// OTel SDK подписывается на источник через AddSource (см. ConfigureObservability),
/// иначе созданные вручную сп publish/consume не попадут в Jaeger.
/// </remarks>
public static class MessagingActivities
{
    public const string SourceName = "Events.Messaging";

    public static readonly ActivitySource Source = new(SourceName);
}
