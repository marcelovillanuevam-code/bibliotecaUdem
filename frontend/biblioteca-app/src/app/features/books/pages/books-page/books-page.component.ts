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
import { ActivatedRoute } from '@angular/router';
import { BooksApiService } from '../../../../core/services/books-api.service';
import { BookCopiesApiService } from '../../../../core/services/book-copies-api.service';
import { BookDetail, BookFilters, BookRecord, BookSaveRequest } from '../../../../shared/models/book.model';
import { BookCopy, BookCopyCondition, BookCopySaveRequest, BookCopyStatus, BookCopyUpdateRequest } from '../../../../shared/models/book-copy.model';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';
import { SearchInputComponent } from '../../../../shared/ui/search-input/search-input.component';

type BooksPageMode = 'catalog' | 'manage';
type BookEditorMode = 'create' | 'edit';
type BookFormField =
  | 'title'
  | 'subtitle'
  | 'isbn'
  | 'publisher'
  | 'publicationYear'
  | 'edition'
  | 'language'
  | 'authorsRaw'
  | 'subjectCodesRaw'
  | 'summaryJson'
  | 'metadataJson';
type CopyFormField = 'barcode' | 'status' | 'condition' | 'acquiredAt';

function jsonStringValidator(control: AbstractControl<string>): ValidationErrors | null {
  const value = control.value.trim();

  if (!value) {
    return null;
  }

  try {
    JSON.parse(value);
    return null;
  } catch {
    return { invalidJson: true };
  }
}

function authorsValidator(control: AbstractControl<string>): ValidationErrors | null {
  const authors = control.value
    .split(/\r?\n/)
    .map((entry) => entry.trim())
    .filter((entry) => entry.length > 0);

  return authors.length > 0 ? null : { invalidAuthors: true };
}

function subjectCodesValidator(control: AbstractControl<string>): ValidationErrors | null {
  const subjects = control.value
    .split(',')
    .map((entry) => entry.trim())
    .filter((entry) => entry.length > 0);

  return subjects.length > 0 ? null : { invalidSubjects: true };
}

function noWhitespaceOnly(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > 0 ? null : { whitespaceOnly: true };
}

@Component({
  selector: 'app-books-page',
  imports: [ReactiveFormsModule, PrimaryButtonComponent, SearchInputComponent],
  templateUrl: './books-page.component.html',
  styleUrl: './books-page.component.scss'
})
export class BooksPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private readonly booksApi = inject(BooksApiService);
  private readonly bookCopiesApi = inject(BookCopiesApiService);
  private readonly mode = (this.route.snapshot.data['mode'] as BooksPageMode | undefined) ?? 'catalog';

  protected readonly isManageMode = this.mode === 'manage';
  protected readonly pageTitle = this.isManageMode ? 'Gestion de Libros' : 'Catalogo de Libros';
  protected readonly pageDescription = this.isManageMode
    ? 'Administra altas, cambios, bajas y consulta detallada del catalogo.'
    : 'Explora el catalogo institucional usando filtros por autor, materia, ISBN y mas.';

  // — Lista principal —
  protected readonly books = signal<BookRecord[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly successMessage = signal('');
  protected readonly deletingBookId = signal<string | null>(null);

  // — Detalle de libro —
  protected readonly detailOpen = signal(false);
  protected readonly detailLoading = signal(false);
  protected readonly detailErrorMessage = signal('');
  protected readonly selectedBook = signal<BookDetail | null>(null);

  // — Editor de libro —
  protected readonly editorMode = signal<BookEditorMode | null>(null);
  protected readonly editorLoading = signal(false);
  protected readonly editorSaving = signal(false);
  protected readonly editorErrorMessage = signal('');
  protected readonly editingBookId = signal<string | null>(null);
  protected readonly editorActionLabel = computed(() => {
    if (this.editorSaving()) {
      return this.editorMode() === 'create' ? 'Registrando...' : 'Guardando...';
    }

    return this.editorMode() === 'create' ? 'Registrar libro' : 'Guardar cambios';
  });

  // — Ejemplares —
  protected readonly copiesOpen = signal(false);
  protected readonly copiesBookId = signal<string | null>(null);
  protected readonly copiesBookTitle = computed(() => {
    const id = this.copiesBookId();
    return id ? (this.books().find(b => b.id === id)?.title ?? '') : '';
  });
  protected readonly copiesLoading = signal(false);
  protected readonly copies = signal<BookCopy[]>([]);
  protected readonly copiesErrorMessage = signal('');

  // — Editor de ejemplar —
  protected readonly copyEditorMode = signal<'create' | 'edit' | null>(null);
  protected readonly copyEditorSaving = signal(false);
  protected readonly copyEditorErrorMessage = signal('');
  protected readonly editingCopyId = signal<string | null>(null);
  protected readonly copyEditorActionLabel = computed(() =>
    this.copyEditorSaving()
      ? (this.copyEditorMode() === 'create' ? 'Agregando...' : 'Guardando...')
      : (this.copyEditorMode() === 'create' ? 'Agregar ejemplar' : 'Guardar cambios')
  );

  // — Formularios —
  protected readonly filterForm = this.formBuilder.nonNullable.group({
    title: [''],
    author: [''],
    subject: [''],
    isbn: [''],
    publisher: [''],
    language: ['']
  });

  protected readonly editorForm = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(500)]],
    subtitle: ['', [Validators.maxLength(500)]],
    isbn: ['', [Validators.maxLength(50)]],
    publisher: ['', [Validators.maxLength(255)]],
    publicationYear: [''],
    edition: ['', [Validators.maxLength(100)]],
    language: ['es', [Validators.required, Validators.maxLength(50)]],
    authorsRaw: ['', [authorsValidator]],
    subjectCodesRaw: ['', [subjectCodesValidator]],
    summaryJson: ['', [jsonStringValidator]],
    metadataJson: ['', [jsonStringValidator]]
  });

  protected readonly copyForm = this.formBuilder.nonNullable.group({
    barcode: ['', [Validators.required, Validators.maxLength(100), noWhitespaceOnly]],
    status: ['AVAILABLE', [Validators.required]],
    condition: [''],
    acquiredAt: [new Date().toISOString().split('T')[0], [Validators.required]]
  });

  constructor() {
    this.loadBooks();
  }

  // ——— Libros: filtros y lista ———

  protected applyFilters(): void {
    this.loadBooks();
  }

  protected resetFilters(): void {
    this.filterForm.reset({
      title: '',
      author: '',
      subject: '',
      isbn: '',
      publisher: '',
      language: ''
    });
    this.loadBooks();
  }

  protected reload(): void {
    this.loadBooks();
  }

  // ——— Detalle de libro ———

  protected openDetail(bookId: string): void {
    this.detailOpen.set(true);
    this.detailLoading.set(true);
    this.detailErrorMessage.set('');
    this.selectedBook.set(null);

    this.booksApi
      .getBook(bookId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (book) => {
          this.selectedBook.set(book);
          this.detailLoading.set(false);
        },
        error: (error: unknown) => {
          this.detailErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible consultar el libro seleccionado.'));
          this.detailLoading.set(false);
        }
      });
  }

  protected closeDetail(): void {
    this.detailOpen.set(false);
    this.detailLoading.set(false);
    this.detailErrorMessage.set('');
    this.selectedBook.set(null);
  }

  // ——— Editor de libro ———

  protected openCreateEditor(): void {
    if (!this.isManageMode || this.editorSaving()) {
      return;
    }

    this.editorMode.set('create');
    this.editingBookId.set(null);
    this.editorLoading.set(false);
    this.editorErrorMessage.set('');
    this.successMessage.set('');
    this.editorForm.enable();
    this.editorForm.reset(this.emptyEditorValue());
  }

  protected openEditEditor(bookId: string): void {
    if (!this.isManageMode || this.editorSaving()) {
      return;
    }

    this.editorMode.set('edit');
    this.editingBookId.set(bookId);
    this.editorLoading.set(true);
    this.editorErrorMessage.set('');
    this.successMessage.set('');
    this.editorForm.disable();
    this.editorForm.reset(this.emptyEditorValue());

    this.booksApi
      .getBook(bookId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (book) => {
          if (this.editingBookId() !== book.id || this.editorMode() !== 'edit') {
            return;
          }

          this.editorForm.enable();
          this.editorForm.reset({
            title: book.title,
            subtitle: book.subtitle ?? '',
            isbn: book.isbn ?? '',
            publisher: book.publisher ?? '',
            publicationYear: book.publicationYear?.toString() ?? '',
            edition: book.edition ?? '',
            language: book.language,
            authorsRaw: this.serializeAuthors(book),
            subjectCodesRaw: book.subjects.map((subject) => subject.code).join(', '),
            summaryJson: book.summaryJson ?? '',
            metadataJson: book.metadataJson ?? ''
          });
          this.editorLoading.set(false);
        },
        error: (error: unknown) => {
          this.editorForm.enable();
          this.editorLoading.set(false);
          this.editorErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible cargar el libro para edicion.'));
        }
      });
  }

  protected closeEditor(): void {
    if (this.editorSaving()) {
      return;
    }

    this.editorMode.set(null);
    this.editingBookId.set(null);
    this.editorLoading.set(false);
    this.editorErrorMessage.set('');
    this.editorForm.enable();
    this.editorForm.reset(this.emptyEditorValue());
  }

  protected submitEditor(): void {
    const mode = this.editorMode();

    if (!mode || this.editorLoading() || this.editorSaving()) {
      return;
    }

    if (this.editorForm.invalid) {
      this.editorForm.markAllAsTouched();
      return;
    }

    const payload = this.buildSaveRequest();
    this.editorSaving.set(true);
    this.editorErrorMessage.set('');

    if (mode === 'create') {
      this.booksApi
        .createBook(payload)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (book) => {
            this.editorSaving.set(false);
            this.upsertBook(book);
            this.successMessage.set('El libro se registro correctamente.');
            this.closeEditor();
          },
          error: (error: unknown) => {
            this.editorSaving.set(false);
            this.editorErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible registrar el libro.'));
          }
        });

      return;
    }

    const bookId = this.editingBookId();
    if (!bookId) {
      this.editorSaving.set(false);
      this.editorErrorMessage.set('No se encontro el libro que se intenta actualizar.');
      return;
    }

    this.booksApi
      .updateBook(bookId, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (book) => {
          this.editorSaving.set(false);
          this.upsertBook(book);
          if (this.selectedBook()?.id === book.id) {
            this.selectedBook.set(book);
          }

          this.successMessage.set('El libro se actualizo correctamente.');
          this.closeEditor();
        },
        error: (error: unknown) => {
          this.editorSaving.set(false);
          this.editorErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible actualizar el libro.'));
        }
      });
  }

  protected removeBook(book: BookRecord): void {
    if (!this.isManageMode) {
      return;
    }

    const confirmed = window.confirm(`Se dara de baja el libro "${book.title}". Deseas continuar?`);

    if (!confirmed || this.deletingBookId() === book.id) {
      return;
    }

    this.deletingBookId.set(book.id);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.booksApi
      .deleteBook(book.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.deletingBookId.set(null);
          this.books.update((books) => books.filter((currentBook) => currentBook.id !== book.id));
          if (this.selectedBook()?.id === book.id) {
            this.closeDetail();
          }

          this.successMessage.set('El libro se dio de baja correctamente.');
        },
        error: (error: unknown) => {
          this.deletingBookId.set(null);
          this.errorMessage.set(this.resolveErrorMessage(error, 'No fue posible dar de baja el libro seleccionado.'));
        }
      });
  }

  // ——— Ejemplares ———

  protected openCopies(bookId: string): void {
    if (!this.isManageMode) return;
    this.copiesOpen.set(true);
    this.copiesBookId.set(bookId);
    this.copiesErrorMessage.set('');
    this.closeCopyEditor();
    this.loadCopies();
  }

  protected closeCopies(): void {
    this.copiesOpen.set(false);
    this.copiesBookId.set(null);
    this.copies.set([]);
    this.copiesErrorMessage.set('');
    this.closeCopyEditor();
  }

  protected reloadCopies(): void {
    this.copiesErrorMessage.set('');
    this.loadCopies();
  }

  protected openCopyCreate(): void {
    if (this.copyEditorSaving()) return;
    this.copyEditorMode.set('create');
    this.editingCopyId.set(null);
    this.copyEditorErrorMessage.set('');
    this.copyForm.enable();
    this.copyForm.reset({
      barcode: '',
      status: 'AVAILABLE',
      condition: '',
      acquiredAt: new Date().toISOString().split('T')[0]
    });
  }

  protected openCopyEdit(copy: BookCopy): void {
    if (this.copyEditorSaving()) return;
    this.copyEditorMode.set('edit');
    this.editingCopyId.set(copy.id);
    this.copyEditorErrorMessage.set('');
    this.copyForm.enable();
    this.copyForm.reset({
      barcode: copy.barcode,
      status: copy.status,
      condition: copy.condition ?? '',
      acquiredAt: copy.acquiredAt.split('T')[0]
    });
    this.copyForm.controls.barcode.disable();
    this.copyForm.controls.acquiredAt.disable();
  }

  protected closeCopyEditor(): void {
    if (this.copyEditorSaving()) return;
    this.copyEditorMode.set(null);
    this.editingCopyId.set(null);
    this.copyEditorErrorMessage.set('');
    this.copyForm.enable();
    this.copyForm.reset({
      barcode: '',
      status: 'AVAILABLE',
      condition: '',
      acquiredAt: new Date().toISOString().split('T')[0]
    });
  }

  protected submitCopyEditor(): void {
    const mode = this.copyEditorMode();
    const bookId = this.copiesBookId();
    if (!mode || !bookId || this.copyEditorSaving()) return;

    if (this.copyForm.invalid) {
      this.copyForm.markAllAsTouched();
      return;
    }

    this.copyEditorSaving.set(true);
    this.copyEditorErrorMessage.set('');
    const formValue = this.copyForm.getRawValue();

    if (mode === 'create') {
      const request: BookCopySaveRequest = {
        barcode: formValue.barcode.trim(),
        status: (formValue.status as BookCopyStatus) || 'AVAILABLE',
        condition: formValue.condition ? (formValue.condition as BookCopyCondition) : undefined,
        acquiredAt: formValue.acquiredAt
      };

      this.bookCopiesApi
        .create(bookId, request)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.copyEditorSaving.set(false);
            this.closeCopyEditor();
            this.loadCopies();
            this.refreshBookCounts(bookId);
          },
          error: (error: unknown) => {
            this.copyEditorSaving.set(false);
            this.copyEditorErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible agregar el ejemplar.'));
          }
        });
      return;
    }

    const copyId = this.editingCopyId();
    if (!copyId) {
      this.copyEditorSaving.set(false);
      return;
    }

    const updateRequest: BookCopyUpdateRequest = {
      status: formValue.status as BookCopyStatus,
      condition: formValue.condition ? (formValue.condition as BookCopyCondition) : undefined
    };

    this.bookCopiesApi
      .update(copyId, updateRequest)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.copyEditorSaving.set(false);
          this.closeCopyEditor();
          this.loadCopies();
          this.refreshBookCounts(bookId);
        },
        error: (error: unknown) => {
          this.copyEditorSaving.set(false);
          this.copyEditorErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible actualizar el ejemplar.'));
        }
      });
  }

  protected removeCopy(copy: BookCopy): void {
    const bookId = this.copiesBookId();
    if (!bookId) return;

    const confirmed = window.confirm(`Se dara de baja el ejemplar con codigo "${copy.barcode}". ¿Deseas continuar?`);
    if (!confirmed) return;

    this.bookCopiesApi
      .delete(copy.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loadCopies();
          this.refreshBookCounts(bookId);
        },
        error: (error: unknown) => {
          this.copiesErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible dar de baja el ejemplar.'));
        }
      });
  }

  // ——— Helpers de UI ———

  protected dismissSuccessMessage(): void {
    this.successMessage.set('');
  }

  protected hasFieldError(fieldName: BookFormField): boolean {
    const control = this.editorForm.controls[fieldName];
    return control.invalid && (control.dirty || control.touched);
  }

  protected fieldErrorMessage(fieldName: BookFormField): string {
    const control = this.editorForm.controls[fieldName];

    if (control.hasError('required')) {
      return 'Este campo es obligatorio.';
    }

    if (control.hasError('maxlength')) {
      const maxLength = control.getError('maxlength')?.requiredLength as number | undefined;
      return `No puede exceder ${maxLength ?? 0} caracteres.`;
    }

    if (control.hasError('invalidJson')) {
      return 'Captura un JSON valido o deja el campo vacio.';
    }

    if (control.hasError('invalidAuthors')) {
      return 'Agrega al menos un autor. Usa una linea por autor.';
    }

    if (control.hasError('invalidSubjects')) {
      return 'Agrega al menos una materia separada por comas.';
    }

    return 'Revisa este valor.';
  }

  protected hasCopyFieldError(fieldName: CopyFormField): boolean {
    const control = this.copyForm.controls[fieldName];
    return control.invalid && (control.dirty || control.touched);
  }

  protected copyFieldErrorMessage(fieldName: CopyFormField): string {
    const control = this.copyForm.controls[fieldName];

    if (control.hasError('required')) {
      return 'Este campo es obligatorio.';
    }

    if (control.hasError('maxlength')) {
      const maxLength = control.getError('maxlength')?.requiredLength as number | undefined;
      return `No puede exceder ${maxLength ?? 0} caracteres.`;
    }

    if (control.hasError('whitespaceOnly')) {
      return 'El codigo no puede ser solo espacios en blanco.';
    }

    return 'Revisa este valor.';
  }

  protected copyStatusLabel(status: string): string {
    switch (status) {
      case 'AVAILABLE': return 'Disponible';
      case 'MAINTENANCE': return 'Mantenimiento';
      case 'LOST': return 'Perdido';
      case 'RETIRED': return 'Retirado';
      case 'LOANED': return 'Prestado';
      case 'RESERVED': return 'Reservado';
      default: return status;
    }
  }

  protected copyConditionLabel(condition: string | null): string {
    switch (condition) {
      case 'NEW': return 'Nuevo';
      case 'GOOD': return 'Bueno';
      case 'WORN': return 'Desgastado';
      case 'DAMAGED': return 'Danado';
      default: return '—';
    }
  }

  protected formatAuthors(book: BookRecord | BookDetail): string {
    return book.authors.map((author) => author.fullName).join(', ');
  }

  protected formatSubjects(book: BookRecord | BookDetail): string {
    return book.subjects.map((subject) => subject.name).join(', ');
  }

  protected formatBookMeta(book: BookRecord): string {
    const parts = [
      book.isbn ? `ISBN ${book.isbn}` : null,
      book.publisher,
      book.publicationYear ? book.publicationYear.toString() : null,
      book.language.toUpperCase()
    ].filter((part): part is string => !!part);

    return parts.join(' · ');
  }

  protected formatTimestamp(value: string): string {
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

  protected locationLabel(book: BookDetail): string {
    const metadata = this.tryParseJson(book.metadataJson);
    if (!metadata) {
      return 'Ubicacion no registrada';
    }

    const nestedLocation = metadata['location'];
    const location =
      typeof nestedLocation === 'object' && nestedLocation !== null
        ? (nestedLocation as Record<string, unknown>)
        : metadata;
    const libraryName = typeof location['libraryName'] === 'string' ? location['libraryName'] : null;
    const section = typeof location['section'] === 'string' ? location['section'] : null;
    const shelf = typeof location['shelf'] === 'string' ? location['shelf'] : null;
    const notes = typeof location['notes'] === 'string' ? location['notes'] : null;

    const parts = [libraryName, section, shelf, notes].filter((part): part is string => !!part?.trim());
    return parts.length > 0 ? parts.join(' · ') : 'Ubicacion no registrada';
  }

  protected isDeleting(bookId: string): boolean {
    return this.deletingBookId() === bookId;
  }

  // ——— Privados ———

  private loadBooks(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.booksApi
      .listBooks(this.buildFilters())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (books) => {
          this.books.set(this.sortBooks(books));
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.resolveErrorMessage(error, 'No fue posible cargar los libros desde la API.'));
          this.loading.set(false);
        }
      });
  }

  private loadCopies(): void {
    const bookId = this.copiesBookId();
    if (!bookId) return;

    this.copiesLoading.set(true);
    this.copiesErrorMessage.set('');

    this.bookCopiesApi
      .listByBook(bookId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (copies) => {
          this.copies.set(copies);
          this.copiesLoading.set(false);
        },
        error: (error: unknown) => {
          this.copiesErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible cargar los ejemplares.'));
          this.copiesLoading.set(false);
        }
      });
  }

  private refreshBookCounts(bookId: string): void {
    this.booksApi
      .getBook(bookId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (book) => this.upsertBook(book),
        error: () => { /* refresh silencioso; se verá en el próximo reload de lista */ }
      });
  }

  private buildFilters(): BookFilters {
    const filters = this.filterForm.getRawValue();

    return {
      title: filters.title,
      author: filters.author,
      subject: filters.subject,
      isbn: filters.isbn,
      publisher: filters.publisher,
      language: filters.language
    };
  }

  private buildSaveRequest(): BookSaveRequest {
    const formValue = this.editorForm.getRawValue();
    const publicationYear = formValue.publicationYear.trim();

    return {
      title: formValue.title.trim(),
      subtitle: this.normalizeOptional(formValue.subtitle),
      isbn: this.normalizeOptional(formValue.isbn),
      publisher: this.normalizeOptional(formValue.publisher),
      publicationYear: publicationYear ? Number(publicationYear) : null,
      edition: this.normalizeOptional(formValue.edition),
      language: formValue.language.trim() || 'es',
      summaryJson: this.normalizeOptional(formValue.summaryJson),
      metadataJson: this.normalizeOptional(formValue.metadataJson),
      authors: formValue.authorsRaw
        .split(/\r?\n/)
        .map((line) => line.trim())
        .filter((line) => line.length > 0)
        .map((line) => {
          const [fullName, contribution] = line.split('|').map((part) => part.trim());
          return {
            fullName,
            contribution: contribution || null
          };
        }),
      subjectCodes: formValue.subjectCodesRaw
        .split(',')
        .map((subjectCode) => subjectCode.trim().toUpperCase())
        .filter((subjectCode) => subjectCode.length > 0)
    };
  }

  private emptyEditorValue() {
    return {
      title: '',
      subtitle: '',
      isbn: '',
      publisher: '',
      publicationYear: '',
      edition: '',
      language: 'es',
      authorsRaw: '',
      subjectCodesRaw: '',
      summaryJson: '',
      metadataJson: ''
    };
  }

  private serializeAuthors(book: BookDetail): string {
    return book.authors
      .map((author) =>
        author.contribution ? `${author.fullName} | ${author.contribution}` : author.fullName
      )
      .join('\n');
  }

  private normalizeOptional(value: string): string | null {
    const normalizedValue = value.trim();
    return normalizedValue ? normalizedValue : null;
  }

  private upsertBook(book: BookDetail): void {
    const record: BookRecord = {
      id: book.id,
      title: book.title,
      subtitle: book.subtitle,
      isbn: book.isbn,
      publisher: book.publisher,
      publicationYear: book.publicationYear,
      edition: book.edition,
      language: book.language,
      authors: [...book.authors],
      subjects: [...book.subjects],
      createdAt: book.createdAt,
      updatedAt: book.updatedAt,
      totalCopies: book.totalCopies,
      availableCopies: book.availableCopies
    };

    this.books.update((books) =>
      this.sortBooks([...books.filter((currentBook) => currentBook.id !== book.id), record])
    );
  }

  private sortBooks(books: ReadonlyArray<BookRecord>): BookRecord[] {
    return [...books].sort((left, right) =>
      left.title.localeCompare(right.title, 'es', { sensitivity: 'base' })
    );
  }

  private tryParseJson(jsonValue: string | null): Record<string, unknown> | null {
    if (!jsonValue) {
      return null;
    }

    try {
      const parsedValue = JSON.parse(jsonValue) as unknown;
      return typeof parsedValue === 'object' && parsedValue !== null
        ? (parsedValue as Record<string, unknown>)
        : null;
    } catch {
      return null;
    }
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
