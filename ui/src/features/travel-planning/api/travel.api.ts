import apiClient from "../../../app/api/api-client";
import type { FlightSearchResultDto, HotelSearchResultDto, TravelPlanDto } from "./travel.dto";

export class TravelService {

    async getFlightPlan(sessionId: string): Promise<FlightSearchResultDto> {

        const response = await apiClient.get<FlightSearchResultDto>(`api/plans/${sessionId}/flights`);
        return response.data;
    }

    async getHotelPlan(sessionId: string): Promise<HotelSearchResultDto> {

        const response = await apiClient.get<HotelSearchResultDto>(`api/plans/${sessionId}/hotels`);
        return response.data;
    }

    async getTravelPlan(sessionId: string): Promise<TravelPlanDto> {

        const response = await apiClient.get<TravelPlanDto>(`api/plans/${sessionId}/travel`);
        return response.data;
    }
}