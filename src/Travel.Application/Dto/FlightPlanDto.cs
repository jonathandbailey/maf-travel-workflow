namespace Travel.Application.Api.Dto;

public class FlightPlanDto
{
    public FlightOptionDto FlightOption { get; set; }
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

public class FlightSearchResponseDto(Guid id)
{
    public Guid Id { get; private set; } = id;
}

public class FlightSearchDto
{
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTimeOffset DepartureDate { get; set; }
    public DateTimeOffset ReturnDate { get; set; }
}

public class FlightSearchResultDto
{
    public Guid Id { get; set; }
    
    public string ArtifactKey { get; set; }

    public List<FlightOptionDto> DepartureFlightOptions { get; set; }

    public List<FlightOptionDto> ReturnFlightOptions { get; set; }
}

public class FlightSearchRequestDto
{
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTimeOffset DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}
