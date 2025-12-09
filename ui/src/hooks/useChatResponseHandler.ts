import { useEffect } from "react";
import type { ChatResponseDto } from "../types/dto/chat-response.dto";
import type { UIExchange } from "../types/ui/UIExchange";
import { UIFactory } from "../factories/UIFactory";
import streamingService from "../services/streaming.service";

interface UseChatResponseHandlerProps {
    setActiveExchange: React.Dispatch<React.SetStateAction<UIExchange | null>>;
    setExchanges: React.Dispatch<React.SetStateAction<UIExchange[]>>;
}

export const useChatResponseHandler = ({ setActiveExchange, setExchanges }: UseChatResponseHandlerProps) => {
    useEffect(() => {
        const handleUserResponse = (response: ChatResponseDto) => {
            console.log('Chat response received:', response);
            if (!response) return;

            // Find the matching exchange and update active exchange
            setActiveExchange(prev => {
                if (prev && prev.assistant.id === response.id) {
                    const updatedAssistant = UIFactory.updateAssistantMessage(
                        prev.assistant,
                        prev.assistant.text + (response.message || ''),
                        false
                    );
                    const updatedExchange = {
                        ...prev,
                        assistant: updatedAssistant
                    };

                    // Check if it's the end of stream
                    if (response.isEndOfStream) {
                        // Add the exchange to the list of exchanges if it doesn't already exist
                        console.log('Finalizing exchange:', updatedExchange);
                        setExchanges(prevExchanges => {
                            const existingIndex = prevExchanges.findIndex(ex => ex.assistant.id === updatedExchange.assistant.id);
                            if (existingIndex >= 0) {
                                // Update existing exchange
                                const newExchanges = [...prevExchanges];
                                newExchanges[existingIndex] = updatedExchange;
                                return newExchanges;
                            } else {
                                // Add new exchange
                                return [...prevExchanges, updatedExchange];
                            }
                        });
                    }

                    return updatedExchange;
                }
                return prev;
            });


        };

        streamingService.on("user", handleUserResponse);

        return () => {
            streamingService.off("user", handleUserResponse);
        };
    }, [setActiveExchange, setExchanges]);
};