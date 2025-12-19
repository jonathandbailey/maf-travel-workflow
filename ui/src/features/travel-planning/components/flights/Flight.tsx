
import { Card, Flex, Typography } from "antd";
import type { FlightOptionDto } from "../../api/travel.dto";
import dayjs from 'dayjs';
import advancedFormat from 'dayjs/plugin/advancedFormat';
import { ArrowRightOutlined } from "@ant-design/icons";

const { Text } = Typography;

dayjs.extend(advancedFormat);

const formatDate = (dateString: string | undefined): string => {
    if (!dateString) return '';
    return dayjs(dateString).format('HH:mm');
};

interface FlightProps {
    flight: FlightOptionDto;
}

const Flight = ({ flight }: FlightProps) => {
    return (
        <Card
            size="small"

            style={{ boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)" }}
        >

            <Flex vertical gap={8}>

                <Flex gap={32} justify="space-around" >
                    <Flex vertical gap="small" >
                        <Text strong style={{ fontSize: '20px' }}>{flight.airline}</Text>
                        <Text type="secondary" style={{ fontSize: '12px' }}>{flight.flightNumber}</Text>
                    </Flex>
                    <Flex vertical gap="extra-small" align="end">
                        <Text type="secondary" style={{ fontSize: '12px' }}>Depart</Text>
                        <Text style={{ fontSize: '24px' }}>{formatDate(flight.departure.datetime)}</Text>
                        <Text style={{ fontSize: '12px' }}>{flight.departure.airportCode}</Text>

                    </Flex>
                    <Flex>
                        <ArrowRightOutlined />
                    </Flex>
                    <Flex vertical gap="extra-small">
                        <Text type="secondary" style={{ fontSize: '12px' }}>Arrive</Text>
                        <Text style={{ fontSize: '24px' }}>{formatDate(flight.arrival.datetime)}</Text>
                        <Text style={{ fontSize: '12px' }}>{flight.arrival.airportCode}</Text>

                    </Flex>
                    <Flex align="center">
                        <Text style={{ fontSize: '24px' }}>{flight.price.amount}{flight.price.currency}</Text>

                    </Flex>

                </Flex>

            </Flex>



        </Card >

    );
}

export default Flight;