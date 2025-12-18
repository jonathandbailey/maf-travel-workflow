import apiClient from "../../../app/api/api-client";
import type { ChatRequestDto, ChatResponseDto } from "./chat.dto";


export class ChatService {


    async startChatExchange(message: string, id: string, sessionId: string, exchangeId: string): Promise<ChatResponseDto> {


        const request: ChatRequestDto = { message, id, sessionId, exchangeId };

        const response = await apiClient.post<ChatResponseDto>(`api/conversations`, request);
        return response.data;
    }


}