import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AccentTone,
  UserCreateRequest,
  UserRecord,
  UserRole,
  UserRoleCode,
  UserSaveRequestBase,
  UserStatus,
  UserStatusCode,
  UserUpdateRequest
} from '../../../../shared/models/user.model';
import { UsersApiService } from '../../../../core/services/users-api.service';
import { AvatarChipComponent } from '../../../../shared/ui/avatar-chip/avatar-chip.component';
import { FilterSelectComponent } from '../../../../shared/ui/filter-select/filter-select.component';
import { PrimaryButtonComponent } from '../../../../shared/ui/primary-button/primary-button.component';
import { SearchInputComponent } from '../../../../shared/ui/search-input/search-input.component';
import { StatusBadgeComponent } from '../../../../shared/ui/status-badge/status-badge.component';

type UserEditorMode = 'create' | 'edit';
type UserFormField =
  | 'username'
  | 'firstName'
  | 'lastName'
  | 'displayName'
  | 'email'
  | 'password'
  | 'preferredLocale'
  | 'documentType'
  | 'documentNumber';

@Component({
  selector: 'app-users-page',
  imports: [
    ReactiveFormsModule,
    AvatarChipComponent,
    FilterSelectComponent,
    PrimaryButtonComponent,
    SearchInputComponent,
    StatusBadgeComponent
  ],
  templateUrl: './users-page.component.html',
  styleUrl: './users-page.component.scss'
})
export class UsersPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly usersApi = inject(UsersApiService);

  protected readonly roleFilterOptions: ReadonlyArray<'Todos los roles' | UserRole> = [
    'Todos los roles',
    'Administrador',
    'Bibliotecario',
    'Profesor',
    'Estudiante'
  ];
  protected readonly roleOptions: ReadonlyArray<{ value: UserRoleCode; label: UserRole }> = [
    { value: 'STUDENT', label: 'Estudiante' },
    { value: 'LIBRARIAN', label: 'Bibliotecario' },
    { value: 'TEACHER', label: 'Profesor' },
    { value: 'ADMIN', label: 'Administrador' }
  ];
  protected readonly statusOptions: ReadonlyArray<{ value: UserStatusCode; label: UserStatus }> = [
    { value: 'active', label: 'Activo' },
    { value: 'pending_verification', label: 'Pendiente' },
    { value: 'inactive', label: 'Inactivo' },
    { value: 'suspended', label: 'Suspendido' }
  ];

  protected readonly users = signal<UserRecord[]>([]);
  protected readonly loading = signal(true);
  protected readonly deletingUserId = signal<string | null>(null);
  protected readonly errorMessage = signal('');
  protected readonly successMessage = signal('');
  protected readonly searchTerm = signal('');
  protected readonly selectedRole = signal<'Todos los roles' | UserRole>('Todos los roles');
  protected readonly editorMode = signal<UserEditorMode | null>(null);
  protected readonly editorLoading = signal(false);
  protected readonly editorSaving = signal(false);
  protected readonly editorErrorMessage = signal('');
  protected readonly editingUserId = signal<string | null>(null);
  protected readonly editorUser = signal<UserRecord | null>(null);
  protected readonly editorTitle = computed(() =>
    this.editorMode() === 'create' ? 'Agregar usuario' : 'Editar usuario'
  );
  protected readonly editorDescription = computed(() =>
    this.editorMode() === 'create'
      ? 'Registra un nuevo perfil y asigna rol, estado y datos de contacto.'
      : 'Actualiza la informacion del usuario seleccionado y guarda los cambios.'
  );
  protected readonly editorActionLabel = computed(() => {
    if (this.editorSaving()) {
      return this.editorMode() === 'create' ? 'Creando...' : 'Guardando...';
    }

    return this.editorMode() === 'create' ? 'Crear usuario' : 'Guardar cambios';
  });
  protected readonly form = this.formBuilder.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    displayName: ['', [Validators.maxLength(160)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
    password: ['', [Validators.minLength(8), Validators.maxLength(100)]],
    roleCode: ['STUDENT' as UserRoleCode, [Validators.required]],
    statusCode: ['active' as UserStatusCode, [Validators.required]],
    preferredLocale: ['es_MX', [Validators.maxLength(10)]],
    documentType: ['', [Validators.maxLength(40)]],
    documentNumber: ['', [Validators.maxLength(80)]]
  });

  protected readonly filteredUsers = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const role = this.selectedRole();

    return this.users().filter((user) => {
      const matchesRole = role === 'Todos los roles' || user.role === role;
      const matchesSearch =
        term.length === 0 ||
        `${user.firstName} ${user.lastName}`.toLowerCase().includes(term) ||
        user.email.toLowerCase().includes(term) ||
        user.universityId.toLowerCase().includes(term);

      return matchesRole && matchesSearch;
    });
  });

  constructor() {
    this.loadUsers();
  }

  protected updateSearch(value: string): void {
    this.searchTerm.set(value);
  }

  protected updateRole(value: string): void {
    this.selectedRole.set(value as 'Todos los roles' | UserRole);
  }

  protected openCreateEditor(): void {
    if (this.editorSaving()) {
      return;
    }

    this.editorMode.set('create');
    this.editingUserId.set(null);
    this.editorUser.set(null);
    this.editorLoading.set(false);
    this.editorErrorMessage.set('');
    this.successMessage.set('');
    this.form.enable();
    this.configurePasswordValidators(true);
    this.form.reset(this.emptyFormValue());
  }

  protected openEditEditor(user: UserRecord): void {
    if (this.editorSaving()) {
      return;
    }

    this.editorMode.set('edit');
    this.editingUserId.set(user.id);
    this.editorUser.set(null);
    this.editorLoading.set(true);
    this.editorErrorMessage.set('');
    this.successMessage.set('');
    this.form.reset(this.emptyFormValue());
    this.form.disable();
    this.configurePasswordValidators(false);

    this.usersApi
      .getUser(user.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (loadedUser) => {
          if (this.editingUserId() !== loadedUser.id || this.editorMode() !== 'edit') {
            return;
          }

          this.editorUser.set(loadedUser);
          this.form.enable();
          this.form.reset({
            username: loadedUser.username,
            firstName: loadedUser.firstName,
            lastName: loadedUser.lastName,
            displayName: loadedUser.displayName,
            email: loadedUser.email,
            password: '',
            roleCode: loadedUser.roleCode,
            statusCode: loadedUser.statusCode,
            preferredLocale: loadedUser.preferredLocale,
            documentType: loadedUser.documentType ?? '',
            documentNumber: loadedUser.documentNumber ?? ''
          });
          this.editorLoading.set(false);
        },
        error: (error: unknown) => {
          this.form.enable();
          this.editorLoading.set(false);
          this.editorErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible cargar el usuario seleccionado.'));
        }
      });
  }

  protected closeEditor(): void {
    if (this.editorSaving()) {
      return;
    }

    this.editorMode.set(null);
    this.editingUserId.set(null);
    this.editorUser.set(null);
    this.editorLoading.set(false);
    this.editorErrorMessage.set('');
    this.form.reset(this.emptyFormValue());
    this.form.enable();
  }

  protected submitEditor(): void {
    const mode = this.editorMode();

    if (!mode || this.editorLoading() || this.editorSaving()) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formValue = this.form.getRawValue();
    const baseRequest = this.buildBaseRequest(formValue);

    this.editorSaving.set(true);
    this.editorErrorMessage.set('');

    if (mode === 'create') {
      const payload: UserCreateRequest = {
        ...baseRequest,
        password: formValue.password.trim()
      };

      this.usersApi
        .createUser(payload)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (createdUser) => {
            this.editorSaving.set(false);
            this.upsertUser(createdUser);
            this.successMessage.set('El usuario se creo correctamente.');
            this.closeEditor();
          },
          error: (error: unknown) => {
            this.editorSaving.set(false);
            this.editorErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible crear el usuario.'));
          }
        });

      return;
    }

    const userId = this.editingUserId();
    if (!userId) {
      this.editorSaving.set(false);
      this.editorErrorMessage.set('No se encontro el usuario que se intenta actualizar.');
      return;
    }

    const payload: UserUpdateRequest = {
      ...baseRequest,
      password: this.normalizeOptional(formValue.password)
    };

    this.usersApi
      .updateUser(userId, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedUser) => {
          this.editorSaving.set(false);
          this.upsertUser(updatedUser);
          this.successMessage.set('El usuario se actualizo correctamente.');
          this.closeEditor();
        },
        error: (error: unknown) => {
          this.editorSaving.set(false);
          this.editorErrorMessage.set(this.resolveErrorMessage(error, 'No fue posible actualizar el usuario.'));
        }
      });
  }

  protected reload(): void {
    this.loadUsers();
  }

  protected removeUser(user: UserRecord): void {
    const confirmed = window.confirm(`Se dara de baja a ${this.fullName(user)}. Deseas continuar?`);

    if (!confirmed || this.deletingUserId() === user.id) {
      return;
    }

    this.deletingUserId.set(user.id);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.usersApi
      .deleteUser(user.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.deletingUserId.set(null);
          this.users.update((users) => users.filter((currentUser) => currentUser.id !== user.id));
          this.successMessage.set('El usuario se dio de baja correctamente.');
        },
        error: () => {
          this.deletingUserId.set(null);
          this.errorMessage.set('No fue posible dar de baja al usuario seleccionado.');
        }
      });
  }

  protected fullName(user: UserRecord): string {
    return `${user.firstName} ${user.lastName}`.trim();
  }

  protected roleTone(role: UserRole): AccentTone {
    switch (role) {
      case 'Administrador':
        return 'blue';
      case 'Bibliotecario':
        return 'amber';
      case 'Profesor':
        return 'violet';
      case 'Estudiante':
        return 'green';
    }
  }

  protected statusTone(status: UserStatus): AccentTone {
    switch (status) {
      case 'Activo':
        return 'green';
      case 'Pendiente':
        return 'amber';
      case 'Suspendido':
        return 'slate';
      case 'Inactivo':
        return 'slate';
    }
  }

  protected isDeleting(userId: string): boolean {
    return this.deletingUserId() === userId;
  }

  protected dismissSuccessMessage(): void {
    this.successMessage.set('');
  }

  protected showFieldError(fieldName: UserFormField): boolean {
    const control = this.form.controls[fieldName];
    return control.invalid && (control.dirty || control.touched);
  }

  protected fieldErrorMessage(fieldName: UserFormField): string {
    const control = this.form.controls[fieldName];

    if (control.hasError('required')) {
      return 'Este campo es obligatorio.';
    }

    if (control.hasError('email')) {
      return 'Ingresa un correo electronico valido.';
    }

    if (control.hasError('minlength')) {
      const requiredLength = control.getError('minlength')?.requiredLength as number | undefined;
      return `Debes capturar al menos ${requiredLength ?? 0} caracteres.`;
    }

    if (control.hasError('maxlength')) {
      const requiredLength = control.getError('maxlength')?.requiredLength as number | undefined;
      return `No puede exceder ${requiredLength ?? 0} caracteres.`;
    }

    return 'Revisa este valor.';
  }

  private loadUsers(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.usersApi
      .listUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => {
          this.users.set(this.sortUsers(users));
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('No fue posible cargar los usuarios desde la API.');
          this.loading.set(false);
        }
      });
  }

  private buildBaseRequest(formValue: ReturnType<typeof this.form.getRawValue>): UserSaveRequestBase {
    return {
      username: formValue.username.trim(),
      firstName: formValue.firstName.trim(),
      lastName: formValue.lastName.trim(),
      displayName: this.normalizeOptional(formValue.displayName),
      email: formValue.email.trim(),
      roleCode: formValue.roleCode,
      statusCode: formValue.statusCode,
      preferredLocale: this.normalizeOptional(formValue.preferredLocale) ?? 'es_MX',
      documentType: this.normalizeOptional(formValue.documentType),
      documentNumber: this.normalizeOptional(formValue.documentNumber),
      metadataJson: this.editorUser()?.metadataJson ?? null
    };
  }

  private configurePasswordValidators(required: boolean): void {
    const validators = [Validators.minLength(8), Validators.maxLength(100)];

    if (required) {
      validators.unshift(Validators.required);
    }

    this.form.controls.password.setValidators(validators);
    this.form.controls.password.updateValueAndValidity();
  }

  private emptyFormValue() {
    return {
      username: '',
      firstName: '',
      lastName: '',
      displayName: '',
      email: '',
      password: '',
      roleCode: 'STUDENT' as UserRoleCode,
      statusCode: 'active' as UserStatusCode,
      preferredLocale: 'es_MX',
      documentType: '',
      documentNumber: ''
    };
  }

  private normalizeOptional(value: string): string | null {
    const normalizedValue = value.trim();
    return normalizedValue ? normalizedValue : null;
  }

  private upsertUser(user: UserRecord): void {
    this.users.update((users) => this.sortUsers([...users.filter((currentUser) => currentUser.id !== user.id), user]));
  }

  private sortUsers(users: ReadonlyArray<UserRecord>): UserRecord[] {
    return [...users].sort((left, right) =>
      left.username.localeCompare(right.username, 'es', { sensitivity: 'base' })
    );
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
