import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatResponse } from '../../shared/models/chatbot.model';

@Injectable({ providedIn: 'root' })
export class ChatbotApiService {
  private readonly http = inject(HttpClient);

  send(message: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(`${environment.apiBaseUrl}/chat`, { message });
  }
}
