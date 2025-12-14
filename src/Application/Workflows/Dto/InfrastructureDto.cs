using Application.Models;

namespace Application.Workflows.Dto;

public class CreateFlightOptions(TravelPlan travelPlan, ReasoningOutputDto message)
{
    public ReasoningOutputDto Message { get; } = message;
    public TravelPlan TravelPlan => travelPlan;
}

public class FlightOptionsCreated(FlightOptionsStatus flightOptionsStatus, UserFlightOptionsStatus userFlightOptionStatus)
{
    public FlightOptionsStatus FlightOptionsStatus { get;  } = flightOptionsStatus;

    public UserFlightOptionsStatus UserFlightOptionStatus { get; } = userFlightOptionStatus;
}

public class CreatePlanRequestDto(TravelPlan travelPlan)
{
    public TravelPlan TravelPlan => travelPlan;
}

public class ArtifactStorageDto(string key, string content)
{
    public string Key { get; } = key;

    public string Content { get; } = content;
}

public class FlightActionResultDto
{
    public FlightSearchResultDto FlightOptions { get; set; }
    public string Action { get; set; }

    public string Status { get; set; }
}


public class FlightSearchResultDto
{
    public string ArtifactKey { get; set; }
    public List<FlightOptionDto> Results { get; set; }
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
