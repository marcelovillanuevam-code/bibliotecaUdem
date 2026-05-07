import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthSessionService } from '../../../../core/services/auth-session.service';
import { LoansApiService } from '../../../../core/services/loans-api.service';
import { LoanRecord, LoanStatus, LoanStatusFilter } from '../../../../shared/models/loan.model';
import { AccentTone } from '../../../../shared/models/user.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';

interface LoanFiltersForm {
  status: LoanStatusFilter;
  user: string;
  loanedFrom: string;
  loanedTo: string;
  dueFrom: string;
  dueTo: string;
}

@Component({
  selector: 'app-loans-page',
  imports: [ReactiveFormsModule, RouterLink, PrimaryButtonComponent, StatusBadgeComponent],
  templateUrl: './loans-page.component.html',
  styleUrl: './loans-page.component.scss'
})
export class LoansPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly authSession = inject(AuthSessionService);
  private readonly loansApi = inject(LoansApiService);

  protected readonly statusOptions: ReadonlyArray<{ value: LoanStatusFilter; label: string }> = [
    { value: 'ALL', label: 'Todos' },
    { value: 'ACTIVE', label: 'Activo' },
    { value: 'OVERDUE', label: 'Vencido' },
    { value: 'RETURNED', label: 'Devuelto' },
    { value: 'LOST', label: 'Perdido' }
  ];
  protected readonly pageSizeOptions = [8, 12, 20];
  protected readonly canManageLoans = computed(() =>
    this.authSession.hasAnyRole(['LIBRARIAN', 'ADMIN'])
  );
  protected readonly loans = signal<LoanRecord[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly currentPage = signal(1);
  protected readonly pageSize = signal(8);
  protected readonly appliedFilters = signal<LoanFiltersForm>(this.emptyFilters());
  protected readonly filterForm = this.formBuilder.nonNullable.group({
    status: ['ALL' as LoanStatusFilter],
    user: [''],
    loanedFrom: [''],
    loanedTo: [''],
    dueFrom: [''],
    dueTo: ['']
  });

  protected readonly filteredLoans = computed(() => {
    const filters = this.appliedFilters();
    const userTerm = filters.user.trim().toLowerCase();

    return this.loans().filter((loan) => {
      const matchesUser =
        !userTerm ||
        loan.userFullName.toLowerCase().includes(userTerm) ||
        loan.userId.toLowerCase().includes(userTerm);
      const matchesLoanedFrom = this.isDateOnOrAfter(loan.loanedAt, filters.loanedFrom);
      const matchesLoanedTo = this.isDateOnOrBefore(loan.loanedAt, filters.loanedTo);
      const matchesDueFrom = this.isDateOnOrAfter(loan.dueAt, filters.dueFrom);
      const matchesDueTo = this.isDateOnOrBefore(loan.dueAt, filters.dueTo);

      return matchesUser && matchesLoanedFrom && matchesLoanedTo && matchesDueFrom && matchesDueTo;
    });
  });

  protected readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.filteredLoans().length / this.pageSize()))
  );
  protected readonly pagedLoans = computed(() => {
    const page = Math.min(this.currentPage(), this.totalPages());
    const start = (page - 1) * this.pageSize();
    return this.filteredLoans().slice(start, start + this.pageSize());
  });
  protected readonly activeLoans = computed(() =>
    this.loans().filter((loan) => loan.status === 'ACTIVE' || loan.status === 'OVERDUE')
  );
  protected readonly historyLoans = computed(() =>
    this.loans().filter((loan) => loan.status !== 'ACTIVE' && loan.status !== 'OVERDUE')
  );

  constructor() {
    this.loadLoans();
  }

  protected applyFilters(): void {
    this.appliedFilters.set(this.filterForm.getRawValue());
    this.currentPage.set(1);
    this.loadLoans();
  }

  protected resetFilters(): void {
    const filters = this.emptyFilters();
    this.filterForm.reset(filters);
    this.appliedFilters.set(filters);
    this.currentPage.set(1);
    this.loadLoans();
  }

  protected reload(): void {
    this.loadLoans();
  }

  protected setPageSize(value: string): void {
    this.pageSize.set(Number(value) || 8);
    this.currentPage.set(1);
  }

  protected previousPage(): void {
    this.currentPage.update((page) => Math.max(1, page - 1));
  }

  protected nextPage(): void {
    this.currentPage.update((page) => Math.min(this.totalPages(), page + 1));
  }

  protected statusLabel(status: LoanStatus): string {
    switch (status) {
      case 'ACTIVE': return 'Activo';
      case 'OVERDUE': return 'Vencido';
      case 'RETURNED': return 'Devuelto';
      case 'LOST': return 'Perdido';
    }
  }

  protected statusTone(status: LoanStatus): AccentTone {
    switch (status) {
      case 'ACTIVE': return 'green';
      case 'OVERDUE': return 'amber';
      case 'RETURNED': return 'blue';
      case 'LOST': return 'slate';
    }
  }

  protected formatDateTime(value: string): string {
    return new Intl.DateTimeFormat('es-MX', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(new Date(value));
  }

  protected formatDate(value: string): string {
    return new Intl.DateTimeFormat('es-MX', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).format(new Date(value));
  }

  protected dueHint(loan: LoanRecord): string {
    if (loan.status !== 'ACTIVE' && loan.status !== 'OVERDUE') {
      return loan.returnedAt ? `Devuelto ${this.formatDate(loan.returnedAt)}` : this.statusLabel(loan.status);
    }

    const today = this.dateOnly(new Date().toISOString());
    const dueDate = this.dateOnly(loan.dueAt);
    const dueTime = new Date(`${dueDate}T00:00:00`).getTime();
    const todayTime = new Date(`${today}T00:00:00`).getTime();
    const diffDays = Math.round((dueTime - todayTime) / 86400000);

    if (diffDays < 0) {
      return `${Math.abs(diffDays)} dias vencido`;
    }

    if (diffDays === 0) {
      return 'Vence hoy';
    }

    return `Vence en ${diffDays} dias`;
  }

  protected trackByLoanId = (_: number, loan: LoanRecord): string => loan.id;

  private loadLoans(): void {
    const currentUserId = this.authSession.currentUserId();

    if (!this.canManageLoans() && !currentUserId) {
      this.loading.set(false);
      this.errorMessage.set('No fue posible identificar al usuario de la sesion.');
      return;
    }

    const filters = this.appliedFilters();
    const status = this.canManageLoans() && filters.status !== 'ALL' ? filters.status : null;
    const request = this.canManageLoans()
      ? this.loansApi.listLoans({ status })
      : this.loansApi.listUserLoans(currentUserId);

    this.loading.set(true);
    this.errorMessage.set('');

    request
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (loans) => {
          this.loans.set(loans);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.resolveErrorMessage(error, 'No fue posible cargar los prestamos.'));
          this.loading.set(false);
        }
      });
  }

  private emptyFilters(): LoanFiltersForm {
    return {
      status: 'ALL',
      user: '',
      loanedFrom: '',
      loanedTo: '',
      dueFrom: '',
      dueTo: ''
    };
  }

  private isDateOnOrAfter(isoValue: string, dateValue: string): boolean {
    return !dateValue || this.dateOnly(isoValue) >= dateValue;
  }

  private isDateOnOrBefore(isoValue: string, dateValue: string): boolean {
    return !dateValue || this.dateOnly(isoValue) <= dateValue;
  }

  private dateOnly(isoValue: string): string {
    return new Date(isoValue).toISOString().split('T')[0];
  }

  private resolveErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof HttpErrorResponse) {
      const detail = error.error?.detail as string | undefined;
      const title = error.error?.title as string | undefined;
      return detail || title || fallbackMessage;
    }

    return fallbackMessage;
  }
}
