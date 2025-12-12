import ChatInput from "../chat/ChatInput"
import { Flex, Typography, Tabs, Timeline, Layout, Spin } from "antd"
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
import AgentFeedback from "../chat/AgentFeedback";
import AgentStatus from "../chat/AgentStatus";

const { Header, Sider, Content } = Layout;

const { Text } = Typography;

const RootLayout = () => {
    const [sessionId] = useState<string>(crypto.randomUUID());
    const [exchanges, setExchanges] = useState<UIExchange[]>([]);
    const [activeExchange, setActiveExchange] = useState<UIExchange | null>(null);
    const [statusItems, setStatusItems] = useState<Status[]>([]);
    const [tabs, setTabs] = useState<TabsProps['items']>([]);
    const [activeKey, setActiveKey] = useState<string>();
    const [travelPlan, setTravelPlan] = useState<TravelPlanDto | null>(null);
    const [activeStatus, setActiveStatus] = useState<Status | null>(null);

    // Debug status items changes

    useChatResponseHandler({ setActiveExchange, setExchanges });
    useStatusUpdateHandler({ setStatusItems, setActiveStatus, setActiveExchange });
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
            <Header className={styles.header}>
                <Flex justify="start" align="center" style={{ height: "100%" }}>
                    <Text style={{ color: "black", fontSize: "24px" }}>Travel Agent</Text>
                </Flex>

            </Header>

            <Layout style={{ height: "calc(100vh - 64px)" }}>
                <Content style={{ background: "white", height: "100%", display: "flex", flexDirection: "column" }} >
                    <Flex vertical align="center" style={{ height: "100%", flex: 1, minHeight: 0 }}>
                        <div style={{ padding: "24px", flexShrink: 0 }}>
                            <Flex justify="center" align="start">
                                <TravelPlan travelPlan={travelPlan} />
                            </Flex>
                        </div>


                        <div style={{ padding: "24px" }} >
                            <Tabs
                                items={tabs}
                                type="card"
                                activeKey={activeKey}
                                onChange={setActiveKey}
                            />
                        </div>
                        <div className={styles.chatInputContainer}>
                            <Flex vertical>
                                {activeExchange && <AgentFeedback message={activeExchange} status={activeStatus} />}

                                <ChatInput onEnter={handlePrompt} />
                            </Flex>

                        </div>
                    </Flex>



                </Content>
                <Sider className={styles.statusSidebar} width={350}>
                    <Tabs type="card"
                        items={[

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
                                                    <AgentStatus statusItems={exchange.status || []} />
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