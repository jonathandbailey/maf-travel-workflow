import { Flex } from "antd";
import AssistantMessage from "./AssistantMessage";
import UserMessage from "./UserMessage";
import type { UIConversationThread } from "../../../types/ui/UIConversationThread";

interface ConversationThreadProps {
    thread: UIConversationThread;
}

const ExchangeMessages = ({ thread }: ConversationThreadProps) => {
    return (
        <>
            {thread.exchanges.map((exchange, idx) => (
                <div key={idx}>
                    <Flex justify="flex-end" style={{ width: "100%" }}>
                        <UserMessage message={exchange.user} />
                    </Flex>


                    <AssistantMessage message={exchange.assistant} />
                </div>
            ))}



        </>);
}

export default ExchangeMessages;