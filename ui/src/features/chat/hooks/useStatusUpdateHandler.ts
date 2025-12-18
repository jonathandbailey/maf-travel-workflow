import { useEffect } from "react";
import type { Status } from "../domain/Status";
import type { Exchange } from "../domain/Exchange";
import streamingService from "../../../app/api/streaming.api";
import type { ChatResponseDto } from "../api/chat.dto";

interface UseStatusUpdateHandlerProps {
    setActiveStatus: React.Dispatch<React.SetStateAction<Status | null>>;
    setActiveExchange: React.Dispatch<React.SetStateAction<Exchange | null>>;
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