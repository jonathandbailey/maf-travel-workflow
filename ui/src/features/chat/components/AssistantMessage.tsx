import { Alert, Card, Flex, Spin } from "antd";
import Markdown from "react-markdown";
import styles from "./AssistantMessage.module.css";
import type { UIMessage } from "../../../types/ui/UIMessage";

interface AssistantMessageProps {
    message: UIMessage
}

const AssistantMessage = ({ message }: AssistantMessageProps) => {
    return (
        <Card
            className={styles["assistant-message-card"]}
            variant="borderless"

        >
            {message.hasError ? (
                <Alert
                    title={message.errorMessage}
                    description="There was an error processing your request. Please try again."
                    type="error"
                    showIcon
                />
            ) : (
                <Flex vertical>
                    {message.isLoading && (
                        <Flex align="center" gap="small" className={styles["assistant-spin-left"]}>
                            <Spin size="small" />
                            {message.statusMessage && (
                                <span style={{ fontSize: '14px', color: '#666' }}>
                                    {message.statusMessage}
                                </span>
                            )}
                        </Flex>
                    )}
                    <Markdown>{message.text}</Markdown>
                </Flex>

            )}
        </Card>
    );
};

export default AssistantMessage;