import { useEffect } from "react";
import { TravelService } from "../../travel-planning/api/travel.api";
import streamingService from "../../../app/api/streaming.api";
import type { TravelPlanDto } from "../../travel-planning/api/travel.dto";

interface UseExchangeStatusUpdateHandlerProps {
    sessionId: string;
    setTravelPlan: React.Dispatch<React.SetStateAction<TravelPlanDto | null>>;
}

export const useTravelPlanUpdateHandler = ({ sessionId, setTravelPlan }: UseExchangeStatusUpdateHandlerProps) => {
    useEffect(() => {

        const handleExchangeStatusUpdate = () => {

            console.log('Requesting travel plan for sessionId:', sessionId);
            const travelService = new TravelService();
            travelService.getTravelPlan(sessionId)

                .then((travelPlan: TravelPlanDto) => {
                    console.log('Travel plan received:', travelPlan);
                    setTravelPlan(travelPlan);
                })

        };

        streamingService.on("travelPlan", handleExchangeStatusUpdate);

        return () => {

            streamingService.off("travelPlan", handleExchangeStatusUpdate);
        };
    }, [setTravelPlan]);
};