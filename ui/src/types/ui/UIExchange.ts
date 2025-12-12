import type { Status } from "./Status";
import type { UIMessage } from "./UIMessage";

export interface UIExchange {
    id: string;
    user: UIMessage;
    assistant: UIMessage;
    status: Status[]
}