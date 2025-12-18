import * as signalR from "@microsoft/signalr";

type CallbackFn = (...args: any[]) => void;

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private handlers: Record<string, CallbackFn> = {};

    async initialize(): Promise<void> {
        console.log("Initializing SignalR connection..." + import.meta.env.VITE_HUB_BASE_URL);
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(import.meta.env.VITE_HUB_BASE_URL + "/hub")
            .withAutomaticReconnect()
            .build();

        try {
            await this.connection.start();
            console.log("SignalR connected");
        } catch (err) {
            console.error("SignalR connection error: ", err);
        }
    }

    on(event: string, callback: CallbackFn): void {
        this.handlers[event] = callback;
        if (this.connection) {
            this.connection.off(event);
            this.connection.on(event, callback);
        }
    }

    off(event: string, callback?: CallbackFn): void {
        if (this.connection) {
            if (callback) {
                this.connection.off(event, callback);
            } else {
                this.connection.off(event);
            }
        }
        if (!callback) {
            delete this.handlers[event];
        }
    }

    stop(): void {
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
            this.handlers = {};
        }
    }
}

const streamingService = new SignalRService();
export default streamingService;