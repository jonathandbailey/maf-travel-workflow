using System.Text.Json.Serialization;

namespace Travel.Application.Api.Models.Flights;

public class FlightPlan
{
    [JsonInclude]
    public FlightOptionsStatus FlightOptionsStatus { get; private set; } = FlightOptionsStatus.NotCreated;

    [JsonInclude]
    public UserFlightOptionsStatus UserFlightOptionStatus { get; private set; } = UserFlightOptionsStatus.NotSelected;

    [JsonInclude]
    public List<FlightOptionSearch> FlightOptions { get; private set; } = [];

    [JsonInclude]
    public FlightOption? FlightOption { get; private set; }

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