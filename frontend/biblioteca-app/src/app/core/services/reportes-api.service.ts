import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Condonacion,
  Deudor,
  DevolucionesTardias,
  MultasRecaudadas
} from '../../shared/models/reporte.model';

@Injectable({ providedIn: 'root' })
export class ReportesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/reportes`;

  getMultasRecaudadas(from: string, to: string): Observable<MultasRecaudadas> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<MultasRecaudadas>(`${this.base}/multas-recaudadas`, { params });
  }

  getMultasPendientes(): Observable<Deudor[]> {
    return this.http.get<Deudor[]>(`${this.base}/multas-pendientes`);
  }

  getDevolucionesTardias(from: string, to: string): Observable<DevolucionesTardias> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<DevolucionesTardias>(`${this.base}/devoluciones-tardias`, { params });
  }

  getCondonaciones(from: string, to: string): Observable<Condonacion[]> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<Condonacion[]>(`${this.base}/condonaciones`, { params });
  }
}
