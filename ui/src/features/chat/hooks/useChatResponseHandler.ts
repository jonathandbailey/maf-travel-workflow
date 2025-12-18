import { useEffect } from "react";
import type { Exchange } from "../domain/Exchange";
import streamingService from "../../../app/api/streaming.api";
import { UIFactory } from "../factories/UIFactory";
import type { ChatResponseDto } from "../api/chat.dto";


interface UseChatResponseHandlerProps {
    setActiveExchange: React.Dispatch<React.SetStateAction<Exchange | null>>;
    setExchanges: React.Dispatch<React.SetStateAction<Exchange[]>>;
}

export const useChatResponseHandler = ({ setActiveExchange, setExchanges }: UseChatResponseHandlerProps) => {
    useEffect(() => {
        const handleUserResponse = (response: ChatResponseDto) => {
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