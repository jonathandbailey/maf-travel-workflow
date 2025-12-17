using Application.Models;

namespace Application.Workflows.Dto;

public class UserParsedDto
{
    public string UserMessage { get; set; }

    public string UserIntent { get; set; }
    
    public TravelPlanUpdateDto? IntentInputs { get; set; }
}

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

public class ReasoningInputDto(string observation, string resultType)
{
    public string Observation { get; } = observation;

    public string ResultType { get; } = resultType;
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

