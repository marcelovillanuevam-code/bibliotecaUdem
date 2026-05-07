import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BooksApiService } from '../../../../core/services/books-api.service';
import { ReservationsApiService } from '../../../../core/services/reservations-api.service';
import { BookDetail } from '../../../../shared/models/book.model';
import { Reservation, ReservationStatus } from '../../../../shared/models/reservation.model';
import { AccentTone } from '../../../../shared/models/user.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';

@Component({
  selector: 'app-book-queue-page',
  imports: [RouterLink, PrimaryButtonComponent, StatusBadgeComponent],
  templateUrl: './book-queue-page.component.html',
  styleUrl: './book-queue-page.component.scss'
})
export class BookQueuePageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly booksApi = inject(BooksApiService);
  private readonly reservationsApi = inject(ReservationsApiService);
  protected readonly bookId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly book = signal<BookDetail | null>(null);
  protected readonly queue = signal<Reservation[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');

  constructor() {
    this.loadBook();
    this.loadQueue();
  }

  protected reload(): void {
    this.loadBook();
    this.loadQueue();
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

  protected shortId(value: string): string {
    return value.slice(0, 8);
  }

  protected trackByReservationId = (_: number, reservation: Reservation): string => reservation.id;

  private loadBook(): void {
    if (!this.bookId) {
      return;
    }

    this.booksApi
      .getBook(this.bookId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (book) => this.book.set(book),
        error: () => this.book.set(null)
      });
  }

  private loadQueue(): void {
    if (!this.bookId) {
      this.loading.set(false);
      this.errorMessage.set('No se encontro el identificador del libro.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.reservationsApi
      .listBookQueue(this.bookId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (queue) => {
          this.queue.set(queue);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.resolveErrorMessage(error, 'No fue posible cargar la cola de reservas.'));
          this.loading.set(false);
        }
      });
  }

  private resolveErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof HttpErrorResponse) {
      const detail = error.error?.detail as string | undefined;
      const title = error.error?.title as string | undefined;
      return detail || title || fallbackMessage;
    }

    return fallbackMessage;
  }
}
