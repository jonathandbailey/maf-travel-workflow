import { Divider, Flex, Tabs } from "antd";
import UserMessage from "./UserMessage";
import AssistantMessage from "./AssistantMessage";
import AgentStatus from "./AgentStatus";
import { useExchangesStore } from "../stores/exchanges.store";
import { useStatusUpdateStore } from "../stores/status-update.store";


const ChatOutput = () => {

    const { exchanges } = useExchangesStore();
    const { statusUpdates } = useStatusUpdateStore();

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
                                    <AgentStatus statusItems={statusUpdates} />

                                </div>
                            )
                        }
                    ]}
                />
            </div>
        </>);
}

export default ChatOutput;