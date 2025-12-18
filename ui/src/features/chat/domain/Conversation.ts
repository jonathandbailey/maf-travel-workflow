import type { ConversationThread } from "./ConversationThread";

export interface Conversation {
    id: string;
    title: string;
    threads: ConversationThread[];
}
