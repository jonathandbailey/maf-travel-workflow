import { Flex, Spin } from "antd";
import type { UIExchange } from "../../types/ui/UIExchange";

interface AgentFeedbackProps {
    message: UIExchange;
}

const AgentFeedback = ({ message }: AgentFeedbackProps) => {
    return (
        <> <Flex vertical>
            <div>
                {message?.assistant.text}
            </div>
            {message?.assistant.isLoading && (
                <Flex align="center" gap="small" >
                    <Spin size="small" />
                    {message?.assistant.statusMessage && (
                        <span style={{ fontSize: '14px', color: '#666' }}>
                            {message.assistant.statusMessage}
                        </span>
                    )}
                </Flex>
            )}
        </Flex>
        </>
    );
}

export default AgentFeedback;