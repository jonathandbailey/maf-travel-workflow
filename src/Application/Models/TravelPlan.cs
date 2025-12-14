namespace Application.Models;

public class TravelPlan
{
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public FlightOptionsStatus FlightOptionsStatus { get; set; } = FlightOptionsStatus.NotCreated;

    public UserFlightOptionsStatus UserFlightOptionStatus { get; set; } = UserFlightOptionsStatus.NotSelected;
}

public class TravelPlanSummary(TravelPlan plan)
{
    public string Origin { get; set; } = !string.IsNullOrEmpty(plan.Origin) ? plan.Origin : TravelPlanSummaryConstants.NotSet;

    public string Destination { get; set; } = !string.IsNullOrEmpty(plan.Destination) ? plan.Destination : TravelPlanSummaryConstants.NotSet;

    public string StartDate { get; set; } = plan.StartDate?.ToString("yyyy-MM-dd") ?? TravelPlanSummaryConstants.NotSet;

    public string EndDate { get; set; } = plan.EndDate?.ToString("yyyy-MM-dd") ?? TravelPlanSummaryConstants.NotSet;

    public string FlightOptionStatus { get; set; } = plan.FlightOptionsStatus.ToString();

    public string UserFlightOptionStatus { get; set; } = plan.UserFlightOptionStatus.ToString();
}

public static class TravelPlanSummaryConstants
{
    public const string NotSet = "Not_Set";
}

public enum FlightOptionsStatus
{
    Created,
    NotCreated
}

public enum UserFlightOptionsStatus
{
    Selected,
    UserChoiceRequired,
    NotSelected
}

