import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoanSummary, RegisterReturnRequest, ReturnRecord, ReturnResult } from '../../shared/models/return.model';

@Injectable({ providedIn: 'root' })
export class ReturnsApiService {
  private readonly http = inject(HttpClient);

  listReturns(): Observable<ReturnRecord[]> {
    return this.http.get<ReturnRecord[]>(`${environment.apiBaseUrl}/devoluciones`);
  }

  getReturn(id: string): Observable<ReturnRecord> {
    return this.http.get<ReturnRecord>(`${environment.apiBaseUrl}/devoluciones/${id}`);
  }

  searchActiveLoans(searchParams: { barcode?: string; userId?: string }): Observable<LoanSummary[]> {
    let params = new HttpParams().set('status', 'ACTIVE');
    if (searchParams.barcode) params = params.set('copyBarcode', searchParams.barcode.trim());
    if (searchParams.userId) params = params.set('userId', searchParams.userId.trim());
    return this.http.get<LoanSummary[]>(`${environment.apiBaseUrl}/prestamos`, { params });
  }

  registerReturn(request: RegisterReturnRequest): Observable<ReturnResult> {
    return this.http.post<ReturnResult>(`${environment.apiBaseUrl}/devoluciones`, request);
  }
}
