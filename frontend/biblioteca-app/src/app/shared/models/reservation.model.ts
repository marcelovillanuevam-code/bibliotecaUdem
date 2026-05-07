export type ReservationStatus = 'PENDING' | 'READY' | 'FULFILLED' | 'CANCELLED' | 'EXPIRED';
export type ReservationStatusFilter = 'ALL' | ReservationStatus;

export interface Reservation {
  id: string;
  userId: string;
  userFullName: string;
  bookId: string;
  bookTitle: string;
  queuePosition: number;
  status: ReservationStatus;
  createdAt: string;
  readyAt: string | null;
  expiresAt: string | null;
  fulfilledAt: string | null;
  fulfilledByLoanId: string | null;
}

export interface CreateReservationRequest {
  bookId: string;
}
