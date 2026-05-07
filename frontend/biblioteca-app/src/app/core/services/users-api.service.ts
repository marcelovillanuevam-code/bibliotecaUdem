import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  UserCreateRequest,
  UserRecord,
  UserRole,
  UserRoleCode,
  UserStatus,
  UserStatusCode,
  UserUpdateRequest
} from '../../shared/models/user.model';

interface ApiUserDto {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  displayName: string;
  email: string;
  universityId: string;
  documentType: string | null;
  documentNumber: string | null;
  roleCode: string;
  roleLabel: string;
  statusCode: string;
  statusLabel: string;
  preferredLocale: string;
  metadataJson: string | null;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
}

interface ApiSaveUserRequest {
  username: string;
  firstName: string;
  lastName: string;
  displayName?: string | null;
  email: string;
  password?: string;
  roleCode: UserRoleCode;
  statusCode: UserStatusCode;
  preferredLocale: string;
  documentType?: string | null;
  documentNumber?: string | null;
  metadataJson?: string | null;
}

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly httpClient = inject(HttpClient);

  listUsers(): Observable<UserRecord[]> {
    return this.httpClient
      .get<ApiUserDto[]>(`${environment.apiBaseUrl}/usuarios`)
      .pipe(map((users) => users.map((user) => this.toUserRecord(user))));
  }

  getUser(id: string): Observable<UserRecord> {
    return this.httpClient
      .get<ApiUserDto>(`${environment.apiBaseUrl}/usuarios/${id}`)
      .pipe(map((user) => this.toUserRecord(user)));
  }

  createUser(request: UserCreateRequest): Observable<UserRecord> {
    return this.httpClient
      .post<ApiUserDto>(`${environment.apiBaseUrl}/usuarios`, this.toApiSaveUserRequest(request))
      .pipe(map((user) => this.toUserRecord(user)));
  }

  updateUser(id: string, request: UserUpdateRequest): Observable<UserRecord> {
    return this.httpClient
      .put<ApiUserDto>(`${environment.apiBaseUrl}/usuarios/${id}`, this.toApiSaveUserRequest(request))
      .pipe(map((user) => this.toUserRecord(user)));
  }

  deleteUser(id: string): Observable<void> {
    return this.httpClient.delete<void>(`${environment.apiBaseUrl}/usuarios/${id}`);
  }

  private toUserRecord(user: ApiUserDto): UserRecord {
    const roleCode = this.roleCodeFromApi(user.roleCode);
    const statusCode = this.statusCodeFromApi(user.statusCode);
    const role = this.roleLabel(roleCode, user.roleLabel);
    const status = this.statusLabel(statusCode, user.statusLabel);

    return {
      id: user.id,
      username: user.username,
      universityId: user.universityId || user.username.toUpperCase(),
      firstName: user.firstName,
      lastName: user.lastName,
      displayName: user.displayName,
      email: user.email,
      roleCode,
      role,
      statusCode,
      status,
      preferredLocale: user.preferredLocale,
      documentType: user.documentType,
      documentNumber: user.documentNumber ?? user.universityId ?? null,
      metadataJson: user.metadataJson,
      initials: this.initialsFrom(user.displayName || `${user.firstName} ${user.lastName}`),
      accent: this.roleTone(role)
    };
  }

  private toApiSaveUserRequest(request: UserCreateRequest | UserUpdateRequest): ApiSaveUserRequest {
    const normalizedPassword = this.normalizeOptional(request.password);

    return {
      username: request.username.trim(),
      firstName: request.firstName.trim(),
      lastName: request.lastName.trim(),
      displayName: this.normalizeOptional(request.displayName),
      email: request.email.trim(),
      ...(normalizedPassword ? { password: normalizedPassword } : {}),
      roleCode: request.roleCode,
      statusCode: request.statusCode,
      preferredLocale: request.preferredLocale.trim() || 'es_MX',
      documentType: this.normalizeOptional(request.documentType),
      documentNumber: this.normalizeOptional(request.documentNumber),
      metadataJson: this.normalizeOptional(request.metadataJson)
    };
  }

  private roleCodeFromApi(roleCode: string): UserRoleCode {
    switch (roleCode) {
      case 'ADMIN':
      case 'LIBRARIAN':
      case 'TEACHER':
      case 'STUDENT':
        return roleCode;
      default:
        return 'STUDENT';
    }
  }

  private statusCodeFromApi(statusCode: string): UserStatusCode {
    switch (statusCode) {
      case 'inactive':
      case 'pending_verification':
      case 'suspended':
      case 'active':
        return statusCode;
      default:
        return 'active';
    }
  }

  private roleLabel(roleCode: UserRoleCode, roleLabel: string): UserRole {
    if (
      roleLabel === 'Administrador' ||
      roleLabel === 'Bibliotecario' ||
      roleLabel === 'Profesor' ||
      roleLabel === 'Estudiante'
    ) {
      return roleLabel;
    }

    switch (roleCode) {
      case 'ADMIN':
        return 'Administrador';
      case 'LIBRARIAN':
        return 'Bibliotecario';
      case 'TEACHER':
        return 'Profesor';
      default:
        return 'Estudiante';
    }
  }

  private statusLabel(statusCode: UserStatusCode, statusLabel: string): UserStatus {
    if (
      statusLabel === 'Activo' ||
      statusLabel === 'Pendiente' ||
      statusLabel === 'Suspendido' ||
      statusLabel === 'Inactivo'
    ) {
      return statusLabel;
    }

    switch (statusCode) {
      case 'inactive':
        return 'Inactivo';
      case 'suspended':
        return 'Suspendido';
      case 'pending_verification':
        return 'Pendiente';
      default:
        return 'Activo';
    }
  }

  private roleTone(role: UserRecord['role']): UserRecord['accent'] {
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

  private initialsFrom(displayName: string): string {
    return displayName
      .split(/\s+/)
      .filter((segment) => segment.length > 0)
      .slice(0, 2)
      .map((segment) => segment[0]?.toUpperCase() ?? '')
      .join('');
  }

  private normalizeOptional(value: string | null | undefined): string | null {
    const normalizedValue = value?.trim();
    return normalizedValue ? normalizedValue : null;
  }
}
