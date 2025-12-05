namespace Application.Workflows.ReAct.Dto;

public class ActObservation(string observation, string resultType)
{
    public string Observation { get;  } = observation;

    public string ResultType { get;  } = resultType;
}