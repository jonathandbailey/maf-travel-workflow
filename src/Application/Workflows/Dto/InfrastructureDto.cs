using Application.Models;
using Application.Models.Flights;

namespace Application.Workflows.Dto;

public class CreateFlightOptions(TravelPlan travelPlan, ReasoningOutputDto message)
{
    public ReasoningOutputDto Message { get; } = message;
    public TravelPlan TravelPlan => travelPlan;
}

public class FlightOptionsCreated(FlightOptionsStatus flightOptionsStatus, UserFlightOptionsStatus userFlightOptionStatus, FlightSearchResultDto flightOptions)
{
    public FlightOptionsStatus FlightOptionsStatus { get;  } = flightOptionsStatus;

    public UserFlightOptionsStatus UserFlightOptionStatus { get; } = userFlightOptionStatus;

    public FlightSearchResultDto FlightOptions { get; set; } = flightOptions;
}

public class AgentResponse(string source, string message, AgentResponseStatus status)
{
    public string Source { get; } = source;
    
    public string Message { get; } = message;

    public AgentResponseStatus Status { get; } = status;

    public override string ToString()
    {
        return $"{Source} : {Message}, Status: {Status}";
    }
}

public enum AgentResponseStatus
{
        Success,
        Error
}

public class CreatePlanRequestDto(TravelPlan travelPlan)
{
    public TravelPlan TravelPlan => travelPlan;
}

public class ArtifactStorageDto(string key, string content)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Key { get; } = key;

    public string Content { get; } = content;
}

public enum FlightAction
{
    FlightOptionsCreated,
    FlightOptionsSelected
}

public class FlightActionResultDto
{
    public FlightSearchResultDto Results { get; set; }
    public FlightAction Action { get; set; }

    public string Status { get; set; }
}


public class FlightSearchResultDto
{
    public string ArtifactKey { get; set; }
    public List<FlightOptionDto> DepartureFlightOptions { get; set; }

    public List<FlightOptionDto> ReturnFlightOptions { get; set; }
}

public class FlightOptionDto
{
    public string Airline { get; set; }
    public string FlightNumber { get; set; }
    public FlightEndpointDto Departure { get; set; }
    public FlightEndpointDto Arrival { get; set; }
    public string Duration { get; set; }
    public PriceDto Price { get; set; }
}

public class FlightEndpointDto
{
    public string Airport { get; set; }

    public string AirportCode { get; set; }
    public DateTime Datetime { get; set; }
}

public class PriceDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

public record HotelSearchResultDto
{
    public string ArtifactKey { get; init; }
    public List<HotelOptionDto> Results { get; init; }
}

public record HotelOptionDto
{
    public string HotelName { get; init; }
    public string Address { get; init; }
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public decimal Rating { get; init; }
    public HotelPriceDto PricePerNight { get; init; }
    public HotelPriceDto TotalPrice { get; init; }
}

public class HotelPriceDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}
