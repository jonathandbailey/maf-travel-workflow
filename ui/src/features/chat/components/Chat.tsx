import { Flex } from "antd";
import AgentFeedback from "./AgentFeedback";
import ChatInput from "./ChatInput";
import { useEffect, useRef, useState } from "react";
import type { Exchange } from "../domain/Exchange";
import { UIFactory } from "../factories/UIFactory";
import { useStatusUpdateHandler } from "../hooks/useStatusUpdateHandler";
import { EventType, HttpAgent, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";
import { TravelService } from "../../travel-planning/api/travel.api";
import { useTravelPlanStore } from "../../travel-planning/stores/travel-plan.store";
import { mapTravelPlanDtoToDomain } from "../../travel-planning/domain/mappers";

interface ChatProps {
    sessionId: string;
}

const Chat = ({ sessionId }: ChatProps) => {

    const [activeExchange, setActiveExchange] = useState<Exchange>(UIFactory.createUIExchange(""));
    const agentRef = useRef<HttpAgent | null>(null);
    const travelService = new TravelService();
    const { addTravelPlan } = useTravelPlanStore();

    const [isLoading, setIsLoading] = useState(false);
    const [statusMessage, setStatusMessage] = useState<string>("");

    useStatusUpdateHandler();

    const [currentStream, setCurrentStream] = useState('');
    const streamTextRef = useRef('');
    const subscriptionRef = useRef<any>(null);



    useEffect(() => {
        const agent = new HttpAgent({
            url: "https://localhost:7266/ag-ui",
            agentId: "unique-agent-id",
            threadId: sessionId
        });


        console.log("Initializing agent with sessionId:", sessionId);
        agentRef.current = agent;

        const subscription = agentRef.current.subscribe({
            onEvent: ({ event }: { event: BaseEvent }) => {
                console.log("Received agent event:", event);

                if (event.type === EventType.TEXT_MESSAGE_CONTENT) {
                    const delta = (event as any).delta || '';
                    streamTextRef.current += delta;
                    setCurrentStream(streamTextRef.current);
                }

                if (event.type === EventType.TEXT_MESSAGE_END) {
                    activeExchange.assistant.isLoading = false;
                    setIsLoading(false);

                    // Call TravelPlan Service and update store
                    travelService.getTravelPlan(sessionId)
                        .then(travelPlanDto => {
                            const travelPlan = mapTravelPlanDtoToDomain(travelPlanDto, sessionId);
                            addTravelPlan(travelPlan);
                        })
                        .catch(error => {
                            console.error("Failed to fetch travel plan:", error);
                        });
                }
                if (event.type === EventType.TEXT_MESSAGE_START) {
                    activeExchange.assistant.isLoading = true;
                    setIsLoading(true);
                }

                if (event.type === EventType.RUN_ERROR) {
                    activeExchange.assistant.isLoading = false;
                    setIsLoading(false);
                }

                if (event.type === EventType.STATE_SNAPSHOT) {
                    setStatusMessage((event as StateSnapshotEvent).snapshot || '');
                }
            },
            onRunFailed: ({ error }: { error: Error }) => {
                console.error("Agent error:", error);

            },
            onRunFinalized: () => {
                console.log("Agent run complete");
            }
        });
        subscriptionRef.current = subscription;
    }, [sessionId]);



    useEffect(() => {
        // Reset stream when activeExchange changes
        streamTextRef.current = '';
        setCurrentStream('');
    }, [activeExchange]);


    function handlePrompt(value: string): void {
        const newExchange = UIFactory.createUIExchange(value);

        setActiveExchange(newExchange);

        setIsLoading(true);
        agentRef.current!.setMessages([{
            id: Date.now().toString(),
            role: "user",
            content: value
        }]);

        agentRef.current!.runAgent({
            tools: [],
            context: [],
            runId: newExchange.assistant.id
        }).catch((error) => {
            console.error('Error running agent:', error);
        });
    }



    return (<>
        <Flex vertical>
            {activeExchange && <AgentFeedback statusMessage={statusMessage} isLoading={isLoading} message={activeExchange} currentStream={currentStream} />}

            <ChatInput onEnter={handlePrompt} />
        </Flex>
    </>)
};
export default Chat;