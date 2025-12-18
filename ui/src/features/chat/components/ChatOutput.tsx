import { Divider, Flex, Tabs } from "antd";
import type { Exchange } from "../domain/Exchange";
import UserMessage from "./UserMessage";
import AssistantMessage from "./AssistantMessage";
import AgentStatus from "./AgentStatus";


interface ChatOutputProps {
    exchanges: Exchange[];
}

const ChatOutput = ({ exchanges }: ChatOutputProps) => {
    return (
        <>
            <div >
                <Tabs type="card"
                    style={{ height: '100%' }}
                    items={[
                        {
                            label: 'Chat',
                            key: 'chat',
                            children: (
                                <div>
                                    {exchanges.map((exchange, idx) => (
                                        <div key={idx}>
                                            <Flex justify="flex-end" >
                                                <UserMessage message={exchange.user} />
                                            </Flex>
                                            <AssistantMessage message={exchange.assistant} />
                                            <Divider />
                                            <AgentStatus statusItems={exchange.status || []} />
                                            <Divider />
                                        </div>
                                    ))}
                                </div>
                            )
                        }
                    ]}
                />
            </div>
        </>);
}

export default ChatOutput;