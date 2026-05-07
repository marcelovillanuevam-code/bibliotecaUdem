export type NotificationType =
  | 'LOAN_RECEIPT'
  | 'RETURN_RECEIPT'
  | 'FINE_CREATED'
  | 'RESERVATION_READY'
  | 'DUE_REMINDER';
export type NotificationChannel = 'EMAIL' | 'WHATSAPP' | 'IN_APP';
export type NotificationStatus = 'PENDING' | 'SENT' | 'FAILED';

export interface Notification {
  id: string;
  userId: string;
  type: NotificationType;
  channel: NotificationChannel;
  subject: string;
  body: string;
  status: NotificationStatus;
  createdAt: string;
  sentAt: string | null;
  payloadJson: string;
}
