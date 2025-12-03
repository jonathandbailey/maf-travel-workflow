import ChatInput from "../chat/ChatInput"
import { Flex, Tabs, Timeline } from "antd"
import type { TabsProps } from "antd";
import { useState } from "react";
import type { UIExchange } from "../../types/ui/UIExchange";
import { ConversationService } from "../../services/conversation.service";
import UserMessage from "../chat/UserMessage";
import AssistantMessage from "../chat/AssistantMessage";
import styles from './RootLayout.module.css';
import { UIFactory } from '../../factories/UIFactory';
import type { Status } from "../../types/ui/Status";
import { useChatResponseHandler } from "../../hooks/useChatResponseHandler";
import { useStatusUpdateHandler } from "../../hooks/useStatusUpdateHandler";
import { useArtifactHandler } from "../../hooks/useArtifactHandler";

const RootLayout = () => {
    const [sessionId] = useState<string>(crypto.randomUUID());
    const [exchanges, setExchanges] = useState<UIExchange[]>([]);
    const [statusItems, setStatusItems] = useState<Status[]>([]);
    const [tabs, setTabs] = useState<TabsProps['items']>([]);
    const [activeKey, setActiveKey] = useState<string>();

    useChatResponseHandler({ setExchanges });
    useStatusUpdateHandler({ setStatusItems });
    useArtifactHandler({ sessionId, setTabs, setActiveKey });

    function handlePrompt(value: string): void {
        const newExchange = UIFactory.createUIExchange(value);

        setExchanges(prev => [...prev, newExchange]);
        setStatusItems([]);

        const conversationService = new ConversationService();
        conversationService.startConversationExchange(
            value,
            newExchange.user.id,
            sessionId,
            newExchange.assistant.id
        ).then(() => {
            // Conversation exchange initiated successfully
        }).catch(error => {
            console.error("Error during conversation exchange:", error);
        });
    }

    return <>

        <Flex gap="large" >

            <div>
                <Tabs
                    items={tabs}
                    activeKey={activeKey}
                    onChange={setActiveKey}
                    tabPlacement="top"
                />
            </div>
            <div className={styles.container}>
                <Flex vertical className={styles.layout}>
                    <div className={styles.content}>
                        {exchanges.map((exchange, idx) => (
                            <div key={idx}>
                                <Flex justify="flex-end" className={styles.userMessageContainer}>
                                    <UserMessage message={exchange.user} />
                                </Flex>
                                <AssistantMessage message={exchange.assistant} />
                            </div>
                        ))}
                    </div>

                    <div className={styles.chatInputContainer}>
                        <ChatInput onEnter={handlePrompt} />
                    </div>
                </Flex>
            </div>
            <div className={styles.statusContainer}>
                <Timeline>
                    {statusItems.map((status, idx) => (
                        <Timeline.Item key={idx}>{status.message}</Timeline.Item>
                    ))}

                </Timeline>
            </div>

        </Flex>




    </>

}

export default RootLayout