import type { UIConversationThread } from "./UIConversationThread";

export interface UIConversation {
    id: string;
    title: string;
    threads: UIConversationThread[];
}
