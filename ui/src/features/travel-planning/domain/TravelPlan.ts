export interface TravelPlan {
    id: string;
    destination: string;
    startDate: Date | null;
    endDate: Date | null;
    origin: string;
    flightPlan?: FlightPlan;
}

export interface FlightPlan {
    flightOption?: FlightOption | null;
}

export interface FlightOption {
    airline: string;
    flightNumber: string;
    departure: FlightEndpoint;
    arrival: FlightEndpoint;
    duration: string;
    price: Price;
}

export interface FlightEndpoint {
    airport: string;
    airportCode: string;
    datetime: Date | null;
}

export interface Price {
    amount: number;
    currency: string;
}