import { useEffect } from "react";
import type { ChatResponseDto } from "../types/dto/chat-response.dto";
import type { UIExchange } from "../types/ui/UIExchange";
import { UIFactory } from "../factories/UIFactory";
import streamingService from "../services/streaming.service";

interface UseChatResponseHandlerProps {
    setExchanges: React.Dispatch<React.SetStateAction<UIExchange[]>>;
}

export const useChatResponseHandler = ({ setExchanges }: UseChatResponseHandlerProps) => {
    useEffect(() => {
        const handleUserResponse = (response: ChatResponseDto) => {
            if (!response) return;

            setExchanges(prev => prev.map(exchange => {
                if (exchange.assistant.id === response.id) {
                    const updatedAssistant = UIFactory.updateAssistantMessage(
                        exchange.assistant,
                        exchange.assistant.text + (response.message || ''),
                        false
                    );
                    return {
                        ...exchange,
                        assistant: updatedAssistant
                    };
                }
                return exchange;
            }));
        };

        streamingService.on("user", handleUserResponse);

        return () => {
            streamingService.off("user", handleUserResponse);
        };
    }, [setExchanges]);
};