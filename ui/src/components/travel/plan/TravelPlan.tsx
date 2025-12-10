import { Card, Flex, Typography } from "antd";
import type { TravelPlanDto } from "../../../types/dto/travel-plan.dto";
import { ArrowRightOutlined } from "@ant-design/icons";
import dayjs from 'dayjs';
import advancedFormat from 'dayjs/plugin/advancedFormat';

dayjs.extend(advancedFormat);

const { Text } = Typography;

interface TravelPlanProps {
    travelPlan: TravelPlanDto | null;
}

const formatDate = (dateString: string | undefined): string => {
    if (!dateString) return '';
    return dayjs(dateString).format('Do, MMM, YYYY');
};

const TravelPlan = ({ travelPlan }: TravelPlanProps) => {
    // Check if we should show each card
    const showOriginCard = !!(travelPlan?.origin || travelPlan?.startDate);
    const showDestinationCard = !!(travelPlan?.destination || travelPlan?.endDate);
    const showArrow = showOriginCard && showDestinationCard;

    return (
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
    );
}
export default TravelPlan;