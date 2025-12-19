import { Card, Flex, Spin } from "antd";
import { OpenAIOutlined } from "@ant-design/icons";
import type { Exchange } from "../domain/Exchange";
import { useStatusStore } from "../stores/status.store";

interface AgentFeedbackProps {
    message: Exchange;

}

const AgentFeedback = ({ message }: AgentFeedbackProps) => {

    const { activeStatus } = useStatusStore();

    return (
        <> <Flex vertical>
            <div style={{ marginBottom: "96px", marginLeft: "24px", height: "32px" }} >
                {!message?.assistant.isLoading && (
                    <Flex>
                        <OpenAIOutlined height={48} width={48} />
                        <Card style={{ boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)", marginLeft: "16px" }}>
                            <div style={{ maxWidth: "600px" }}>{message?.assistant.text}</div>
                        </Card>




                    </Flex>)}

            </div>
            {message?.assistant.isLoading && (
                <Flex align="center" gap="small" style={{ padding: "16px" }}>
                    <Spin size="small" />
                    {activeStatus?.message && (
                        <span style={{ fontSize: '14px', color: '#666' }}>
                            {activeStatus.message}
                        </span>
                    )}
                </Flex>
            )}
        </Flex >
        </>
    );
}

export default AgentFeedback;