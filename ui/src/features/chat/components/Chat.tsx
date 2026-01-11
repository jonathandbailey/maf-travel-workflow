import { Flex } from "antd";
import AgentFeedback from "./AgentFeedback";
import ChatInput from "./ChatInput";
import { useEffect, useRef, useState } from "react";
import type { Exchange } from "../domain/Exchange";
import { ChatService } from "../api/chat.api";
import { UIFactory } from "../factories/UIFactory";
import { useStatusUpdateHandler } from "../hooks/useStatusUpdateHandler";
import type { ChatResponseDto } from "../api/chat.dto";
import streamingService from "../../../app/api/streaming.api";

interface ChatProps {
    sessionId: string;
}

const Chat = ({ sessionId }: ChatProps) => {

    const [activeExchange, setActiveExchange] = useState<Exchange>(UIFactory.createUIExchange(""));

    useStatusUpdateHandler();

    const [currentStream, setCurrentStream] = useState('');
    const streamTextRef = useRef('');

    useEffect(() => {
        const handleUserResponse = (response: ChatResponseDto) => {
            if (!response) return;

            console.log("Received streaming response:", response);
            streamTextRef.current += response.message;
            setCurrentStream(streamTextRef.current);
        }

        streamingService.on("user", handleUserResponse);

        return () => {
            streamingService.off("user", handleUserResponse);
        };
    }, []);

    useEffect(() => {
        // Reset stream when activeExchange changes
        streamTextRef.current = '';
        setCurrentStream('');
    }, [activeExchange]);


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

        }).catch(error => {
            console.error("Error during conversation exchange:", error);
        });
    }


    return (<>
        <Flex vertical>
            {activeExchange && <AgentFeedback message={activeExchange} currentStream={currentStream} />}

            <ChatInput onEnter={handlePrompt} />
        </Flex>
    </>)
};
export default Chat;