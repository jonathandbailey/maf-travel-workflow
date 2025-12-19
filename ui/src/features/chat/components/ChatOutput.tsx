import { Divider, Flex, Tabs } from "antd";
import UserMessage from "./UserMessage";
import AssistantMessage from "./AssistantMessage";
import AgentStatus from "./AgentStatus";
import { useExchangesStore } from "../stores/exchanges.store";
import { useStatusStore } from "../stores/status.store";


const ChatOutput = () => {

    const { exchanges } = useExchangesStore();
    const { statuses } = useStatusStore();

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
                        },
                        {
                            label: 'Status',
                            key: 'status',
                            children: (
                                <div>
                                    <AgentStatus statusItems={statuses} />

                                </div>
                            )
                        }
                    ]}
                />
            </div>
        </>);
}

export default ChatOutput;