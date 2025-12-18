import type { Exchange } from "../domain/Exchange";
import type { Message } from "../domain/Message";


export class UIFactory {
    /**
     * Creates a new UIMessage with default values
     */
    static createUIMessage(
        text: string,
        role: 'user' | 'assistant',
        options?: {
            id?: string;
            isLoading?: boolean;
            hasError?: boolean;
            errorMessage?: string;
            statusMessage?: string;
        }
    ): Message {
        return {
            id: options?.id ?? crypto.randomUUID(),
            text,
            role,
            isLoading: options?.isLoading ?? false,
            hasError: options?.hasError ?? false,
            errorMessage: options?.errorMessage ?? '',
            statusMessage: options?.statusMessage ?? ''
        };
    }

    /**
     * Creates a new user message
     */
    static createUserMessage(text: string, id?: string): Message {
        return this.createUIMessage(text, 'user', { id });
    }

    /**
     * Creates a new assistant message in loading state
     */
    static createAssistantMessage(id?: string): Message {
        return this.createUIMessage('', 'assistant', {
            id,
            isLoading: true
        });
    }

    /**
     * Creates a new UIExchange with user and assistant messages
     */
    static createUIExchange(
        userText: string,
        options?: {
            id?: string;
            userMessageId?: string;
            assistantMessageId?: string;
        }
    ): Exchange {
        const userMessage = this.createUserMessage(userText, options?.userMessageId);
        const assistantMessage = this.createAssistantMessage(options?.assistantMessageId);

        return {
            id: options?.id ?? crypto.randomUUID(),
            user: userMessage,
            assistant: assistantMessage,
            status: []
        };
    }

    /**
     * Updates an assistant message with new text and loading state
     */
    static updateAssistantMessage(
        message: Message,
        text: string,
        isLoading: boolean = false
    ): Message {
        return {
            ...message,
            text,
            isLoading
        };
    }
}