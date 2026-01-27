using Travel.Application.Api.Infrastructure.Seed;
using Travel.Application.Domain.Flights;

namespace Travel.Application.Services;

public class FlightSearchService : IFlightSearchService
{
    public async Task<FlightSearch> SearchFlights(string origin, string destination, DateTimeOffset departureDate, DateTimeOffset returnDate)
    {
        var rng = new Random();
        
        var originAirport = DataRegistry.EuropeanCities.FirstOrDefault(c => c.City == origin);
        var destinationAirport = DataRegistry.EuropeanCities.FirstOrDefault(c => c.City == destination);

        if (originAirport == null || destinationAirport == null)
        {
            return new FlightSearch(new List<FlightOption>(), new List<FlightOption>());
        }

        var departureFlights = new List<FlightOption>();
        var numberOfDepartureFlights = 3;
        
        for (int i = 0; i < numberOfDepartureFlights; i++)
        {
            var airline = DataRegistry.EuropeanAirlines[rng.Next(DataRegistry.EuropeanAirlines.Count)];
            var departureTime = departureDate.AddHours(rng.Next(6, 22));
            var flightDuration = TimeSpan.FromMinutes(rng.Next(60, 300)); 
            
            var flightOption = new FlightOption
            {
                Airline = airline.Name,
                FlightNumber = GenerateFlightNumber(airline.Code),
                Departure = new FlightEndpoint
                {
                    Airport = originAirport.AirportName,
                    AirportCode = originAirport.AirportCode,
                    Datetime = departureTime.DateTime
                },
                Arrival = new FlightEndpoint
                {
                    Airport = destinationAirport.AirportName,
                    AirportCode = destinationAirport.AirportCode,
                    Datetime = departureTime.Add(flightDuration).DateTime
                },
                Duration = FormatDuration(flightDuration),
                Price = new FlightPrice
                {
                    Amount = airline.IsLowCost 
                        ? rng.Next(50, 150) 
                        : rng.Next(150, 500),
                    Currency = "EUR"
                }
            };
            
            departureFlights.Add(flightOption);
        }
        
        var returnFlights = new List<FlightOption>();
        var numberOfReturnFlights = 3;
        
        for (int i = 0; i < numberOfReturnFlights; i++)
        {
            var airline = DataRegistry.EuropeanAirlines[rng.Next(DataRegistry.EuropeanAirlines.Count)];
            var returnDepartureTime = returnDate.AddHours(rng.Next(6, 22));
            var flightDuration = TimeSpan.FromMinutes(rng.Next(60, 300)); 
            
            var flightOption = new FlightOption
            {
                Airline = airline.Name,
                FlightNumber = GenerateFlightNumber(airline.Code),
                Departure = new FlightEndpoint
                {
                    Airport = destinationAirport.AirportCode,
                    Datetime = returnDepartureTime.DateTime
                },
                Arrival = new FlightEndpoint
                {
                    Airport = originAirport.AirportCode,
                    Datetime = returnDepartureTime.Add(flightDuration).DateTime
                },
                Duration = FormatDuration(flightDuration),
                Price = new FlightPrice
                {
                    Amount = airline.IsLowCost 
                        ? rng.Next(50, 150) 
                        : rng.Next(150, 500),
                    Currency = "EUR"
                }
            };
            
            returnFlights.Add(flightOption);
        }
        
        departureFlights = departureFlights.OrderBy(f => f.Price.Amount).ToList();
        returnFlights = returnFlights.OrderBy(f => f.Price.Amount).ToList();
        
        return await Task.FromResult(new FlightSearch(departureFlights, returnFlights));
    }

    private string GenerateFlightNumber(string code)
    {
        var rng = new Random();

        return $"{code}-{rng.Next(100, 9999)}";
    }
    
    private string FormatDuration(TimeSpan duration)
    {
        var hours = (int)duration.TotalHours;
        var minutes = duration.Minutes;
        return $"{hours}h {minutes}m";
    }

    
}

public interface IFlightSearchService
{
    Task<FlightSearch> SearchFlights(string origin, string destination, DateTimeOffset departureDate, DateTimeOffset returnDate);
}