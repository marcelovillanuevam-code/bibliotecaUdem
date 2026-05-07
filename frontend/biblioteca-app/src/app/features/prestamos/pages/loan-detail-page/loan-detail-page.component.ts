import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthSessionService } from '../../../../core/services/auth-session.service';
import { LoansApiService } from '../../../../core/services/loans-api.service';
import { LoanRecord, LoanStatus } from '../../../../shared/models/loan.model';
import { AccentTone } from '../../../../shared/models/user.model';
import { resolveHttpError } from '../../../../shared/utils/http-error';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';

type ToastTone = 'success' | 'error';

@Component({
  selector: 'app-loan-detail-page',
  imports: [RouterLink, PrimaryButtonComponent, StatusBadgeComponent],
  templateUrl: './loan-detail-page.component.html',
  styleUrl: './loan-detail-page.component.scss'
})
export class LoanDetailPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authSession = inject(AuthSessionService);
  private readonly loansApi = inject(LoansApiService);
  private readonly loanId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly loan = signal<LoanRecord | null>(null);
  protected readonly loading = signal(true);
  protected readonly renewing = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly toast = signal<{ tone: ToastTone; message: string } | null>(this.initialToast());

  protected readonly canRenew = computed(() => {
    const loan = this.loan();
    if (!loan) return false;

    const isOwner = loan.userId === this.authSession.currentUserId();
    const isStaff = this.authSession.hasAnyRole(['LIBRARIAN', 'ADMIN']);
    return (isOwner || isStaff) && loan.status === 'ACTIVE' && loan.renewalCount < 2;
  });

  constructor() {
    this.loadLoan();
  }

  protected renew(): void {
    const loan = this.loan();

    if (!loan || !this.canRenew() || this.renewing()) {
      return;
    }

    this.renewing.set(true);
    this.toast.set(null);

    this.loansApi
      .renewLoan(loan.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedLoan) => {
          this.loan.set(updatedLoan);
          this.renewing.set(false);
          this.showToast('Prestamo renovado correctamente.', 'success');
        },
        error: (error: unknown) => {
          this.renewing.set(false);
          this.showToast(resolveHttpError(error, 'No fue posible renovar el prestamo.'), 'error');
        }
      });
  }

  protected reload(): void {
    this.loadLoan();
  }

  protected dismissToast(): void {
    this.toast.set(null);
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

  protected renewalLimitText(loan: LoanRecord): string {
    return `${loan.renewalCount} de 2 renovaciones usadas`;
  }

  private loadLoan(): void {
    if (!this.loanId) {
      this.loading.set(false);
      this.errorMessage.set('No se encontro el identificador del prestamo.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.loansApi
      .getLoan(this.loanId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (loan) => {
          this.loan.set(loan);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(resolveHttpError(error, 'No fue posible cargar el prestamo.'));
          this.loading.set(false);
        }
      });
  }

  private showToast(message: string, tone: ToastTone): void {
    this.toast.set({ message, tone });
  }

  private initialToast(): { tone: ToastTone; message: string } | null {
    const navigationState = this.router.getCurrentNavigation()?.extras.state as { toast?: string } | undefined;
    const historyState = typeof history !== 'undefined' ? (history.state as { toast?: string }) : undefined;
    const message = navigationState?.toast ?? historyState?.toast;

    return message ? { tone: 'success', message } : null;
  }

}

