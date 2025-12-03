import type { HotelOptionDto } from "../../../types/dto/hotel.dto";

interface HotelProps {
    hotel: HotelOptionDto;
}

const Hotel = ({ hotel }: HotelProps) => {
    return (
        <div>
            <h3>{hotel.hotelName}</h3>
            <p>Address: {hotel.address}</p>
            <p>Check-in: {hotel.checkIn}</p>
            <p>Check-out: {hotel.checkOut}</p>
            <p>Rating: {hotel.rating} stars</p>
            <p>Price per night: {hotel.pricePerNight.amount} {hotel.pricePerNight.currency}</p>
            <p>Total price: {hotel.totalPrice.amount} {hotel.totalPrice.currency}</p>
        </div>
    );
}

export default Hotel;