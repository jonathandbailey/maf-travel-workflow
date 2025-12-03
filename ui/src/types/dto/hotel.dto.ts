export interface HotelSearchResultDto {
    artifactKey: string;
    results: HotelOptionDto[];
}

export interface HotelOptionDto {
    hotelName: string;
    address: string;
    checkIn: string;
    checkOut: string;
    rating: number;
    pricePerNight: HotelPriceDto;
    totalPrice: HotelPriceDto;
}

export interface HotelPriceDto {
    amount: number;
    currency: string;
}