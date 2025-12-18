
import { Card } from "antd";
import type { FlightOptionDto } from "../../../../types/dto/flight.dto";

interface FlightProps {
    flight: FlightOptionDto;
}

const Flight = ({ flight }: FlightProps) => {
    return (
        <Card
            size="small"

            style={{ boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)", width: "300px" }}
        >

            <div>
                <h3>{flight.airline} {flight.flightNumber}</h3>
                <p>From: {flight.departure.airport} at {flight.departure.datetime}</p>
                <p>To: {flight.arrival.airport} at {flight.arrival.datetime}</p>
                <p>Duration: {flight.duration}</p>
                <p>Price: {flight.price.amount} {flight.price.currency}</p>
            </div>
        </Card>

    );
}

export default Flight;