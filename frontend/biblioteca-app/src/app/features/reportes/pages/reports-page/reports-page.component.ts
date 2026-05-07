import { Component, DestroyRef, inject, signal, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReportesApiService } from '../../../../core/services/reportes-api.service';
import {
  Condonacion,
  Deudor,
  DevolucionesTardias,
  MultasRecaudadas
} from '../../../../shared/models/reporte.model';

type Tab = 'recaudadas' | 'pendientes' | 'tardias' | 'condonaciones';

@Component({
  selector: 'app-reports-page',
  imports: [],
  templateUrl: './reports-page.component.html',
  styleUrl: './reports-page.component.scss'
})
export class ReportsPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly api = inject(ReportesApiService);

  protected readonly activeTab = signal<Tab>('recaudadas');
  protected readonly loading = signal(false);
  protected readonly errorMessage = signal('');

  // Filtros de fecha — por defecto el mes actual
  protected readonly fromDate = signal(this.firstDayOfMonth());
  protected readonly toDate = signal(this.today());

  // Datos por tab
  protected readonly recaudadas = signal<MultasRecaudadas | null>(null);
  protected readonly pendientes = signal<Deudor[]>([]);
  protected readonly tardias = signal<DevolucionesTardias | null>(null);
  protected readonly condonaciones = signal<Condonacion[]>([]);

  protected readonly tabsWithRange: ReadonlyArray<Tab> = ['recaudadas', 'tardias', 'condonaciones'];

  protected readonly showDateFilter = computed(() =>
    this.tabsWithRange.includes(this.activeTab())
  );

  constructor() {
    this.loadCurrentTab();
  }

  protected setTab(tab: Tab): void {
    this.activeTab.set(tab);
    this.loadCurrentTab();
  }

  protected setFromDate(value: string): void {
    this.fromDate.set(value);
    this.loadCurrentTab();
  }

  protected setToDate(value: string): void {
    this.toDate.set(value);
    this.loadCurrentTab();
  }

  protected exportCsv(): void {
    const tab = this.activeTab();
    switch (tab) {
      case 'recaudadas': this.exportRecaudadas(); break;
      case 'pendientes': this.exportPendientes(); break;
      case 'tardias': this.exportTardias(); break;
      case 'condonaciones': this.exportCondonaciones(); break;
    }
  }

  protected formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(amount);
  }

  protected formatDate(isoDate: string): string {
    return new Intl.DateTimeFormat('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })
      .format(new Date(isoDate));
  }

  protected motivoLabel(motivo: string): string {
    switch (motivo) {
      case 'LATE': return 'Retraso';
      case 'DAMAGE': return 'Daño';
      case 'LOSS': return 'Pérdida';
      default: return motivo;
    }
  }

  private loadCurrentTab(): void {
    const tab = this.activeTab();
    this.errorMessage.set('');
    this.loading.set(true);

    const from = this.fromDate();
    const to = this.toDate();

    switch (tab) {
      case 'recaudadas':
        this.api.getMultasRecaudadas(from, to)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (d) => { this.recaudadas.set(d); this.loading.set(false); },
            error: () => { this.errorMessage.set('No se pudo cargar el reporte.'); this.loading.set(false); }
          });
        break;

      case 'pendientes':
        this.api.getMultasPendientes()
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (d) => { this.pendientes.set(d); this.loading.set(false); },
            error: () => { this.errorMessage.set('No se pudo cargar el reporte.'); this.loading.set(false); }
          });
        break;

      case 'tardias':
        this.api.getDevolucionesTardias(from, to)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (d) => { this.tardias.set(d); this.loading.set(false); },
            error: () => { this.errorMessage.set('No se pudo cargar el reporte.'); this.loading.set(false); }
          });
        break;

      case 'condonaciones':
        this.api.getCondonaciones(from, to)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (d) => { this.condonaciones.set(d); this.loading.set(false); },
            error: () => { this.errorMessage.set('No se pudo cargar el reporte.'); this.loading.set(false); }
          });
        break;
    }
  }

  // CSV helpers
  private downloadCsv(filename: string, rows: string[][]): void {
    const bom = '﻿';
    const content = bom + rows.map(r => r.map(c => `"${(c ?? '').toString().replace(/"/g, '""')}"`).join(',')).join('\r\n');
    const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  private exportRecaudadas(): void {
    const d = this.recaudadas();
    if (!d) return;
    const rows: string[][] = [
      ['Reporte: Multas Recaudadas'],
      [`Total General: ${this.formatAmount(d.totalGeneral)}`, `Total Multas: ${d.totalConteo}`],
      [],
      ['Mes', 'Total', 'Cantidad'],
      ...d.porMes.map(m => [m.mes, m.total.toString(), m.conteo.toString()]),
      [],
      ['Motivo', 'Total', 'Cantidad'],
      ...d.porMotivo.map(m => [this.motivoLabel(m.motivo), m.total.toString(), m.conteo.toString()])
    ];
    this.downloadCsv(`multas-recaudadas-${this.fromDate()}-${this.toDate()}.csv`, rows);
  }

  private exportPendientes(): void {
    const rows: string[][] = [
      ['Usuario', 'Email', 'Cant. Multas', 'Total Pendiente'],
      ...this.pendientes().map(d => [
        d.nombreUsuario, d.emailUsuario, d.cantidadMultas.toString(), d.totalPendiente.toString()
      ])
    ];
    this.downloadCsv('multas-pendientes.csv', rows);
  }

  private exportTardias(): void {
    const d = this.tardias();
    if (!d) return;
    const rows: string[][] = [
      ['Total Devoluciones', 'Total Tardías', '% Tardías', 'Promedio Días Retraso'],
      [
        d.totalDevoluciones.toString(),
        d.totalTardias.toString(),
        `${d.porcentajeTardias}%`,
        d.promedioDiasRetraso != null ? d.promedioDiasRetraso.toString() : 'N/A'
      ]
    ];
    this.downloadCsv(`devoluciones-tardias-${this.fromDate()}-${this.toDate()}.csv`, rows);
  }

  private exportCondonaciones(): void {
    const rows: string[][] = [
      ['Deudor', 'Email', 'Motivo Multa', 'Motivo Condonación', 'Monto', 'Fecha', 'Condonado Por'],
      ...this.condonaciones().map(c => [
        c.nombreDeudor,
        c.emailDeudor,
        this.motivoLabel(c.motivo),
        c.motivoCondonacion ?? '',
        c.monto.toString(),
        this.formatDate(c.condonadaEn),
        c.nombreCondonador
      ])
    ];
    this.downloadCsv(`condonaciones-${this.fromDate()}-${this.toDate()}.csv`, rows);
  }

  private today(): string {
    return new Date().toISOString().split('T')[0];
  }

  private firstDayOfMonth(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`;
  }
}
