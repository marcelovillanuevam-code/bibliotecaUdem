import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FinesApiService } from '../../../../core/services/fines-api.service';
import { AuthSessionService } from '../../../../core/services/auth-session.service';
import { FineReason, FineRecord, FineStatus, PaymentMethod } from '../../../../shared/models/fine.model';
import { AccentTone } from '../../../../shared/models/user.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';

@Component({
  selector: 'app-fine-detail-page',
  imports: [ReactiveFormsModule, PrimaryButtonComponent, StatusBadgeComponent],
  templateUrl: './fine-detail-page.component.html',
  styleUrl: './fine-detail-page.component.scss'
})
export class FineDetailPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);
  private readonly finesApi = inject(FinesApiService);
  private readonly authSession = inject(AuthSessionService);

  protected readonly isAdmin = computed(() => this.authSession.currentUserRoleCode() === 'ADMIN');
  protected readonly canConfirmPayment = computed(() => {
    const role = this.authSession.currentUserRoleCode();
    return role === 'ADMIN' || role === 'LIBRARIAN';
  });

  protected readonly fine = signal<FineRecord | null>(null);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly paymentModalOpen = signal(false);
  protected readonly waiveModalOpen = signal(false);
  protected readonly actionLoading = signal(false);
  protected readonly actionError = signal('');

  protected readonly paymentMethods: ReadonlyArray<{ value: PaymentMethod; label: string }> = [
    { value: 'CASH', label: 'Efectivo' },
    { value: 'TRANSFER', label: 'Transferencia' },
    { value: 'CARD', label: 'Tarjeta' }
  ];

  protected readonly paymentForm = this.formBuilder.nonNullable.group({
    method: ['CASH' as PaymentMethod, [Validators.required]],
    reference: ['']
  });

  protected readonly waiveForm = this.formBuilder.nonNullable.group({
    reason: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(500)]]
  });

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadFine(id);
    } else {
      this.errorMessage.set('Identificador de multa no encontrado.');
      this.loading.set(false);
    }
  }

  protected openPaymentModal(): void {
    this.paymentForm.reset({ method: 'CASH', reference: '' });
    this.actionError.set('');
    this.paymentModalOpen.set(true);
  }

  protected closePaymentModal(): void {
    if (this.actionLoading()) return;
    this.paymentModalOpen.set(false);
  }

  protected openWaiveModal(): void {
    this.waiveForm.reset({ reason: '' });
    this.actionError.set('');
    this.waiveModalOpen.set(true);
  }

  protected closeWaiveModal(): void {
    if (this.actionLoading()) return;
    this.waiveModalOpen.set(false);
  }

  protected submitPayment(): void {
    if (this.paymentForm.invalid || this.actionLoading()) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    const fine = this.fine();
    if (!fine) return;

    const { method, reference } = this.paymentForm.getRawValue();
    this.actionLoading.set(true);
    this.actionError.set('');

    this.finesApi
      .confirmPayment(fine.id, { method, reference: reference.trim() || null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.fine.set(updated);
          this.paymentModalOpen.set(false);
          this.actionLoading.set(false);
        },
        error: (error: unknown) => {
          this.actionLoading.set(false);
          const detail = (error as { error?: { detail?: string } })?.error?.detail;
          this.actionError.set(detail ?? 'No fue posible confirmar el pago.');
        }
      });
  }

  protected submitWaive(): void {
    if (this.waiveForm.invalid || this.actionLoading()) {
      this.waiveForm.markAllAsTouched();
      return;
    }

    const fine = this.fine();
    if (!fine) return;

    const { reason } = this.waiveForm.getRawValue();
    this.actionLoading.set(true);
    this.actionError.set('');

    this.finesApi
      .waiveFine(fine.id, { reason: reason.trim() })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.fine.set(updated);
          this.waiveModalOpen.set(false);
          this.actionLoading.set(false);
        },
        error: (error: unknown) => {
          this.actionLoading.set(false);
          const detail = (error as { error?: { detail?: string } })?.error?.detail;
          this.actionError.set(detail ?? 'No fue posible condonar la multa.');
        }
      });
  }

  protected goBack(): void {
    this.router.navigateByUrl('/dashboard/multas');
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
      case 'LATE': return 'Por retraso';
      case 'DAMAGE': return 'Por daño';
      case 'LOSS': return 'Por pérdida';
    }
  }

  protected formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(amount);
  }

  protected formatDate(isoDate: string | null): string {
    if (!isoDate) return '—';
    return new Intl.DateTimeFormat('es-MX', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(new Date(isoDate));
  }

  private loadFine(id: string): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.finesApi
      .getFine(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (fine) => {
          this.fine.set(fine);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('No fue posible cargar la multa.');
          this.loading.set(false);
        }
      });
  }
}
