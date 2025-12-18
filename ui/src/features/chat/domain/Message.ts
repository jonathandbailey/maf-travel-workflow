export interface Message {
    id: string;
    text: string;
    role: 'user' | 'assistant';
    isLoading: boolean;
    hasError: boolean;
    errorMessage: string;
    statusMessage: string;
}