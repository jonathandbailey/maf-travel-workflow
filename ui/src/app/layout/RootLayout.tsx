import ChatInput from "../../features/chat/components/ChatInput"
import { Flex, Layout } from "antd"
import { useState } from "react";
import type { Exchange } from "../../features/chat/domain/Exchange";
import { ChatService } from "../../features/chat/api/chat.api";
import styles from './RootLayout.module.css';
import type { Status } from "../../features/chat/domain/Status";
import TravelPlan from "../../features/travel-planning/components/plan/TravelPlan";

import Welcome from "../../features/dashboard/components/Welcome";

import RootHeader from "./RootHeader";
import ChatOutput from "../../features/chat/components/ChatOutput";
import NavigationHeader from "./NavigationHeader";
import TravelOptions from "../../features/travel-planning/components/plan/TravelOptions";
import AgentFeedback from "../../features/chat/components/AgentFeedback";
import { useChatResponseHandler } from "../../features/chat/hooks/useChatResponseHandler";
import { useStatusUpdateHandler } from "../../features/chat/hooks/useStatusUpdateHandler";
import { useTravelPlanUpdateHandler } from "../../features/chat/hooks/useExchangeStatusUpdateHandler";
import { UIFactory } from "../../features/chat/factories/UIFactory";
import type { TravelPlanDto } from "../../features/travel-planning/api/travel.dto";

const { Header, Sider, Content } = Layout;

const RootLayout = () => {
    const [sessionId] = useState<string>(crypto.randomUUID());
    const [exchanges, setExchanges] = useState<Exchange[]>([]);
    const [activeExchange, setActiveExchange] = useState<Exchange | null>(null);
    const [travelPlan, setTravelPlan] = useState<TravelPlanDto | null>(null);
    const [activeStatus, setActiveStatus] = useState<Status | null>(null);

    const [collapsed, setCollapsed] = useState(true);

    useChatResponseHandler({ setActiveExchange, setExchanges });
    useStatusUpdateHandler({ setActiveStatus, setActiveExchange });
    useTravelPlanUpdateHandler({ sessionId, setTravelPlan });

    function handlePrompt(value: string): void {
        const newExchange = UIFactory.createUIExchange(value);

        setActiveExchange(newExchange);

        const conversationService = new ChatService();
        conversationService.startChatExchange(
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
                <RootHeader />
            </Header>

            <Layout style={{ height: "calc(100vh - 64px)" }}>

                <Content style={{ background: "white", height: "100%", display: "flex", flexDirection: "column" }} >
                    <Header style={{ background: "white", padding: 0 }}>
                        <NavigationHeader collapsed={collapsed} setCollapsed={setCollapsed} />
                    </Header>
                    <Flex vertical align="center" style={{ height: "100%", flex: 1, minHeight: 0 }}>
                        <Flex justify="center" align="start">
                            <TravelPlan travelPlan={travelPlan} />
                        </Flex>

                        {exchanges.length === 0 && <Welcome />}
                        <TravelOptions sessionId={sessionId} />

                        <div className={styles.chatInputContainer}>
                            <Flex vertical>
                                {activeExchange && <AgentFeedback message={activeExchange} status={activeStatus} />}

                                <ChatInput onEnter={handlePrompt} />
                            </Flex>

                        </div>
                    </Flex>



                </Content>
                <Sider className={styles.statusSidebar} collapsible collapsed={collapsed} trigger={null} width={550}>
                    {!collapsed && (
                        <ChatOutput exchanges={exchanges} />
                    )}

                </Sider>
            </Layout>
        </Layout>

    </>

}

export default RootLayout