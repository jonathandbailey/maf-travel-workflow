import ChatInput from "../chat/ChatInput"
import { Flex, Typography, Tabs, Timeline, Layout, Spin, Button } from "antd"
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

import { FaPlaneUp } from "react-icons/fa6";
import Welcome from "./Welcome";
import { MenuFoldOutlined, MenuUnfoldOutlined } from "@ant-design/icons";

import TravelIcon from '../../assets/fly.png';

const { Header, Sider, Content } = Layout;

const { Title } = Typography;

const RootLayout = () => {
    const [sessionId] = useState<string>(crypto.randomUUID());
    const [exchanges, setExchanges] = useState<UIExchange[]>([]);
    const [activeExchange, setActiveExchange] = useState<UIExchange | null>(null);
    const [statusItems, setStatusItems] = useState<Status[]>([]);
    const [tabs, setTabs] = useState<TabsProps['items']>([]);
    const [activeKey, setActiveKey] = useState<string>();
    const [travelPlan, setTravelPlan] = useState<TravelPlanDto | null>(null);
    const [activeStatus, setActiveStatus] = useState<Status | null>(null);

    const [collapsed, setCollapsed] = useState(true);

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
                    <img
                        src={TravelIcon}
                        alt="Travel App Logo"
                        // 2. Resize the image via CSS since the source file is large
                        style={{ height: '64px', width: 'auto', marginRight: '0px' }}
                    />
                    <Title level={4} style={{ marginLeft: "2px", marginBottom: 0, marginTop: 0 }}>Travel Planner</Title>
                </Flex>

            </Header>

            <Layout style={{ height: "calc(100vh - 64px)" }}>

                <Content style={{ background: "white", height: "100%", display: "flex", flexDirection: "column" }} >
                    <Header style={{ background: "white", padding: 0 }}>
                        <Flex justify="end">
                            <Button
                                type="text"
                                icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                                onClick={() => setCollapsed(!collapsed)}
                                style={{
                                    fontSize: '16px',
                                    width: 64,
                                    height: 64,
                                }}
                            />
                        </Flex>

                    </Header>
                    <Flex vertical align="center" style={{ height: "100%", flex: 1, minHeight: 0 }}>
                        <div style={{ padding: "24px", flexShrink: 0 }}>
                            <Flex justify="center" align="start">
                                <TravelPlan travelPlan={travelPlan} />
                            </Flex>
                        </div>

                        {exchanges.length === 0 && <Welcome />}
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
                <Sider className={styles.statusSidebar} collapsible collapsed={collapsed} trigger={null} width={350}>
                    {!collapsed && (
                        <div className={styles.chatContainer}>
                            <Tabs type="card"
                                style={{ height: '100%' }}
                                items={[
                                    {
                                        label: 'Chat',
                                        key: 'chat',
                                        children: (
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
                                        )
                                    }
                                ]}
                            />
                        </div>
                    )}

                </Sider>
            </Layout>
        </Layout>

    </>

}

export default RootLayout