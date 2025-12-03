import apiClient from "../api/client/api-client";
import type { ChatRequestDto } from "../types/dto/chat-request.dto";
import type { ChatResponseDto } from "../types/dto/chat-response.dto";
import type { FlightSearchResultDto } from "../types/dto/flight.dto";
import type { HotelSearchResultDto } from "../types/dto/hotel.dto";


export class ConversationService {


    async startConversationExchange(message: string, id: string, sessionId: string, exchangeId: string): Promise<ChatResponseDto> {


        const request: ChatRequestDto = { message, id, sessionId, exchangeId };

        const response = await apiClient.post<ChatResponseDto>(`api/conversations`, request);
        return response.data;
    }

    async getFlightPlan(sessionId: string): Promise<FlightSearchResultDto> {

        const response = await apiClient.get<FlightSearchResultDto>(`api/plans/${sessionId}/flights`);
        return response.data;
    }

    async getHotelPlan(sessionId: string): Promise<HotelSearchResultDto> {

        const response = await apiClient.get<HotelSearchResultDto>(`api/plans/${sessionId}/hotels`);
        return response.data;
    }
}