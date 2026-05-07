import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { DashboardApiService } from '../../../../core/services/dashboard-api.service';
import { AuthSessionService } from '../../../../core/services/auth-session.service';
import {
  DashboardIcon,
  DashboardKpis,
  LoanDailyKpi,
  RecentActivity
} from '../../../../shared/models/dashboard.model';
import { AvatarChipComponent } from '../../../../shared/ui/avatar-chip/avatar-chip.component';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';

interface KpiCard {
  label: string;
  value: string;
  note: string;
  icon: DashboardIcon;
  tone: 'blue' | 'green' | 'amber' | 'violet' | 'red';
}

@Component({
  selector: 'app-dashboard-page',
  imports: [RouterLink, AvatarChipComponent, PrimaryButtonComponent, DatePipe, DecimalPipe],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss'
})
export class DashboardPageComponent implements OnInit {
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly authSession = inject(AuthSessionService);
  protected readonly currentUser = this.authSession.currentUser;

  protected readonly kpis = signal<DashboardKpis | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly refreshedAt = signal<Date | null>(null);
  protected readonly isExecutiveUser = computed(() =>
    this.authSession.hasAnyRole(['ADMIN', 'LIBRARIAN'])
  );

  protected readonly kpiCards = computed<KpiCard[]>(() => {
    const data = this.kpis();
    if (!data) return [];

    return [
      {
        label: 'Libros activos',
        value: this.formatNumber(data.books.active),
        note: `${this.formatNumber(data.books.total)} registros totales`,
        icon: 'books',
        tone: 'blue'
      },
      {
        label: 'Ejemplares disponibles',
        value: this.formatNumber(data.copies.available),
        note: `${this.formatNumber(data.copies.total)} ejemplares en inventario`,
        icon: 'copies',
        tone: 'green'
      },
      {
        label: 'Prestamos activos',
        value: this.formatNumber(data.loans.active),
        note: `${this.formatNumber(data.loans.overdue)} vencidos`,
        icon: 'loans',
        tone: data.loans.overdue > 0 ? 'red' : 'violet'
      },
      {
        label: 'Multas pendientes',
        value: this.formatCurrency(data.fines.totalAmountPendingMxn),
        note: `${this.formatNumber(data.fines.pending)} multas abiertas`,
        icon: 'fines',
        tone: data.fines.pending > 0 ? 'amber' : 'green'
      },
      {
        label: 'Reservas activas',
        value: this.formatNumber(data.reservations.active),
        note: `${this.formatNumber(data.reservations.ready)} listas para retiro`,
        icon: 'reservations',
        tone: 'blue'
      },
      {
        label: 'Usuarios activos',
        value: this.formatNumber(data.users.active),
        note: `${this.formatNumber(data.users.total)} usuarios registrados`,
        icon: 'users',
        tone: 'violet'
      }
    ];
  });

  protected readonly loanChart = computed(() => this.kpis()?.loans.last30Days ?? []);
  protected readonly maxDailyLoans = computed(() =>
    Math.max(1, ...this.loanChart().map((point) => point.total))
  );
  protected readonly recentActivity = computed(() => this.kpis()?.recentActivity ?? []);

  protected readonly quickActions = [
    { label: 'Registrar prestamo', route: '/dashboard/prestamos/nuevo' },
    { label: 'Registrar devolucion', route: '/dashboard/devoluciones/nueva' },
    { label: 'Gestionar multas', route: '/dashboard/multas' },
    { label: 'Ver reportes', route: '/dashboard/reportes' }
  ];

  ngOnInit(): void {
    if (this.isExecutiveUser()) {
      this.loadKpis();
    }
  }

  protected loadKpis(): void {
    if (!this.isExecutiveUser()) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.dashboardApi
      .getKpis()
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (kpis) => {
          this.kpis.set(kpis);
          this.refreshedAt.set(new Date());
        },
        error: () => {
          this.errorMessage.set('No se pudieron cargar los KPIs del dashboard.');
        }
      });
  }

  protected barHeight(point: LoanDailyKpi): number {
    if (point.total === 0) return 4;
    return Math.max(12, Math.round((point.total / this.maxDailyLoans()) * 100));
  }

  protected activityTone(activity: RecentActivity): string {
    switch (activity.action) {
      case 'INSERT':
        return 'green';
      case 'UPDATE':
        return 'blue';
      case 'DELETE':
        return 'red';
      default:
        return 'violet';
    }
  }

  protected statIcon(icon: DashboardIcon): string {
    switch (icon) {
      case 'books':
        return 'M6.5 5.5H10.5C11.3284 5.5 12 6.17157 12 7V19C12 18.1716 11.3284 17.5 10.5 17.5H6.5C5.67157 17.5 5 18.1716 5 19V7C5 6.17157 5.67157 5.5 6.5 5.5ZM13.5 5.5H17.5C18.3284 5.5 19 6.17157 19 7V19C19 18.1716 18.3284 17.5 17.5 17.5H13.5C12.6716 17.5 12 18.1716 12 19V7C12 6.17157 12.6716 5.5 13.5 5.5Z';
      case 'copies':
        return 'M8 4H16V18H8V4ZM6 7H4V20H14V18M10 8H14M10 12H14';
      case 'users':
        return 'M16 20V18.8C16 17.806 15.194 17 14.2 17H8.8C7.80589 17 7 17.806 7 18.8V20M17.5 7.5C17.5 9.15685 16.1569 10.5 14.5 10.5C12.8431 10.5 11.5 9.15685 11.5 7.5C11.5 5.84315 12.8431 4.5 14.5 4.5C16.1569 4.5 17.5 5.84315 17.5 7.5ZM12.5 8C12.5 9.38071 11.3807 10.5 10 10.5C8.61929 10.5 7.5 9.38071 7.5 8C7.5 6.61929 8.61929 5.5 10 5.5C11.3807 5.5 12.5 6.61929 12.5 8Z';
      case 'loans':
        return 'M7 4H17C18.1046 4 19 4.89543 19 6V20L16 18L13 20L10 18L7 20V6C7 4.89543 7.89543 4 9 4ZM10 8H14M10 12H16';
      case 'fines':
        return 'M7 4H17V20H7V4ZM10 8H14M10 12H14M10 16H12M17 8H19M17 12H19';
      case 'reservations':
        return 'M7 5H17C18.1046 5 19 5.89543 19 7V19L16 17.5L13 19L10 17.5L7 19V7C7 5.89543 7.89543 5 9 5ZM10 9H14M10 13H16';
      default:
        return 'M6 12.5L10 16.5L18 7.5M12 21C16.9706 21 21 16.9706 21 12C21 7.02944 16.9706 3 12 3C7.02944 3 3 7.02944 3 12C3 16.9706 7.02944 21 12 21Z';
    }
  }

  private formatNumber(value: number): string {
    return new Intl.NumberFormat('es-MX').format(value);
  }

  private formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
      maximumFractionDigits: 2
    }).format(value);
  }
}
