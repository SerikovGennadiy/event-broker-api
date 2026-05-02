using System.Text.Json.Serialization;

namespace Shared.RequestSpecification;

public class EventParameters : Parameters
{
    public EventParameters() => OrderBy = "title";

    public string? Title { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public bool ValidatePeriod { get; set; } = false;

    [JsonIgnore]
    public bool CheckPeriod =>
        ValidatePeriod && From.HasValue && (!To.HasValue || From <= To);
}
