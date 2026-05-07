import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Notification,
  NotificationChannel,
  NotificationStatus,
  NotificationType
} from '../../shared/models/notification.model';

interface ApiNotificationDto {
  id: string;
  userId: string;
  type: string;
  channel: string;
  subject: string;
  body: string;
  status: string;
  createdAt: string;
  sentAt: string | null;
  payloadJson?: string | null;
}

@Injectable({ providedIn: 'root' })
export class NotificationsApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/notificaciones`;

  listInbox(unreadOnly = false): Observable<Notification[]> {
    const params = new HttpParams().set('unreadOnly', unreadOnly);

    return this.httpClient
      .get<ApiNotificationDto[]>(this.baseUrl, { params })
      .pipe(map((notifications) => notifications.map((notification) => this.toNotification(notification))));
  }

  markAsRead(id: string): Observable<void> {
    return this.httpClient.patch<void>(`${this.baseUrl}/${id}/leida`, {});
  }

  private toNotification(notification: ApiNotificationDto): Notification {
    return {
      id: notification.id,
      userId: notification.userId,
      type: this.notificationTypeFromApi(notification.type),
      channel: this.notificationChannelFromApi(notification.channel),
      subject: notification.subject,
      body: notification.body,
      status: this.notificationStatusFromApi(notification.status),
      createdAt: notification.createdAt,
      sentAt: notification.sentAt,
      payloadJson: notification.payloadJson?.trim() || '{}'
    };
  }

  private notificationTypeFromApi(type: string): NotificationType {
    switch (type) {
      case 'LOAN_RECEIPT':
      case 'RETURN_RECEIPT':
      case 'FINE_CREATED':
      case 'RESERVATION_READY':
      case 'DUE_REMINDER':
        return type;
      default:
        return 'DUE_REMINDER';
    }
  }

  private notificationChannelFromApi(channel: string): NotificationChannel {
    switch (channel) {
      case 'EMAIL':
      case 'WHATSAPP':
      case 'IN_APP':
        return channel;
      default:
        return 'IN_APP';
    }
  }

  private notificationStatusFromApi(status: string): NotificationStatus {
    switch (status) {
      case 'SENT':
      case 'FAILED':
      case 'PENDING':
        return status;
      default:
        return 'PENDING';
    }
  }
}
