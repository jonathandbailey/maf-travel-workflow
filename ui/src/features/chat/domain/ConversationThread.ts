import type { Exchange } from "./Exchange";

export interface ConversationThread {
    id: string;
    exchanges: Exchange[];
}