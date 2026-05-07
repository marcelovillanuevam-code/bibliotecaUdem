import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateLoanRequest,
  LoanQuery,
  LoanRecord,
  LoanRenewal,
  LoanStatus
} from '../../shared/models/loan.model';

interface ApiLoanDto {
  id: string;
  userId: string;
  userFullName: string;
  bookCopyId: string;
  bookTitle: string;
  isbn: string | null;
  loanedAt: string;
  dueAt: string;
  returnedAt: string | null;
  status: string;
  renewalCount: number;
  renewals?: LoanRenewal[] | null;
}

@Injectable({ providedIn: 'root' })
export class LoansApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/prestamos`;

  listLoans(query: LoanQuery = {}): Observable<LoanRecord[]> {
    return this.httpClient
      .get<ApiLoanDto[]>(this.baseUrl, { params: this.buildParams(query) })
      .pipe(map((loans) => loans.map((loan) => this.toLoanRecord(loan))));
  }

  listUserLoans(userId: string, query: LoanQuery = {}): Observable<LoanRecord[]> {
    return this.httpClient
      .get<ApiLoanDto[]>(`${environment.apiBaseUrl}/usuarios/${userId}/prestamos`, {
        params: this.buildParams(query)
      })
      .pipe(map((loans) => loans.map((loan) => this.toLoanRecord(loan))));
  }

  listActiveUserLoans(userId: string): Observable<LoanRecord[]> {
    return this.listUserLoans(userId, { status: 'ACTIVE' });
  }

  getLoan(id: string): Observable<LoanRecord> {
    return this.httpClient
      .get<ApiLoanDto>(`${this.baseUrl}/${id}`)
      .pipe(map((loan) => this.toLoanRecord(loan)));
  }

  createLoan(request: CreateLoanRequest): Observable<LoanRecord> {
    return this.httpClient
      .post<ApiLoanDto>(this.baseUrl, {
        userId: request.userId,
        bookCopyId: request.bookCopyId,
        durationDaysOverride: request.durationDaysOverride ?? null
      })
      .pipe(map((loan) => this.toLoanRecord(loan)));
  }

  renewLoan(id: string): Observable<LoanRecord> {
    return this.httpClient
      .post<ApiLoanDto>(`${this.baseUrl}/${id}/renovaciones`, {})
      .pipe(map((loan) => this.toLoanRecord(loan)));
  }

  private buildParams(query: LoanQuery): HttpParams {
    let params = new HttpParams();

    if (query.status) {
      params = params.set('status', query.status);
    }

    return params;
  }

  private toLoanRecord(loan: ApiLoanDto): LoanRecord {
    return {
      id: loan.id,
      userId: loan.userId,
      userFullName: loan.userFullName,
      bookCopyId: loan.bookCopyId,
      bookTitle: loan.bookTitle,
      isbn: loan.isbn ?? 'N/A',
      loanedAt: loan.loanedAt,
      dueAt: loan.dueAt,
      returnedAt: loan.returnedAt,
      status: this.loanStatusFromApi(loan.status),
      renewalCount: loan.renewalCount ?? 0,
      renewals: [...(loan.renewals ?? [])].sort((left, right) =>
        left.renewedAt.localeCompare(right.renewedAt)
      )
    };
  }

  private loanStatusFromApi(status: string): LoanStatus {
    switch (status) {
      case 'RETURNED':
      case 'OVERDUE':
      case 'LOST':
      case 'ACTIVE':
        return status;
      default:
        return 'ACTIVE';
    }
  }
}
