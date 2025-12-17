namespace Application.Models.Flights;

public class FlightPlan
{
    public FlightOptionsStatus FlightOptionsStatus { get; set; } = FlightOptionsStatus.NotCreated;

    public UserFlightOptionsStatus UserFlightOptionStatus { get; set; } = UserFlightOptionsStatus.NotSelected;

    public List<FlightOptionSearch> FlightOptions { get; set; } = [];

    public FlightOption? FlightOption { get; set; }

    public void SelectFlightOption(FlightOption flightOption)
    {
        FlightOption = flightOption;
        UserFlightOptionStatus = UserFlightOptionsStatus.Selected;
    }

    public void AddFlightOptions(FlightOptionSearch flightOptions)
    {
        FlightOptions.Add(flightOptions);

        FlightOptionsStatus = FlightOptionsStatus.Created;
        UserFlightOptionStatus = UserFlightOptionsStatus.UserChoiceRequired;
    }
}