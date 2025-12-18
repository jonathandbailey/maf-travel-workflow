import { Flex, Timeline, Typography } from "antd";
import type { Status } from "../domain/Status";

interface AgentStatusProps {
    statusItems: Status[];
}

const { Text, Title } = Typography;

const AgentStatus = ({ statusItems }: AgentStatusProps) => {
    return (
        <>
            <div style={{ marginTop: "8px", marginBottom: "16px" }} >
                <Timeline>
                    {statusItems.map((status, index) => (
                        <Timeline.Item key={index} >
                            <Flex vertical>

                                <Title level={5} style={{ marginTop: "0px" }}> {status.source}  </Title>
                                <Text> {status.message}</Text>

                                <Text type="secondary" italic style={{ marginTop: "8px", marginBottom: "8px" }}>{status.details}</Text>

                            </Flex>

                        </Timeline.Item>
                    ))}
                </Timeline>
            </div>
        </>
    )
}

export default AgentStatus;