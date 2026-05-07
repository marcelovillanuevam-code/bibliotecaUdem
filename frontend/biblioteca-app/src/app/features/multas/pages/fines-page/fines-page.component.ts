import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FinesApiService } from '../../../../core/services/fines-api.service';
import { AuthSessionService } from '../../../../core/services/auth-session.service';
import { FineReason, FineRecord, FineStatus } from '../../../../shared/models/fine.model';
import { AccentTone } from '../../../../shared/models/user.model';
import { FilterSelectComponent } from '../../../../shared/ui/filter-select/filter-select.component';
import { SearchInputComponent } from '../../../../shared/ui/search-input/search-input.component';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';

@Component({
  selector: 'app-fines-page',
  imports: [FilterSelectComponent, SearchInputComponent, StatusBadgeComponent],
  templateUrl: './fines-page.component.html',
  styleUrl: './fines-page.component.scss'
})
export class FinesPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly finesApi = inject(FinesApiService);
  private readonly authSession = inject(AuthSessionService);

  protected readonly isManager = computed(() => {
    const role = this.authSession.currentUserRoleCode();
    return role === 'ADMIN' || role === 'LIBRARIAN';
  });

  protected readonly statusOptions: ReadonlyArray<string> = ['Todas', 'Pendiente', 'Pagada', 'Condonada'];
  protected readonly statusFilter = signal('Todas');
  protected readonly searchTerm = signal('');
  protected readonly fines = signal<FineRecord[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');

  protected readonly filteredFines = computed(() => {
    const apiStatus = this.toApiStatus(this.statusFilter());
    const term = this.searchTerm().trim().toLowerCase();

    return this.fines().filter(fine => {
      const matchesStatus = !apiStatus || fine.status === apiStatus;
      const matchesTerm =
        !term ||
        (fine.borrowerName?.toLowerCase().includes(term) ?? false) ||
        (fine.borrowerEmail?.toLowerCase().includes(term) ?? false) ||
        (fine.bookTitle?.toLowerCase().includes(term) ?? false);
      return matchesStatus && matchesTerm;
    });
  });

  constructor() {
    this.loadFines();
  }

  protected updateStatus(value: string): void {
    this.statusFilter.set(value);
  }

  protected updateSearch(value: string): void {
    this.searchTerm.set(value);
  }

  protected viewFine(fine: FineRecord): void {
    this.router.navigateByUrl(`/dashboard/multas/${fine.id}`);
  }

  protected reload(): void {
    this.loadFines();
  }

  protected statusLabel(status: FineStatus): string {
    switch (status) {
      case 'PENDING': return 'Pendiente';
      case 'PAID': return 'Pagada';
      case 'WAIVED': return 'Condonada';
    }
  }

  protected statusTone(status: FineStatus): AccentTone {
    switch (status) {
      case 'PENDING': return 'amber';
      case 'PAID': return 'green';
      case 'WAIVED': return 'slate';
    }
  }

  protected reasonLabel(reason: FineReason): string {
    switch (reason) {
      case 'LATE': return 'Retraso';
      case 'DAMAGE': return 'Daño';
      case 'LOSS': return 'Pérdida';
    }
  }

  protected formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(amount);
  }

  protected formatDate(isoDate: string): string {
    return new Intl.DateTimeFormat('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })
      .format(new Date(isoDate));
  }

  private toApiStatus(label: string): FineStatus | '' {
    switch (label) {
      case 'Pendiente': return 'PENDING';
      case 'Pagada': return 'PAID';
      case 'Condonada': return 'WAIVED';
      default: return '';
    }
  }

  private loadFines(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    const userId = this.authSession.currentUserId();
    const obs = this.isManager()
      ? this.finesApi.listFines({})
      : this.finesApi.getUserFines(userId!);

    obs.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (fines) => {
        this.fines.set(fines);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('No fue posible cargar las multas.');
        this.loading.set(false);
      }
    });
  }
}
