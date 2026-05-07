import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FinesApiService } from '../../../../core/services/fines-api.service';
import { FineConfig } from '../../../../shared/models/fine.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';

@Component({
  selector: 'app-fine-config-page',
  imports: [ReactiveFormsModule, PrimaryButtonComponent],
  templateUrl: './fine-config-page.component.html',
  styleUrl: './fine-config-page.component.scss'
})
export class FineConfigPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly finesApi = inject(FinesApiService);

  protected readonly activeConfig = signal<FineConfig | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly successMessage = signal('');

  protected readonly form = this.formBuilder.nonNullable.group({
    lateRatePerDayMxn: [5.0, [Validators.required, Validators.min(0), Validators.max(9999)]],
    damageFlatMxn: [200.0, [Validators.required, Validators.min(0), Validators.max(99999)]],
    lossFlatMxn: [500.0, [Validators.required, Validators.min(0), Validators.max(99999)]],
    effectiveFrom: ['', [Validators.required]]
  });

  constructor() {
    this.loadConfig();
  }

  protected onSubmit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.finesApi
      .saveConfig({
        lateRatePerDayMxn: value.lateRatePerDayMxn,
        damageFlatMxn: value.damageFlatMxn,
        lossFlatMxn: value.lossFlatMxn,
        effectiveFrom: value.effectiveFrom
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (config) => {
          this.activeConfig.set(config);
          this.saving.set(false);
          this.successMessage.set('Configuración actualizada correctamente.');
        },
        error: (error: unknown) => {
          this.saving.set(false);
          const detail = (error as { error?: { detail?: string } })?.error?.detail;
          this.errorMessage.set(detail ?? 'No fue posible guardar la configuración.');
        }
      });
  }

  protected formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(amount);
  }

  protected formatDate(isoDate: string): string {
    return new Intl.DateTimeFormat('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })
      .format(new Date(isoDate));
  }

  protected showFieldError(field: 'lateRatePerDayMxn' | 'damageFlatMxn' | 'lossFlatMxn' | 'effectiveFrom'): boolean {
    const control = this.form.controls[field];
    return control.invalid && (control.dirty || control.touched);
  }

  private loadConfig(): void {
    this.loading.set(true);

    this.finesApi
      .getActiveConfig()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (config) => {
          this.activeConfig.set(config);
          this.form.patchValue({
            lateRatePerDayMxn: config.lateRatePerDayMxn,
            damageFlatMxn: config.damageFlatMxn,
            lossFlatMxn: config.lossFlatMxn,
            effectiveFrom: new Date().toISOString().substring(0, 10)
          });
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        }
      });
  }
}
