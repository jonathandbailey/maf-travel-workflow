namespace Application.Models.Flights;

public class FlightOption
{
    public string Airline { get; set; }
    public string FlightNumber { get; set; }
    public FlightEndpoint Departure { get; set; }
    public FlightEndpoint Arrival { get; set; }
    public string Duration { get; set; }
    public FlightPrice Price { get; set; }
}