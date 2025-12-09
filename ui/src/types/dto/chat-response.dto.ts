export interface ChatResponseDto {
    id: string
    message: string;
    threadId: string;
    isEndOfStream: boolean;
}