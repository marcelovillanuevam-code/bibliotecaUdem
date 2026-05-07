import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthSessionService } from '../../../../core/services/auth-session.service';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly authSession = inject(AuthSessionService);

  protected readonly pending = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly form = this.formBuilder.nonNullable.group({
    username: ['admin1', [Validators.required, Validators.minLength(3)]],
    password: ['Admin123!', [Validators.required, Validators.minLength(8)]]
  });

  protected submit(): void {
    if (this.form.invalid || this.pending()) {
      this.form.markAllAsTouched();
      return;
    }

    this.pending.set(true);
    this.errorMessage.set('');

    this.authSession
      .login(this.form.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.pending.set(false);
          void this.router.navigateByUrl('/dashboard');
        },
        error: (error: unknown) => {
          this.pending.set(false);
          this.errorMessage.set(this.resolveErrorMessage(error));
        }
      });
  }

  protected fillDemoCredentials(): void {
    this.form.setValue({
      username: 'admin1',
      password: 'Admin123!'
    });
  }

  protected hasError(fieldName: 'username' | 'password'): boolean {
    const control = this.form.controls[fieldName];
    return control.invalid && (control.dirty || control.touched);
  }

  private resolveErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const detail = error.error?.detail as string | undefined;
      const title = error.error?.title as string | undefined;
      return detail || title || 'No fue posible iniciar sesion con la API.';
    }

    return 'No fue posible iniciar sesion con la API.';
  }
}
