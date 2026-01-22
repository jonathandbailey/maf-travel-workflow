using Travel.Workflows.Models;

namespace Travel.Workflows.Dto;


public class ReasoningOutputDto
{
    public string Thought { get; set; } = string.Empty;

    public NextAction NextAction { get; set; } 

    public string Status { get; set; } = string.Empty;
  

    public TravelPlanUpdateDto? TravelPlanUpdate { get; set; }

    public UserInputRequestDto? UserInput { get; set; }

    public void Error()
    {

    }
}

public enum NextAction
{
    RequestInformation,
    FlightAgent,
    Error
}

public class StartWorkflowDto(Guid threadId, ReasoningInputDto reasoningInputDto)
{
    public Guid ThreadId { get; } = threadId;
    public ReasoningInputDto ReasoningInputDto { get; } = reasoningInputDto;
}

public class ReasoningInputDto(string observation)
{
    public string Observation { get; } = observation;
}

public class ReasoningState(string observation, TravelPlanSummary travelPlanSummary)
{
    public string Observation { get; } = observation;
    public TravelPlanSummary TravelPlanSummary { get; } = travelPlanSummary;
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

