import { Component, DestroyRef, ElementRef, HostListener, inject, signal, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { AuthSessionService } from '../../../../core/services/auth-session.service';
import { NotificationsApiService } from '../../../../core/services/notifications-api.service';
import { Notification, NotificationType } from '../../../../shared/models/notification.model';

@Component({
  selector: 'app-notification-bell',
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.scss'
})
export class NotificationBellComponent {
  private static readonly pollingMs = 60000;
  private readonly destroyRef = inject(DestroyRef);
  private readonly elementRef = inject(ElementRef<HTMLElement>);
  private readonly router = inject(Router);
  private readonly authSession = inject(AuthSessionService);
  private readonly notificationsApi = inject(NotificationsApiService);

  protected readonly open = signal(false);
  protected readonly loading = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly processingNotificationId = signal<string | null>(null);
  protected readonly notifications = signal<Notification[]>([]);
  protected readonly latestNotifications = computed(() => this.notifications().slice(0, 10));
  protected readonly unreadCount = computed(() =>
    this.notifications().filter((notification) => notification.status === 'PENDING').length
  );

  constructor() {
    this.loadNotifications();
    this.startPolling();
  }

  @HostListener('document:click', ['$event.target'])
  protected closeOnOutsideClick(target: EventTarget | null): void {
    if (target instanceof Node && !this.elementRef.nativeElement.contains(target)) {
      this.open.set(false);
    }
  }

  protected toggleOpen(): void {
    this.open.update((value) => !value);

    if (this.open()) {
      this.loadNotifications();
    }
  }

  protected refresh(): void {
    this.loadNotifications();
  }

  protected openNotification(notification: Notification): void {
    const targetRoute = this.resolveTargetRoute(notification);

    if (notification.status !== 'PENDING') {
      this.open.set(false);
      this.router.navigateByUrl(targetRoute);
      return;
    }

    this.processingNotificationId.set(notification.id);

    this.notificationsApi
      .markAsRead(notification.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.processingNotificationId.set(null);
          this.markAsReadLocally(notification.id);
          this.open.set(false);
          this.router.navigateByUrl(targetRoute);
        },
        error: () => {
          this.processingNotificationId.set(null);
          this.open.set(false);
          this.router.navigateByUrl(targetRoute);
        }
      });
  }

  protected isProcessing(notification: Notification): boolean {
    return this.processingNotificationId() === notification.id;
  }

  protected unreadBadge(): string {
    const count = this.unreadCount();
    return count > 99 ? '99+' : count.toString();
  }

  protected typeLabel(type: NotificationType): string {
    switch (type) {
      case 'LOAN_RECEIPT': return 'Prestamo';
      case 'RETURN_RECEIPT': return 'Devolucion';
      case 'FINE_CREATED': return 'Multa';
      case 'RESERVATION_READY': return 'Reserva';
      case 'DUE_REMINDER': return 'Recordatorio';
    }
  }

  protected formatTimestamp(value: string): string {
    return new Intl.DateTimeFormat('es-MX', {
      day: '2-digit',
      month: 'short',
      hour: '2-digit',
      minute: '2-digit'
    }).format(new Date(value));
  }

  protected trackByNotificationId = (_: number, notification: Notification): string => notification.id;

  private loadNotifications(): void {
    if (!this.authSession.isAuthenticated()) {
      this.notifications.set([]);
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.notificationsApi
      .listInbox(false)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (notifications) => {
          this.notifications.set(notifications);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('No fue posible cargar notificaciones.');
          this.loading.set(false);
        }
      });
  }

  private startPolling(): void {
    if (typeof window === 'undefined') {
      return;
    }

    const intervalId = window.setInterval(() => this.loadNotifications(), NotificationBellComponent.pollingMs);
    this.destroyRef.onDestroy(() => window.clearInterval(intervalId));
  }

  private markAsReadLocally(notificationId: string): void {
    const sentAt = new Date().toISOString();

    this.notifications.update((notifications) =>
      notifications.map((notification) =>
        notification.id === notificationId
          ? { ...notification, status: 'SENT', sentAt }
          : notification
      )
    );
  }

  private resolveTargetRoute(notification: Notification): string {
    const payload = this.parsePayload(notification.payloadJson);
    const loanId = this.payloadString(payload, 'loanId');
    const fineId = this.payloadString(payload, 'fineId');
    const reservationId = this.payloadString(payload, 'reservationId');
    const bookId = this.payloadString(payload, 'bookId');

    if (loanId) {
      return `/dashboard/prestamos/${loanId}`;
    }

    if (fineId) {
      return `/dashboard/multas/${fineId}`;
    }

    if (reservationId) {
      return '/dashboard/reservas';
    }

    if (bookId && this.authSession.hasAnyRole(['LIBRARIAN', 'ADMIN'])) {
      return `/dashboard/libros/${bookId}/reservas`;
    }

    return notification.type === 'RESERVATION_READY' ? '/dashboard/reservas' : '/dashboard';
  }

  private parsePayload(payloadJson: string): Record<string, unknown> {
    try {
      const parsedPayload = JSON.parse(payloadJson) as unknown;
      return typeof parsedPayload === 'object' && parsedPayload !== null
        ? (parsedPayload as Record<string, unknown>)
        : {};
    } catch {
      return {};
    }
  }

  private payloadString(payload: Record<string, unknown>, key: string): string | null {
    const value = payload[key];
    return typeof value === 'string' && value.trim() ? value : null;
  }
}
