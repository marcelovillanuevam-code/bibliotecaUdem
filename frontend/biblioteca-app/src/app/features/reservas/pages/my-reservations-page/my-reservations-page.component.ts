import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AuthSessionService } from '../../../../core/services/auth-session.service';
import { ReservationsApiService } from '../../../../core/services/reservations-api.service';
import { Reservation, ReservationStatus } from '../../../../shared/models/reservation.model';
import { resolveHttpError } from '../../../../shared/utils/http-error';
import { AccentTone } from '../../../../shared/models/user.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';

type ToastTone = 'success' | 'error';

@Component({
  selector: 'app-my-reservations-page',
  imports: [RouterLink, PrimaryButtonComponent, StatusBadgeComponent],
  templateUrl: './my-reservations-page.component.html',
  styleUrl: './my-reservations-page.component.scss'
})
export class MyReservationsPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly authSession = inject(AuthSessionService);
  private readonly reservationsApi = inject(ReservationsApiService);

  protected readonly reservations = signal<Reservation[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly cancellingReservationId = signal<string | null>(null);
  protected readonly toast = signal<{ tone: ToastTone; message: string } | null>(null);
  protected readonly now = signal(new Date());

  protected readonly activeReservations = computed(() =>
    this.reservations().filter((reservation) =>
      reservation.status === 'PENDING' || reservation.status === 'READY'
    )
  );
  protected readonly historyReservations = computed(() =>
    this.reservations().filter((reservation) =>
      reservation.status !== 'PENDING' && reservation.status !== 'READY'
    )
  );

  constructor() {
    this.startCountdownClock();
    this.loadReservations();
  }

  protected reload(): void {
    this.loadReservations();
  }

  protected dismissToast(): void {
    this.toast.set(null);
  }

  protected cancelReservation(reservation: Reservation): void {
    if (!this.canCancel(reservation) || this.cancellingReservationId() === reservation.id) {
      return;
    }

    this.cancellingReservationId.set(reservation.id);
    this.toast.set(null);

    this.reservationsApi
      .cancelReservation(reservation.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.cancellingReservationId.set(null);
          this.reservations.update((reservations) =>
            reservations.map((currentReservation) =>
              currentReservation.id === reservation.id
                ? { ...currentReservation, status: 'CANCELLED' }
                : currentReservation
            )
          );
          this.showToast('Reserva cancelada correctamente.', 'success');
        },
        error: (error: unknown) => {
          this.cancellingReservationId.set(null);
          this.showToast(resolveHttpError(error, 'No fue posible cancelar la reserva.'), 'error');
        }
      });
  }

  protected canCancel(reservation: Reservation): boolean {
    return reservation.status === 'PENDING' || reservation.status === 'READY';
  }

  protected isCancelling(reservation: Reservation): boolean {
    return this.cancellingReservationId() === reservation.id;
  }

  protected statusLabel(status: ReservationStatus): string {
    switch (status) {
      case 'PENDING': return 'Pendiente';
      case 'READY': return 'Lista';
      case 'FULFILLED': return 'Completada';
      case 'CANCELLED': return 'Cancelada';
      case 'EXPIRED': return 'Expirada';
    }
  }

  protected statusTone(status: ReservationStatus): AccentTone {
    switch (status) {
      case 'READY': return 'green';
      case 'PENDING': return 'amber';
      case 'FULFILLED': return 'blue';
      case 'CANCELLED':
      case 'EXPIRED':
        return 'slate';
    }
  }

  protected formatDateTime(value: string | null): string {
    if (!value) {
      return 'N/A';
    }

    return new Intl.DateTimeFormat('es-MX', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(new Date(value));
  }

  protected countdownLabel(reservation: Reservation): string {
    if (reservation.status !== 'READY' || !reservation.expiresAt) {
      return 'Sin contador activo';
    }

    const remainingMs = new Date(reservation.expiresAt).getTime() - this.now().getTime();

    if (remainingMs <= 0) {
      return 'Tiempo agotado';
    }

    const totalMinutes = Math.ceil(remainingMs / 60000);
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    return `${hours}h ${minutes.toString().padStart(2, '0')}m`;
  }

  protected trackByReservationId = (_: number, reservation: Reservation): string => reservation.id;

  private loadReservations(): void {
    const currentUserId = this.authSession.currentUserId();

    if (!currentUserId) {
      this.loading.set(false);
      this.errorMessage.set('No fue posible identificar al usuario de la sesion.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.reservationsApi
      .listUserReservations(currentUserId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (reservations) => {
          this.reservations.set(this.sortReservations(reservations));
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(resolveHttpError(error, 'No fue posible cargar tus reservas.'));
          this.loading.set(false);
        }
      });
  }

  private sortReservations(reservations: ReadonlyArray<Reservation>): Reservation[] {
    const statusOrder: Record<ReservationStatus, number> = {
      READY: 0,
      PENDING: 1,
      FULFILLED: 2,
      CANCELLED: 3,
      EXPIRED: 4
    };

    return [...reservations].sort((left, right) => {
      const statusDiff = statusOrder[left.status] - statusOrder[right.status];
      if (statusDiff !== 0) {
        return statusDiff;
      }

      return right.createdAt.localeCompare(left.createdAt);
    });
  }

  private startCountdownClock(): void {
    if (typeof window === 'undefined') {
      return;
    }

    const intervalId = window.setInterval(() => this.now.set(new Date()), 1000);
    this.destroyRef.onDestroy(() => window.clearInterval(intervalId));
  }

  private showToast(message: string, tone: ToastTone): void {
    this.toast.set({ message, tone });
  }

}

