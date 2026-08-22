using Users.Domain.Models;

namespace Users.Application.Common.RequestSpecification;

public class EventParameters : Parameters
{
    public EventParameters() => OrderBy = "name";

    public string? Name { get; set; }
    public Role? Role { get; set; }
}
