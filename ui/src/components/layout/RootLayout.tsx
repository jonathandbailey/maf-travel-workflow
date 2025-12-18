import ChatInput from "../chat/ChatInput"
import { Flex, Layout } from "antd"
import { useState } from "react";
import type { UIExchange } from "../../types/ui/UIExchange";
import { ConversationService } from "../../services/conversation.service";
import styles from './RootLayout.module.css';
import { UIFactory } from '../../factories/UIFactory';
import type { Status } from "../../types/ui/Status";
import { useChatResponseHandler } from "../../hooks/useChatResponseHandler";
import { useStatusUpdateHandler } from "../../hooks/useStatusUpdateHandler";
import { useTravelPlanUpdateHandler } from "../../hooks/useExchangeStatusUpdateHandler";
import TravelPlan from "../travel/plan/TravelPlan";
import type { TravelPlanDto } from "../../types/dto/travel-plan.dto";
import AgentFeedback from "../chat/AgentFeedback";

import Welcome from "./Welcome";

import RootHeader from "./RootHeader";
import ChatOutput from "../chat/ChatOutput";
import NavigationHeader from "./NavigationHeader";
import TravelOptions from "../travel/plan/TravelOptions";

const { Header, Sider, Content } = Layout;

const RootLayout = () => {
    const [sessionId] = useState<string>(crypto.randomUUID());
    const [exchanges, setExchanges] = useState<UIExchange[]>([]);
    const [activeExchange, setActiveExchange] = useState<UIExchange | null>(null);
    const [travelPlan, setTravelPlan] = useState<TravelPlanDto | null>(null);
    const [activeStatus, setActiveStatus] = useState<Status | null>(null);

    const [collapsed, setCollapsed] = useState(true);

    useChatResponseHandler({ setActiveExchange, setExchanges });
    useStatusUpdateHandler({ setActiveStatus, setActiveExchange });
    useTravelPlanUpdateHandler({ sessionId, setTravelPlan });

    function handlePrompt(value: string): void {
        const newExchange = UIFactory.createUIExchange(value);

        setActiveExchange(newExchange);

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