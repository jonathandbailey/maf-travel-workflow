import { useEffect } from "react";
import type { Exchange } from "../domain/Exchange";
import streamingService from "../../../app/api/streaming.api";
import { UIFactory } from "../factories/UIFactory";
import type { ChatResponseDto } from "../api/chat.dto";
import { useExchangesStore } from "../stores/exchanges.store";


interface UseChatResponseHandlerProps {
    setActiveExchange: React.Dispatch<React.SetStateAction<Exchange | null>>;
}

export const useChatResponseHandler = ({ setActiveExchange }: UseChatResponseHandlerProps) => {
    const { exchanges, addExchange } = useExchangesStore();

    useEffect(() => {
        const handleUserResponse = (response: ChatResponseDto) => {
            if (!response) return;

            let updatedExchange: Exchange | null = null;

            setActiveExchange(prev => {
                if (prev && prev.assistant.id === response.id) {
                    const updatedAssistant = UIFactory.updateAssistantMessage(
                        prev.assistant,
                        prev.assistant.text + (response.message || ''),
                        false
                    );
                    updatedExchange = {
                        ...prev,
                        assistant: updatedAssistant
                    };



                    return updatedExchange;
                }
                return prev;
            });


        };

        streamingService.on("user", handleUserResponse);

        return () => {
            streamingService.off("user", handleUserResponse);
        };
    }, [setActiveExchange, exchanges, addExchange]);
};