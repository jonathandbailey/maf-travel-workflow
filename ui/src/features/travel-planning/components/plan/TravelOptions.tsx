import { Tabs, type TabsProps } from "antd";
import { useState } from "react";
import { useFlightOptionStore } from "../../stores/flight-option.store";
import { mapFlightOptionToDto } from "../../domain/mappers";
import FlightList from "../flights/FlightList";

interface TravelOptionsProps {
    sessionId: string;
}

const TravelOptions = ({ sessionId }: TravelOptionsProps) => {
    const [tabs, setTabs] = useState<TabsProps['items']>([]);
    const [activeKey, setActiveKey] = useState<string>();
    const { flightSearchResults } = useFlightOptionStore();

    return (
        <>
            <div style={{ padding: "24px" }}>
                {flightSearchResults && (
                    <FlightList
                        flights={flightSearchResults.departureFlightOptions.map(mapFlightOptionToDto)}
                        returnFlights={flightSearchResults.returnFlightOptions.map(mapFlightOptionToDto)}
                    />
                )}
            </div>
        </>);
}

export default TravelOptions;