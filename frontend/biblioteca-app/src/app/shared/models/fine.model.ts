export type FineReason = 'LATE' | 'DAMAGE' | 'LOSS';
export type FineStatus = 'PENDING' | 'PAID' | 'WAIVED';
export type PaymentMethod = 'CASH' | 'TRANSFER' | 'CARD';

export interface FineRecord {
  id: string;
  returnId: string;
  userId: string;
  reason: FineReason;
  amount: number;
  daysLate: number | null;
  status: FineStatus;
  createdAt: string;
  paidAt: string | null;
  paidByUserId: string | null;
  borrowerName: string | null;
  borrowerEmail: string | null;
  bookTitle: string | null;
}

export interface FineFilters {
  userId?: string;
  status?: FineStatus | '';
}

export interface ConfirmPaymentRequest {
  method: PaymentMethod;
  reference: string | null;
}

export interface WaiveFineRequest {
  reason: string;
}

export interface FineConfig {
  id: string;
  lateRatePerDayMxn: number;
  damageFlatMxn: number;
  lossFlatMxn: number;
  effectiveFrom: string;
}

export interface SaveFineConfigRequest {
  lateRatePerDayMxn: number;
  damageFlatMxn: number;
  lossFlatMxn: number;
  effectiveFrom: string;
}
