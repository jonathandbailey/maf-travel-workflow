import type { TravelPlan, FlightPlan, FlightOption, FlightEndpoint, Price } from '../domain/TravelPlan';
import type { TravelPlanDto, FlightPlanDto, FlightOptionDto, FlightEndpointDto, PriceDto } from '../api/travel.dto';

export function mapTravelPlanDtoToDomain(dto: TravelPlanDto, sessionId?: string): TravelPlan {
    return {
        id: sessionId || dto.id || '',
        destination: dto.destination,
        startDate: new Date(dto.startDate),
        endDate: new Date(dto.endDate),
        origin: dto.origin,
        flightPlan: dto.flightPlan ? mapFlightPlanDtoToDomain(dto.flightPlan) : { flightOption: null },
    };
}

export function mapTravelPlanToDto(domain: TravelPlan): TravelPlanDto {
    return {
        id: domain.id,
        destination: domain.destination,
        startDate: domain.startDate.toISOString(),
        endDate: domain.endDate.toISOString(),
        origin: domain.origin,
        flightPlan: domain.flightPlan ? mapFlightPlanToDto(domain.flightPlan) : undefined,
    };
}

function mapFlightPlanDtoToDomain(dto: FlightPlanDto): FlightPlan {
    return {
        flightOption: dto.flightOption ? mapFlightOptionDtoToDomain(dto.flightOption) : null,
    };
}

function mapFlightPlanToDto(domain: FlightPlan): FlightPlanDto {
    return {
        flightOption: domain.flightOption ? mapFlightOptionToDto(domain.flightOption) : null,
    };
}

function mapFlightOptionDtoToDomain(dto: FlightOptionDto): FlightOption {
    return {
        airline: dto.airline,
        flightNumber: dto.flightNumber,
        departure: mapFlightEndpointDtoToDomain(dto.departure),
        arrival: mapFlightEndpointDtoToDomain(dto.arrival),
        duration: dto.duration,
        price: mapPriceDtoToDomain(dto.price),
    };
}

function mapFlightOptionToDto(domain: FlightOption): FlightOptionDto {
    return {
        airline: domain.airline,
        flightNumber: domain.flightNumber,
        departure: mapFlightEndpointToDto(domain.departure),
        arrival: mapFlightEndpointToDto(domain.arrival),
        duration: domain.duration,
        price: mapPriceToDto(domain.price),
    };
}

function mapFlightEndpointDtoToDomain(dto: FlightEndpointDto): FlightEndpoint {
    return {
        airport: dto.airport,
        airportCode: dto.airportCode,
        datetime: new Date(dto.datetime),
    };
}

function mapFlightEndpointToDto(domain: FlightEndpoint): FlightEndpointDto {
    return {
        airport: domain.airport,
        airportCode: domain.airportCode,
        datetime: domain.datetime.toISOString(),
    };
}

function mapPriceDtoToDomain(dto: PriceDto): Price {
    return {
        amount: dto.amount,
        currency: dto.currency,
    };
}

function mapPriceToDto(domain: Price): PriceDto {
    return {
        amount: domain.amount,
        currency: domain.currency,
    };
}