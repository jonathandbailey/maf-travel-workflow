import { useEffect } from "react";
import type { ChatResponseDto } from "../types/dto/chat-response.dto";
import type { Status } from "../types/ui/Status";
import streamingService from "../services/streaming.service";

interface UseStatusUpdateHandlerProps {
    setStatusItems: React.Dispatch<React.SetStateAction<Status[]>>;
    setActiveStatus: React.Dispatch<React.SetStateAction<Status | null>>;
}

export const useStatusUpdateHandler = ({ setStatusItems, setActiveStatus }: UseStatusUpdateHandlerProps) => {
    useEffect(() => {
        const handleStatusUpdate = (response: ChatResponseDto) => {
            if (!response) return;

            const newStatus = { message: response.message || '' };

            setStatusItems(prev => {
                const newItems = [
                    ...prev,
                    newStatus
                ];
                return newItems;
            });

            // Update the active status to the most recent one
            setActiveStatus(newStatus);
        };

        streamingService.on("status", handleStatusUpdate);

        return () => {
            streamingService.off("status", handleStatusUpdate);
        };
    }, [setStatusItems, setActiveStatus]);
};