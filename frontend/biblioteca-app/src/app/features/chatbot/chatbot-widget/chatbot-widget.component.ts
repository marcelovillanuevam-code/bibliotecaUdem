import { Component, DestroyRef, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { ChatbotApiService } from '../../../core/services/chatbot-api.service';
import { ChatAction, ChatMessage } from '../../../shared/models/chatbot.model';

const WELCOME: ChatMessage = {
  role: 'bot',
  text: '¡Hola! Soy el asistente de la biblioteca UDEM. Puedes preguntarme sobre libros, tus multas, préstamos, horarios y más.',
  timestamp: new Date()
};

@Component({
  selector: 'app-chatbot-widget',
  imports: [],
  templateUrl: './chatbot-widget.component.html',
  styleUrl: './chatbot-widget.component.scss'
})
export class ChatbotWidgetComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly chatApi = inject(ChatbotApiService);
  private readonly router = inject(Router);

  @ViewChild('messagesEl') private messagesRef!: ElementRef<HTMLDivElement>;

  protected readonly panelOpen = signal(false);
  protected readonly loading = signal(false);
  protected readonly messages = signal<ChatMessage[]>([]);

  protected togglePanel(): void {
    const next = !this.panelOpen();
    this.panelOpen.set(next);
    if (next && this.messages().length === 0)
      this.messages.set([WELCOME]);
    if (next) this.scrollToBottom(0);
  }

  protected onSubmit(e: Event, input: HTMLInputElement): void {
    e.preventDefault();
    const text = input.value.trim();
    if (!text || this.loading()) return;
    input.value = '';
    this.sendMessage(text);
  }

  protected navigateTo(url: string): void {
    this.panelOpen.set(false);
    this.router.navigateByUrl(url);
  }

  private sendMessage(text: string): void {
    this.messages.update(msgs => [...msgs, { role: 'user', text, timestamp: new Date() }]);
    this.loading.set(true);
    this.scrollToBottom();

    this.chatApi.send(text)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.messages.update(msgs => [...msgs, {
            role: 'bot',
            text: res.reply,
            actions: res.actions ?? undefined,
            timestamp: new Date()
          }]);
          this.loading.set(false);
          this.scrollToBottom();
        },
        error: () => {
          this.messages.update(msgs => [...msgs, {
            role: 'bot',
            text: 'Hubo un error al procesar tu mensaje. Intenta de nuevo.',
            timestamp: new Date()
          }]);
          this.loading.set(false);
          this.scrollToBottom();
        }
      });
  }

  private scrollToBottom(delay = 50): void {
    setTimeout(() => {
      const el = this.messagesRef?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    }, delay);
  }

  protected trackByIndex(_index: number): number {
    return _index;
  }

  protected trackAction(_index: number, a: ChatAction): string {
    return a.url + a.label;
  }
}
