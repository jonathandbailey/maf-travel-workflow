import { Flex, Typography } from "antd";
import Flight from "./Flight";
import type { FlightOptionDto } from "../../api/travel.dto";

const { Text } = Typography;

interface FlightListProps {
    flights: FlightOptionDto[];
    returnFlights: FlightOptionDto[];
}

const FlightList = ({ flights, returnFlights }: FlightListProps) => {
    return (<>
        <Flex gap={24}>
            <Flex vertical gap="middle" wrap="wrap" justify="center">
                <Text type="secondary" style={{ fontSize: '12px' }}>Outbound</Text>
                {flights.map((flight, index) => (
                    <Flight key={index} flight={flight} />
                ))}

            </Flex>
            <Flex vertical gap="middle" wrap="wrap" justify="center">
                <Text type="secondary" style={{ fontSize: '12px' }}>Return</Text>

                {returnFlights.map((flight, index) => (
                    <Flight key={index} flight={flight} />
                ))}

            </Flex>
        </Flex>

    </>



    );
}

export default FlightList;