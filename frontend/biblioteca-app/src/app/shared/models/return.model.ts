export type ReturnCondition = 'OK' | 'DAMAGED' | 'LOST';

export interface LoanSummary {
  id: string;
  userId: string;
  bookCopyId: string;
  bookTitle: string;
  copyBarcode: string;
  borrowerName: string;
  borrowerEmail: string;
  loanedAt: string;
  dueAt: string;
  status: string;
}

export interface ReturnRecord {
  id: string;
  loanId: string;
  returnedAt: string;
  condition: ReturnCondition;
  inspectionNotes: string | null;
  receivedByUserId: string;
  bookTitle: string | null;
  copyBarcode: string | null;
  borrowerName: string | null;
  borrowerEmail: string | null;
  fines?: Array<{ id: string; reason: string; amount: number }>;
}

export interface ReturnResult {
  return: ReturnRecord;
  finesGenerated: Array<{ id: string; reason: string; amount: number }>;
}

export interface RegisterReturnRequest {
  loanId: string;
  condition: ReturnCondition;
  inspectionNotes?: string | null;
}
