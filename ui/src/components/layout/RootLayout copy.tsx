import ChatInput from "../chat/ChatInput"
import { Flex, Splitter, Tabs, Timeline, Layout } from "antd"
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
import { useExchangeStatusUpdateHandler } from "../../hooks/useExchangeStatusUpdateHandler";
import { useArtifactHandler } from "../../hooks/useArtifactHandler";

const { Header, Footer, Sider, Content } = Layout;

const RootLayout = () => {
    const [sessionId] = useState<string>(crypto.randomUUID());
    const [exchanges, setExchanges] = useState<UIExchange[]>([]);
    const [activeExchange, setActiveExchange] = useState<UIExchange | null>(null);
    const [statusItems, setStatusItems] = useState<Status[]>([]);
    const [tabs, setTabs] = useState<TabsProps['items']>([]);
    const [activeKey, setActiveKey] = useState<string>();

    // Debug status items changes
    console.log('Current statusItems:', statusItems);

    useChatResponseHandler({ setActiveExchange, setExchanges });
    useStatusUpdateHandler({ setStatusItems });
    // useExchangeStatusUpdateHandler({ setExchanges });
    useArtifactHandler({ sessionId, setTabs, setActiveKey });

    function handlePrompt(value: string): void {
        const newExchange = UIFactory.createUIExchange(value);

        setActiveExchange(newExchange);

        // setExchanges(prev => [...prev, newExchange]);
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

        <Layout>
            <Header className={styles.header}></Header>
            <Layout>
                <Content className={styles.contentArea}>
                    <Tabs className={styles.tabs}
                        items={tabs}
                        activeKey={activeKey}
                        onChange={setActiveKey}
                        tabPlacement="top"
                    />
                    {activeExchange && (
                        <div>
                            <Flex justify="flex-end" className={styles.userMessageContainer}>
                                <UserMessage message={activeExchange?.user} />
                            </Flex>
                            <AssistantMessage message={activeExchange?.assistant} />
                        </div>
                    )}

                    <div className={styles.chatInputContainer}>
                        <ChatInput onEnter={handlePrompt} />
                    </div>
                </Content>
                <Sider className={styles.statusSidebar} width={300} >
                    <div className={styles.container}>
                        <Tabs>
                            <Tabs.TabPane tab="Chat" key="chat">
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


                                </Flex>
                            </Tabs.TabPane>
                            <Tabs.TabPane tab="Status" key="status">
                                <Timeline>
                                    {statusItems.map((status, index) => (
                                        <Timeline.Item key={index} >
                                            {status.message}
                                        </Timeline.Item>
                                    ))}
                                </Timeline>
                            </Tabs.TabPane>
                        </Tabs>

                    </div>
                </Sider>
            </Layout>


        </Layout>




    </>

}

export default RootLayout