import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ReturnsApiService } from '../../../../core/services/returns-api.service';
import { LoanSummary, ReturnCondition, ReturnResult } from '../../../../shared/models/return.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';

type SearchMode = 'barcode' | 'userId';
type Step = 'search' | 'form' | 'result';

@Component({
  selector: 'app-new-return-page',
  imports: [FormsModule, PrimaryButtonComponent],
  templateUrl: './new-return-page.component.html',
  styleUrl: './new-return-page.component.scss'
})
export class NewReturnPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly returnsApi = inject(ReturnsApiService);

  protected readonly step = signal<Step>('search');
  protected readonly searchMode = signal<SearchMode>('barcode');
  protected searchQuery = '';
  protected readonly searchLoading = signal(false);
  protected readonly searchError = signal('');
  protected readonly loanResults = signal<LoanSummary[]>([]);
  protected readonly selectedLoan = signal<LoanSummary | null>(null);
  protected condition: ReturnCondition = 'OK';
  protected inspectionNotes = '';
  protected readonly submitLoading = signal(false);
  protected readonly submitError = signal('');
  protected readonly returnResult = signal<ReturnResult | null>(null);

  protected get searchPlaceholder(): string {
    return this.searchMode() === 'barcode'
      ? 'Escanear o escribir código de barras...'
      : 'ID de usuario o correo electrónico...';
  }

  protected setSearchMode(mode: SearchMode): void {
    this.searchMode.set(mode);
    this.searchQuery = '';
    this.loanResults.set([]);
    this.searchError.set('');
  }

  protected onSearch(): void {
    const query = this.searchQuery.trim();
    if (!query) return;

    this.searchLoading.set(true);
    this.searchError.set('');
    this.loanResults.set([]);

    const params = this.searchMode() === 'barcode' ? { barcode: query } : { userId: query };

    this.returnsApi
      .searchActiveLoans(params)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (results) => {
          this.loanResults.set(results);
          this.searchLoading.set(false);
          if (results.length === 0) {
            this.searchError.set('No se encontraron préstamos activos con ese criterio.');
          }
        },
        error: () => {
          this.searchLoading.set(false);
          this.searchError.set('Error al buscar préstamos. Verifica el criterio e intenta de nuevo.');
        }
      });
  }

  protected selectLoan(loan: LoanSummary): void {
    this.selectedLoan.set(loan);
    this.condition = 'OK';
    this.inspectionNotes = '';
    this.submitError.set('');
    this.step.set('form');
  }

  protected backToSearch(): void {
    this.selectedLoan.set(null);
    this.step.set('search');
  }

  protected onSubmit(): void {
    const loan = this.selectedLoan();
    if (!loan || this.submitLoading()) return;

    this.submitLoading.set(true);
    this.submitError.set('');

    this.returnsApi
      .registerReturn({
        loanId: loan.id,
        condition: this.condition,
        inspectionNotes: this.inspectionNotes.trim() || null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.returnResult.set(result);
          this.submitLoading.set(false);
          this.step.set('result');
        },
        error: (error: unknown) => {
          this.submitLoading.set(false);
          const detail = (error as { error?: { detail?: string } })?.error?.detail;
          this.submitError.set(detail ?? 'No fue posible registrar la devolución. Intenta de nuevo.');
        }
      });
  }

  protected registerAnother(): void {
    this.step.set('search');
    this.searchQuery = '';
    this.loanResults.set([]);
    this.selectedLoan.set(null);
    this.returnResult.set(null);
    this.searchError.set('');
  }

  protected goToFines(): void {
    this.router.navigateByUrl('/dashboard/multas');
  }

  protected formatDate(isoDate: string): string {
    return new Intl.DateTimeFormat('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })
      .format(new Date(isoDate));
  }

  protected reasonLabel(reason: string): string {
    switch (reason) {
      case 'LATE': return 'Por retraso';
      case 'DAMAGE': return 'Por daño';
      case 'LOSS': return 'Por pérdida';
      default: return reason;
    }
  }

  protected formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(amount);
  }
}
