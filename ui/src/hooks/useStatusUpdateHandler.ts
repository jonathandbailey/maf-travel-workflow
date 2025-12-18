import { useEffect } from "react";
import type { ChatResponseDto } from "../types/dto/chat-response.dto";
import type { Status } from "../types/ui/Status";
import type { UIExchange } from "../types/ui/UIExchange";
import streamingService from "../services/streaming.service";

interface UseStatusUpdateHandlerProps {
    setActiveStatus: React.Dispatch<React.SetStateAction<Status | null>>;
    setActiveExchange: React.Dispatch<React.SetStateAction<UIExchange | null>>;
}

export const useStatusUpdateHandler = ({ setActiveStatus, setActiveExchange }: UseStatusUpdateHandlerProps) => {
    useEffect(() => {
        const handleStatusUpdate = (response: ChatResponseDto) => {
            if (!response) return;

            const newStatus: Status = {
                message: response.message || '',
                details: response.details || '',
                source: response.source || ''
            };

            setActiveStatus(newStatus);

            setActiveExchange(prev => {
                if (!prev) return prev;

                return {
                    ...prev,
                    status: [
                        ...prev.status,
                        newStatus
                    ]
                };
            });
        };

        streamingService.on("status", handleStatusUpdate);

        return () => {
            streamingService.off("status", handleStatusUpdate);
        };
    }, [setActiveStatus, setActiveExchange]);
};