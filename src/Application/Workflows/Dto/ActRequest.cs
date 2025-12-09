namespace Application.Workflows.Dto;

public class ActRequest
{
    public string Thought { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public TravelPlanUpdateDto? TravelPlanUpdate { get; set; }

    public UserInputRequestDto? UserInput { get; set; }
}

public class TravelPlanUpdateDto()
{
    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}

public class UserInputRequestDto()
{
    public string Question { get; set; } = string.Empty;

    public List<string> RequiredInputs { get; set; } = new List<string>();
}