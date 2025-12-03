import { useEffect } from "react";
import type { ChatResponseDto } from "../types/dto/chat-response.dto";
import type { Status } from "../types/ui/Status";
import streamingService from "../services/streaming.service";

interface UseStatusUpdateHandlerProps {
    setStatusItems: React.Dispatch<React.SetStateAction<Status[]>>;
}

export const useStatusUpdateHandler = ({ setStatusItems }: UseStatusUpdateHandlerProps) => {
    useEffect(() => {
        const handleStatusUpdate = (response: ChatResponseDto) => {
            if (!response) return;

            setStatusItems(prev => [
                ...prev,
                { message: response.message || '' }
            ]);
        };

        streamingService.on("status", handleStatusUpdate);

        return () => {
            streamingService.off("status", handleStatusUpdate);
        };
    }, [setStatusItems]);
};