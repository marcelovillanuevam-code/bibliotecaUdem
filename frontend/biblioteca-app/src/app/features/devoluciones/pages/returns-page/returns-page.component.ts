import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReturnsApiService } from '../../../../core/services/returns-api.service';
import { ReturnCondition, ReturnRecord } from '../../../../shared/models/return.model';
import { AccentTone } from '../../../../shared/models/user.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';

@Component({
  selector: 'app-returns-page',
  imports: [PrimaryButtonComponent, StatusBadgeComponent],
  templateUrl: './returns-page.component.html',
  styleUrl: './returns-page.component.scss'
})
export class ReturnsPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly returnsApi = inject(ReturnsApiService);

  protected readonly returns = signal<ReturnRecord[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');

  constructor() {
    this.loadReturns();
  }

  protected navigateToNew(): void {
    this.router.navigateByUrl('/dashboard/devoluciones/nueva');
  }

  protected reload(): void {
    this.loadReturns();
  }

  protected conditionLabel(condition: ReturnCondition): string {
    switch (condition) {
      case 'OK': return 'OK';
      case 'DAMAGED': return 'Dañado';
      case 'LOST': return 'Perdido';
    }
  }

  protected conditionTone(condition: ReturnCondition): AccentTone {
    switch (condition) {
      case 'OK': return 'green';
      case 'DAMAGED': return 'amber';
      case 'LOST': return 'slate';
    }
  }

  protected formatDate(isoDate: string): string {
    return new Intl.DateTimeFormat('es-MX', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(new Date(isoDate));
  }

  private loadReturns(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.returnsApi
      .listReturns()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (records) => {
          this.returns.set(records);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('No fue posible cargar las devoluciones.');
          this.loading.set(false);
        }
      });
  }
}
