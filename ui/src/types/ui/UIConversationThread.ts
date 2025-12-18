import type { UIExchange } from "./UIExchange";

export interface UIConversationThread {
    id: string;
    exchanges: UIExchange[];
}