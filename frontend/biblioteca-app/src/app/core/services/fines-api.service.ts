import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ConfirmPaymentRequest,
  FineConfig,
  FineFilters,
  FineRecord,
  FineStatus,
  SaveFineConfigRequest,
  WaiveFineRequest
} from '../../shared/models/fine.model';

@Injectable({ providedIn: 'root' })
export class FinesApiService {
  private readonly http = inject(HttpClient);

  listFines(filters: FineFilters): Observable<FineRecord[]> {
    let params = new HttpParams();
    if (filters.userId) params = params.set('userId', filters.userId);
    if (filters.status) params = params.set('status', filters.status);
    return this.http.get<FineRecord[]>(`${environment.apiBaseUrl}/multas`, { params });
  }

  getFine(id: string): Observable<FineRecord> {
    return this.http.get<FineRecord>(`${environment.apiBaseUrl}/multas/${id}`);
  }

  getUserFines(userId: string, status?: FineStatus): Observable<FineRecord[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<FineRecord[]>(`${environment.apiBaseUrl}/usuarios/${userId}/multas`, { params });
  }

  listUserFines(userId: string, status?: FineStatus): Observable<FineRecord[]> {
    return this.getUserFines(userId, status);
  }

  confirmPayment(fineId: string, request: ConfirmPaymentRequest): Observable<FineRecord> {
    return this.http.post<FineRecord>(`${environment.apiBaseUrl}/multas/${fineId}/pagos`, request);
  }

  waiveFine(fineId: string, request: WaiveFineRequest): Observable<FineRecord> {
    return this.http.post<FineRecord>(`${environment.apiBaseUrl}/multas/${fineId}/condonar`, request);
  }

  getActiveConfig(): Observable<FineConfig> {
    return this.http.get<FineConfig>(`${environment.apiBaseUrl}/configuracion-multas`);
  }

  saveConfig(request: SaveFineConfigRequest): Observable<FineConfig> {
    return this.http.put<FineConfig>(`${environment.apiBaseUrl}/configuracion-multas`, request);
  }

  getPendingCount(userId: string): Observable<number> {
    return this.getUserFines(userId, 'PENDING').pipe(map(fines => fines.length));
  }
}
