import { Card, Flex, Spin } from "antd";
import { OpenAIOutlined } from "@ant-design/icons";
import type { Exchange } from "../domain/Exchange";
import { useStatusUpdateStore } from "../stores/status-update.store";

interface AgentFeedbackProps {
    message: Exchange;
    currentStream?: string;
    isLoading: boolean;
    statusMessage?: string;
}

const AgentFeedback = ({ message, currentStream, isLoading, statusMessage }: AgentFeedbackProps) => {


    const { currentStatusUpdate } = useStatusUpdateStore();

    return (
        <> <Flex vertical>
            <div style={{ marginBottom: "96px", marginLeft: "24px", height: "32px" }} >

                <Flex>

                    <Card variant="borderless" style={{ boxShadow: "0 0px 0px rgba(0, 0, 0, 0.1)", marginLeft: "16px", width: "600px" }}>
                        <div style={{ maxWidth: "600px" }}>{currentStream}</div>
                    </Card>
                </Flex>

            </div>
            {isLoading && (
                <Flex align="center" gap="small" style={{ padding: "16px" }}>
                    <Spin size="small" />
                    {statusMessage && (
                        <span style={{ fontSize: '14px', color: '#666' }}>
                            {statusMessage}
                        </span>
                    )}
                </Flex>
            )}
        </Flex >
        </>
    );
}

export default AgentFeedback;