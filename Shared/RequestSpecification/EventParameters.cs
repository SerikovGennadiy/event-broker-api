namespace Shared.RequestSpecification;

public class EventParameters : Parameters
{
    public EventParameters() => OrderBy = "title";

    public string? Title { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public bool ValidatePeriod => From < To;
}
