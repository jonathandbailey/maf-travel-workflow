import ChatInput from "../chat/ChatInput"
import { Flex, Splitter, Tabs, Timeline, Layout, Spin } from "antd"
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

        <Layout className={styles.layout}>
            <Header className={styles.header}></Header>

            <Layout>
                <Content className={styles.content}>
                    <div className={styles.mainArea}>
                        Main Area
                    </div>
                    <div className={styles.chatInputContainer}>
                        <Flex vertical>
                            <div>
                                {activeExchange?.assistant.text}
                            </div>
                            {activeExchange?.assistant.isLoading && (
                                <Flex align="center" gap="small" className={styles["assistant-spin-left"]}>
                                    <Spin size="small" />
                                    {activeExchange?.assistant.statusMessage && (
                                        <span style={{ fontSize: '14px', color: '#666' }}>
                                            {activeExchange.assistant.statusMessage}
                                        </span>
                                    )}
                                </Flex>
                            )}
                            <ChatInput onEnter={handlePrompt} />
                        </Flex>

                    </div>
                </Content>
                <Sider className={styles.statusSidebar} width={350}>
                    <Tabs type="card"
                        items={[
                            {
                                label: 'Status',
                                key: 'status',
                                children: (
                                    <div className={styles.statusContainer}>
                                        <Timeline>
                                            {statusItems.map((status, index) => (
                                                <Timeline.Item key={index} >
                                                    {status.message}
                                                </Timeline.Item>
                                            ))}
                                        </Timeline>
                                    </div>

                                )
                            },
                            {
                                label: 'Chat',
                                key: 'chat',
                                children: (
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

                                )
                            }
                        ]}
                    />
                </Sider>
            </Layout>
        </Layout>

    </>

}

export default RootLayout