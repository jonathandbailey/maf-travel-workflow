import { Card, Flex, Typography } from "antd";
import { ArrowRightOutlined } from "@ant-design/icons";
import dayjs from 'dayjs';
import advancedFormat from 'dayjs/plugin/advancedFormat';
import Flight from "../flights/Flight";
import { useTravelPlanStore } from "../../stores/travel-plan.store";

dayjs.extend(advancedFormat);

const { Text } = Typography;

interface TravelPlanProps {
    sessionId: string;
}

const formatDate = (date: Date | null | undefined): string => {
    if (!date) return '';
    return dayjs(date).format('Do, MMM, YYYY');
};

const TravelPlan = ({ sessionId }: TravelPlanProps) => {

    const travelPlan = useTravelPlanStore((state) => {
        console.log("TravelPlan lookup - SessionId:", sessionId, "Available IDs:", state.travelPlans.map(tp => tp.id));
        return state.travelPlans.find(tp => tp.id === sessionId);
    });

    console.log("Found travel plan:", travelPlan ? "YES" : "NO");

    const showOriginCard = !!(travelPlan?.origin || travelPlan?.startDate);
    const showDestinationCard = !!(travelPlan?.destination || travelPlan?.endDate);
    const showArrow = showOriginCard && showDestinationCard;

    return (
        <>
            <Flex gap="small" style={{ minHeight: 'auto' }}>
                {showOriginCard && (
                    <Card size="small" style={{ padding: '8px 12px', boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)", }}>
                        <Flex vertical gap="extra-small">
                            <Text type="secondary" style={{ fontSize: '12px' }}>From</Text>
                            <Text strong style={{ fontSize: '20px' }}>{travelPlan?.origin}</Text>
                            <Text style={{ fontSize: '14px' }}>{formatDate(travelPlan?.startDate)}</Text>
                        </Flex>
                    </Card>
                )}
                {showArrow && <ArrowRightOutlined />}
                {showDestinationCard && (
                    <Card size="small" style={{ padding: '8px 12px', boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)", }}>
                        <Flex vertical>
                            <Text type="secondary" style={{ fontSize: '12px' }}>To</Text>
                            <Text strong style={{ fontSize: '20px' }}>{travelPlan?.destination}</Text>
                            <Text style={{ fontSize: '14px' }}>{formatDate(travelPlan?.endDate)}</Text>
                        </Flex>
                    </Card>
                )}
            </Flex>
            <div>
                {travelPlan?.flightPlan?.flightOption && (
                    <Flight flight={travelPlan.flightPlan.flightOption} />
                )}
            </div>
        </>
    );
}
export default TravelPlan;