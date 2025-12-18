import type { Status } from "./Status";
import type { Message } from "./Message";

export interface Exchange {
    id: string;
    user: Message;
    assistant: Message;
    status: Status[]
}