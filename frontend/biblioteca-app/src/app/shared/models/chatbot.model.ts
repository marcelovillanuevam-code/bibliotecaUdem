export interface ChatAction {
  label: string;
  url: string;
}

export interface ChatRequest {
  message: string;
}

export interface ChatResponse {
  reply: string;
  actions: ChatAction[] | null;
}

export interface ChatMessage {
  role: 'user' | 'bot';
  text: string;
  actions?: ChatAction[];
  timestamp: Date;
}
