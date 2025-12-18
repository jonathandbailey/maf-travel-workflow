import { useEffect } from "react";
import type { TabsProps } from "antd";
import streamingService from "../../../app/api/streaming.api";
import FlightList from "../components/flights/FlightList";
import HotelList from "../components/hotels/HotelList";
import { TravelService } from "../api/travel.api";
import type { ArtifactStatusDto } from "../../chat/api/chat.dto";

interface UseArtifactHandlerProps {
    sessionId: string;
    setTabs: React.Dispatch<React.SetStateAction<TabsProps['items']>>;
    setActiveKey: React.Dispatch<React.SetStateAction<string | undefined>>;
}

export const useArtifactHandler = ({ sessionId, setTabs, setActiveKey }: UseArtifactHandlerProps) => {
    useEffect(() => {
        const handleArtifact = (response: ArtifactStatusDto) => {

            if (response.key === 'Flights') {
                const travelService = new TravelService();
                travelService.getFlightPlan(sessionId).then(flightPlan => {
                    console.log("Flight plan downloaded:", flightPlan);

                    const newTab = {
                        key: response.key,
                        label: response.key,
                        children: (
                            <div style={{ padding: '16px' }}>
                                <FlightList flights={flightPlan.results} />
                            </div>
                        ),
                    };

                    setTabs(prev => {
                        const existingTab = prev?.find(tab => tab.key === response.key);
                        if (existingTab) {
                            // Update existing tab with flight plan content
                            const updatedTabs = prev?.map(tab =>
                                tab.key === response.key ? newTab : tab
                            ) || [];
                            setActiveKey(response.key);
                            return updatedTabs;
                        }
                        const newTabs = prev ? [...prev, newTab] : [newTab];
                        setActiveKey(response.key);
                        return newTabs;
                    });
                }).catch(error => {
                    console.error("Error downloading flight plan:", error);

                    const errorTab = {
                        key: response.key,
                        label: response.key,
                        children: <div style={{ padding: '16px', color: 'red' }
                        }> Error loading flight plan: {error.message} </div>,
                    };

                    setTabs(prev => {
                        const existingTab = prev?.find(tab => tab.key === response.key);
                        if (existingTab) {
                            const updatedTabs = prev?.map(tab =>
                                tab.key === response.key ? errorTab : tab
                            ) || [];
                            setActiveKey(response.key);
                            return updatedTabs;
                        }
                        const newTabs = prev ? [...prev, errorTab] : [errorTab];
                        setActiveKey(response.key);
                        return newTabs;
                    });
                });
                return; // Early return to avoid creating default tab
            }

            if (response.key === 'hotels') {
                const travelService = new TravelService();
                travelService.getHotelPlan(sessionId).then(hotelPlan => {
                    console.log("Hotel plan downloaded:", hotelPlan);

                    const newTab = {
                        key: response.key,
                        label: response.key,
                        children: (
                            <div style={{ padding: '16px' }}>
                                <HotelList hotels={hotelPlan.results} />
                            </div>
                        ),
                    };

                    setTabs(prev => {
                        const existingTab = prev?.find(tab => tab.key === response.key);
                        if (existingTab) {
                            // Update existing tab with hotel plan content
                            const updatedTabs = prev?.map(tab =>
                                tab.key === response.key ? newTab : tab
                            ) || [];
                            setActiveKey(response.key);
                            return updatedTabs;
                        }
                        const newTabs = prev ? [...prev, newTab] : [newTab];
                        setActiveKey(response.key);
                        return newTabs;
                    });
                }).catch(error => {
                    console.error("Error downloading hotel plan:", error);

                    const errorTab = {
                        key: response.key,
                        label: response.key,
                        children: <div style={{ padding: '16px', color: 'red' }
                        }> Error loading hotel plan: {error.message} </div>,
                    };

                    setTabs(prev => {
                        const existingTab = prev?.find(tab => tab.key === response.key);
                        if (existingTab) {
                            const updatedTabs = prev?.map(tab =>
                                tab.key === response.key ? errorTab : tab
                            ) || [];
                            setActiveKey(response.key);
                            return updatedTabs;
                        }
                        const newTabs = prev ? [...prev, errorTab] : [errorTab];
                        setActiveKey(response.key);
                        return newTabs;
                    });
                });
                return; // Early return to avoid creating default tab
            }

            const newTab = {
                key: response.key,
                label: response.key,
                children: <div>Artifact: {response.key}</div>,
            };

            setTabs(prev => {
                const existingTab = prev?.find(tab => tab.key === response.key);
                if (existingTab) {
                    setActiveKey(response.key);
                    return prev;
                }
                const newTabs = prev ? [...prev, newTab] : [newTab];
                setActiveKey(response.key);
                return newTabs;
            });
        };

        streamingService.on("artifact", handleArtifact);

        return () => {
            streamingService.off("artifact", handleArtifact);
        };
    }, [sessionId, setTabs, setActiveKey]);
};