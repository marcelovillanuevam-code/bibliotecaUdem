import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BookCopiesApiService } from '../../../../core/services/book-copies-api.service';
import { BooksApiService } from '../../../../core/services/books-api.service';
import { LoansApiService } from '../../../../core/services/loans-api.service';
import { UsersApiService } from '../../../../core/services/users-api.service';
import { BookCopy } from '../../../../shared/models/book-copy.model';
import { BookRecord } from '../../../../shared/models/book.model';
import { UserRecord, UserRoleCode } from '../../../../shared/models/user.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';

type ToastTone = 'success' | 'error';
type NewLoanField = 'userQuery' | 'bookQuery' | 'bookCopyId' | 'dueDate';

const DEFAULT_DURATIONS_BY_ROLE: Record<UserRoleCode, number> = {
  STUDENT: 7,
  TEACHER: 21,
  LIBRARIAN: 14,
  ADMIN: 14
};

function dueDateValidator(control: AbstractControl<string>): ValidationErrors | null {
  const value = control.value;

  if (!value) {
    return null;
  }

  const today = dateInputFrom(new Date());
  const dueTime = new Date(`${value}T00:00:00`).getTime();
  const todayTime = new Date(`${today}T00:00:00`).getTime();
  const diffDays = Math.round((dueTime - todayTime) / 86400000);

  if (Number.isNaN(diffDays) || diffDays < 1) {
    return { minDate: true };
  }

  if (diffDays > 60) {
    return { maxDuration: true };
  }

  return null;
}

function dateInputFrom(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function addDays(date: Date, days: number): Date {
  const next = new Date(date);
  next.setDate(next.getDate() + days);
  return next;
}

@Component({
  selector: 'app-new-loan-page',
  imports: [ReactiveFormsModule, RouterLink, PrimaryButtonComponent],
  templateUrl: './new-loan-page.component.html',
  styleUrl: './new-loan-page.component.scss'
})
export class NewLoanPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly usersApi = inject(UsersApiService);
  private readonly booksApi = inject(BooksApiService);
  private readonly bookCopiesApi = inject(BookCopiesApiService);
  private readonly loansApi = inject(LoansApiService);

  protected readonly todayInput = dateInputFrom(new Date());
  protected readonly minDueDate = dateInputFrom(addDays(new Date(), 1));
  protected readonly users = signal<UserRecord[]>([]);
  protected readonly books = signal<BookRecord[]>([]);
  protected readonly availableCopies = signal<BookCopy[]>([]);
  protected readonly selectedUser = signal<UserRecord | null>(null);
  protected readonly selectedBook = signal<BookRecord | null>(null);
  protected readonly userQuery = signal('');
  protected readonly bookQuery = signal('');
  protected readonly loadingUsers = signal(true);
  protected readonly loadingBooks = signal(true);
  protected readonly copiesLoading = signal(false);
  protected readonly copyMessage = signal('');
  protected readonly saving = signal(false);
  protected readonly toast = signal<{ tone: ToastTone; message: string } | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    userQuery: ['', [Validators.required]],
    userId: ['', [Validators.required]],
    bookQuery: ['', [Validators.required]],
    bookId: ['', [Validators.required]],
    bookCopyId: ['', [Validators.required]],
    dueDate: [this.defaultDueDate('STUDENT'), [Validators.required, dueDateValidator]]
  });

  protected readonly userMatches = computed(() => {
    const term = this.userQuery().trim().toLowerCase();
    if (term.length < 2) return [];

    return this.users()
      .filter((user) =>
        user.username.toLowerCase().includes(term) ||
        user.email.toLowerCase().includes(term) ||
        this.fullName(user).toLowerCase().includes(term)
      )
      .slice(0, 8);
  });

  protected readonly bookMatches = computed(() => {
    const term = this.bookQuery().trim().toLowerCase();
    if (term.length < 2) return [];

    return this.books()
      .filter((book) =>
        book.title.toLowerCase().includes(term) ||
        (book.isbn ?? '').toLowerCase().includes(term)
      )
      .slice(0, 8);
  });

  protected readonly showUserMatches = computed(() => {
    const selectedUser = this.selectedUser();
    return this.userMatches().length > 0 && this.userQuery().trim() !== (selectedUser ? this.userLabel(selectedUser) : '');
  });

  protected readonly showBookMatches = computed(() => {
    const selectedBook = this.selectedBook();
    return this.bookMatches().length > 0 && this.bookQuery().trim() !== (selectedBook ? this.bookLabel(selectedBook) : '');
  });

  protected readonly selectedUserHint = computed(() => {
    const user = this.selectedUser();
    if (!user) return '';
    const days = DEFAULT_DURATIONS_BY_ROLE[user.roleCode];
    return `${user.role} - plazo sugerido: ${days} dias`;
  });

  constructor() {
    this.loadUsers();
    this.loadBooks();
  }

  protected updateUserQuery(value: string): void {
    this.userQuery.set(value);

    const selectedUser = this.selectedUser();
    if (selectedUser && value.trim() !== this.userLabel(selectedUser)) {
      this.selectedUser.set(null);
      this.form.controls.userId.setValue('');
    }
  }

  protected selectUser(user: UserRecord): void {
    const label = this.userLabel(user);
    this.selectedUser.set(user);
    this.userQuery.set(label);
    this.form.patchValue({
      userQuery: label,
      userId: user.id,
      dueDate: this.defaultDueDate(user.roleCode)
    });
    this.form.controls.userId.markAsDirty();
  }

  protected updateBookQuery(value: string): void {
    this.bookQuery.set(value);

    const selectedBook = this.selectedBook();
    if (selectedBook && value.trim() !== this.bookLabel(selectedBook)) {
      this.selectedBook.set(null);
      this.availableCopies.set([]);
      this.copyMessage.set('');
      this.form.patchValue({
        bookId: '',
        bookCopyId: ''
      });
    }
  }

  protected selectBook(book: BookRecord): void {
    const label = this.bookLabel(book);
    this.selectedBook.set(book);
    this.bookQuery.set(label);
    this.form.patchValue({
      bookQuery: label,
      bookId: book.id,
      bookCopyId: ''
    });
    this.form.controls.bookId.markAsDirty();
    this.loadCopies(book.id);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.showToast('Completa los campos requeridos antes de registrar el prestamo.', 'error');
      return;
    }

    const formValue = this.form.getRawValue();
    const durationDaysOverride = this.durationDaysUntil(formValue.dueDate);

    if (durationDaysOverride < 1 || durationDaysOverride > 60) {
      this.form.controls.dueDate.setErrors({ maxDuration: durationDaysOverride > 60, minDate: durationDaysOverride < 1 });
      return;
    }

    this.saving.set(true);
    this.toast.set(null);

    this.loansApi
      .createLoan({
        userId: formValue.userId,
        bookCopyId: formValue.bookCopyId,
        durationDaysOverride
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (loan) => {
          this.saving.set(false);
          this.router.navigate(['/dashboard/prestamos', loan.id], {
            state: { toast: 'Prestamo registrado correctamente.' }
          });
        },
        error: (error: unknown) => {
          this.saving.set(false);
          const fallback = error instanceof HttpErrorResponse && error.status === 409
            ? 'No se pudo registrar el prestamo por una regla de circulacion.'
            : 'No fue posible registrar el prestamo.';
          this.showToast(this.resolveErrorMessage(error, fallback), 'error');
        }
      });
  }

  protected dismissToast(): void {
    this.toast.set(null);
  }

  protected hasFieldError(fieldName: NewLoanField): boolean {
    const control = this.form.controls[fieldName];
    return control.invalid && (control.dirty || control.touched);
  }

  protected hasSelectionError(controlName: 'userId' | 'bookId'): boolean {
    const control = this.form.controls[controlName];
    const queryControl = controlName === 'userId'
      ? this.form.controls.userQuery
      : this.form.controls.bookQuery;

    return control.invalid && (queryControl.dirty || queryControl.touched || control.dirty || control.touched);
  }

  protected fieldErrorMessage(fieldName: NewLoanField): string {
    const control = this.form.controls[fieldName];

    if (control.hasError('required')) {
      return 'Este campo es obligatorio.';
    }

    if (control.hasError('minDate')) {
      return 'La fecha debe ser posterior a hoy.';
    }

    if (control.hasError('maxDuration')) {
      return 'El plazo no puede exceder 60 dias.';
    }

    return 'Revisa este valor.';
  }

  protected userLabel(user: UserRecord): string {
    return `${user.username} - ${user.email}`;
  }

  protected bookLabel(book: BookRecord): string {
    return `${book.title}${book.isbn ? ` - ISBN ${book.isbn}` : ''}`;
  }

  protected fullName(user: UserRecord): string {
    return `${user.firstName} ${user.lastName}`.trim();
  }

  protected copyLabel(copy: BookCopy): string {
    const location = copy.locationName ? ` - ${copy.locationName}` : '';
    return `${copy.barcode}${location}`;
  }

  private loadUsers(): void {
    this.loadingUsers.set(true);

    this.usersApi
      .listUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => {
          this.users.set(users);
          this.loadingUsers.set(false);
        },
        error: (error: unknown) => {
          this.loadingUsers.set(false);
          this.showToast(this.resolveErrorMessage(error, 'No fue posible cargar usuarios.'), 'error');
        }
      });
  }

  private loadBooks(): void {
    this.loadingBooks.set(true);

    this.booksApi
      .listBooks({})
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (books) => {
          this.books.set(books);
          this.loadingBooks.set(false);
        },
        error: (error: unknown) => {
          this.loadingBooks.set(false);
          this.showToast(this.resolveErrorMessage(error, 'No fue posible cargar libros.'), 'error');
        }
      });
  }

  private loadCopies(bookId: string): void {
    this.copiesLoading.set(true);
    this.copyMessage.set('');
    this.availableCopies.set([]);

    this.bookCopiesApi
      .listByBook(bookId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (copies) => {
          if (this.selectedBook()?.id !== bookId) return;

          const availableCopies = copies
            .filter((copy) => copy.status === 'AVAILABLE')
            .sort((left, right) => left.barcode.localeCompare(right.barcode, 'es'));
          this.availableCopies.set(availableCopies);
          this.copyMessage.set(
            availableCopies.length > 0
              ? `${availableCopies.length} ejemplares disponibles`
              : 'Este libro no tiene ejemplares disponibles.'
          );
          this.copiesLoading.set(false);
        },
        error: (error: unknown) => {
          this.copyMessage.set(this.resolveErrorMessage(error, 'No fue posible cargar ejemplares.'));
          this.copiesLoading.set(false);
        }
      });
  }

  private defaultDueDate(roleCode: UserRoleCode): string {
    return dateInputFrom(addDays(new Date(), DEFAULT_DURATIONS_BY_ROLE[roleCode]));
  }

  private durationDaysUntil(dateInput: string): number {
    const dueTime = new Date(`${dateInput}T00:00:00`).getTime();
    const todayTime = new Date(`${this.todayInput}T00:00:00`).getTime();
    return Math.round((dueTime - todayTime) / 86400000);
  }

  private showToast(message: string, tone: ToastTone): void {
    this.toast.set({ message, tone });
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
