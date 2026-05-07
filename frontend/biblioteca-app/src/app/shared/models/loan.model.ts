export type LoanStatus = 'ACTIVE' | 'RETURNED' | 'OVERDUE' | 'LOST';
export type LoanStatusFilter = 'ALL' | LoanStatus;

export interface LoanRenewal {
  id: string;
  loanId: string;
  renewedAt: string;
  previousDueAt: string;
  newDueAt: string;
  renewedByUserId: string;
}

export interface LoanRecord {
  id: string;
  userId: string;
  userFullName: string;
  bookCopyId: string;
  bookTitle: string;
  isbn: string;
  loanedAt: string;
  dueAt: string;
  returnedAt: string | null;
  status: LoanStatus;
  renewalCount: number;
  renewals: LoanRenewal[];
}

export interface CreateLoanRequest {
  userId: string;
  bookCopyId: string;
  durationDaysOverride?: number | null;
}

export interface LoanQuery {
  status?: LoanStatus | null;
}
