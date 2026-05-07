import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateReservationRequest,
  Reservation,
  ReservationStatus
} from '../../shared/models/reservation.model';

interface ApiReservationDto {
  id: string;
  userId: string;
  userFullName?: string | null;
  bookId: string;
  bookTitle: string;
  queuePosition: number;
  status: string;
  createdAt: string;
  readyAt: string | null;
  expiresAt: string | null;
  fulfilledAt: string | null;
  fulfilledByLoanId: string | null;
}

@Injectable({ providedIn: 'root' })
export class ReservationsApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/reservas`;

  createReservation(request: CreateReservationRequest): Observable<Reservation> {
    return this.httpClient
      .post<ApiReservationDto>(this.baseUrl, { bookId: request.bookId })
      .pipe(map((reservation) => this.toReservation(reservation)));
  }

  getReservation(id: string): Observable<Reservation> {
    return this.httpClient
      .get<ApiReservationDto>(`${this.baseUrl}/${id}`)
      .pipe(map((reservation) => this.toReservation(reservation)));
  }

  listUserReservations(userId: string): Observable<Reservation[]> {
    return this.httpClient
      .get<ApiReservationDto[]>(`${environment.apiBaseUrl}/usuarios/${userId}/reservas`)
      .pipe(map((reservations) => reservations.map((reservation) => this.toReservation(reservation))));
  }

  listBookQueue(bookId: string): Observable<Reservation[]> {
    return this.httpClient
      .get<ApiReservationDto[]>(`${environment.apiBaseUrl}/libros/${bookId}/reservas`)
      .pipe(map((reservations) => reservations.map((reservation) => this.toReservation(reservation))));
  }

  cancelReservation(id: string): Observable<void> {
    return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
  }

  private toReservation(reservation: ApiReservationDto): Reservation {
    return {
      id: reservation.id,
      userId: reservation.userId,
      userFullName: reservation.userFullName?.trim() || 'N/A',
      bookId: reservation.bookId,
      bookTitle: reservation.bookTitle,
      queuePosition: reservation.queuePosition,
      status: this.reservationStatusFromApi(reservation.status),
      createdAt: reservation.createdAt,
      readyAt: reservation.readyAt,
      expiresAt: reservation.expiresAt,
      fulfilledAt: reservation.fulfilledAt,
      fulfilledByLoanId: reservation.fulfilledByLoanId
    };
  }

  private reservationStatusFromApi(status: string): ReservationStatus {
    switch (status) {
      case 'READY':
      case 'FULFILLED':
      case 'CANCELLED':
      case 'EXPIRED':
      case 'PENDING':
        return status;
      default:
        return 'PENDING';
    }
  }
}
