import type { HotelOptionDto } from "../../../types/dto/hotel.dto";
import Hotel from "./Hotel";

interface HotelListProps {
    hotels: HotelOptionDto[];
}

const HotelList = ({ hotels }: HotelListProps) => {
    return (
        <div>
            <h2>Available Hotels</h2>
            {hotels.length === 0 ? (
                <p>No hotels available</p>
            ) : (
                <div>
                    {hotels.map((hotel, index) => (
                        <Hotel key={index} hotel={hotel} />
                    ))}
                </div>
            )}
        </div>
    );
}

export default HotelList;