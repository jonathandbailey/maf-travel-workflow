import { useEffect } from "react";
import type { Status } from "../domain/Status";
import streamingService from "../../../app/api/streaming.api";
import type { ChatResponseDto } from "../api/chat.dto";
import { useStatusStore } from "../stores/status.store";



export const useStatusUpdateHandler = () => {

    const { addStatus } = useStatusStore();

    useEffect(() => {
        const handleStatusUpdate = (response: ChatResponseDto) => {
            if (!response) return;

            const newStatus: Status = {
                message: response.message || '',
                details: response.details || '',
                source: response.source || ''
            };

            addStatus(newStatus);

        };

        streamingService.on("status", handleStatusUpdate);

        return () => {
            streamingService.off("status", handleStatusUpdate);
        };
    }, [addStatus]);
};