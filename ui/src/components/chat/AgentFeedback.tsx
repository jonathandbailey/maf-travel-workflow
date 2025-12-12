import { Flex, Spin } from "antd";
import type { UIExchange } from "../../types/ui/UIExchange";
import type { Status } from "../../types/ui/Status";

interface AgentFeedbackProps {
    message: UIExchange;
    status?: Status | null;
}

const AgentFeedback = ({ message, status }: AgentFeedbackProps) => {
    return (
        <> <Flex vertical>
            <div>
                {message?.assistant.text}
            </div>
            {message?.assistant.isLoading && (
                <Flex align="center" gap="small" >
                    <Spin size="small" />
                    {status?.message && (
                        <span style={{ fontSize: '14px', color: '#666' }}>
                            {status.message}
                        </span>
                    )}
                </Flex>
            )}
        </Flex>
        </>
    );
}

export default AgentFeedback;