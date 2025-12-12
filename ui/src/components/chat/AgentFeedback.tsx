import { Flex, Popover, Spin } from "antd";
import type { UIExchange } from "../../types/ui/UIExchange";
import type { Status } from "../../types/ui/Status";
import { OpenAIOutlined } from "@ant-design/icons";

interface AgentFeedbackProps {
    message: UIExchange;
    status?: Status | null;
}

const AgentFeedback = ({ message, status }: AgentFeedbackProps) => {
    return (
        <> <Flex vertical>
            <div style={{ marginBottom: "24px", marginLeft: "24px", height: "32px" }} >
                {!message?.assistant.isLoading && (
                    <Flex>
                        <Popover open={true}
                            placement="right"
                            content={<div style={{ maxWidth: "600px" }}>{message?.assistant.text}</div>}
                        >
                            <OpenAIOutlined />
                        </Popover>

                        { }
                    </Flex>)}

            </div>
            {message?.assistant.isLoading && (
                <Flex align="center" gap="small" style={{ padding: "16px" }}>
                    <Spin size="small" />
                    {status?.message && (
                        <span style={{ fontSize: '14px', color: '#666' }}>
                            {status.message}
                        </span>
                    )}
                </Flex>
            )}
        </Flex >
        </>
    );
}

export default AgentFeedback;