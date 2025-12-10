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
import { useTravelPlanUpdateHandler } from "../../hooks/useExchangeStatusUpdateHandler";
import { useArtifactHandler } from "../../hooks/useArtifactHandler";
import TravelPlan from "../travel/plan/TravelPlan";
import type { TravelPlanDto } from "../../types/dto/travel-plan.dto";

const { Header, Footer, Sider, Content } = Layout;

const RootLayout = () => {
    const [sessionId] = useState<string>(crypto.randomUUID());
    const [exchanges, setExchanges] = useState<UIExchange[]>([]);
    const [activeExchange, setActiveExchange] = useState<UIExchange | null>(null);
    const [statusItems, setStatusItems] = useState<Status[]>([]);
    const [tabs, setTabs] = useState<TabsProps['items']>([]);
    const [activeKey, setActiveKey] = useState<string>();
    const [travelPlan, setTravelPlan] = useState<TravelPlanDto | null>(null);

    // Debug status items changes

    useChatResponseHandler({ setActiveExchange, setExchanges });
    useStatusUpdateHandler({ setStatusItems });
    useTravelPlanUpdateHandler({ sessionId, setTravelPlan });
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
                <Content style={{ background: "white" }} >


                    <div style={{ padding: "24px" }}>
                        <Flex justify="center" align="start" style={{ minHeight: 'auto', flex: 'none' }}>
                            <TravelPlan travelPlan={travelPlan} />
                        </Flex>
                    </div>


                    <div >
                        <Tabs items={
                            tabs
                        } activeKey={activeKey
                        }></Tabs>
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