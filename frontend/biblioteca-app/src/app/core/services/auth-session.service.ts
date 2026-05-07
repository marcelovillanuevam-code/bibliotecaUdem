import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest } from '../../shared/models/auth.model';
import { CurrentUser } from '../../shared/models/dashboard.model';
import { UserRoleCode } from '../../shared/models/user.model';

interface StoredSession {
  tokenType: string;
  accessToken: string;
  expiresAtUtc: string;
  user: AuthResponse['user'];
}

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private static readonly storageKey = 'biblioteca-udem.session';
  private readonly httpClient = inject(HttpClient);
  private readonly session = signal<StoredSession | null>(this.readSession());

  readonly isAuthenticated = computed(() => this.session() !== null);
  readonly accessToken = computed(() => this.session()?.accessToken ?? '');
  readonly currentUserId = computed(() => this.session()?.user.id ?? '');
  readonly currentUserRoleCodes = computed<UserRoleCode[]>(() =>
    this.session()?.user.roles.filter((role): role is UserRoleCode => this.isKnownRole(role)) ?? []
  );
  readonly currentUser = computed<CurrentUser>(() => {
    const session = this.session();

    if (!session) {
      return {
        name: 'Invitado',
        role: 'Sin sesion',
        email: '',
        initials: '--',
        code: 'N/A'
      };
    }

    const roleCode = session.user.roles[0] ?? '';
    const displayName = session.user.displayName || session.user.username;

    return {
      name: displayName,
      role: this.roleLabel(roleCode),
      email: session.user.email,
      initials: this.initialsFrom(displayName),
      code: session.user.username.toUpperCase()
    };
  });

  login(request: LoginRequest): Observable<void> {
    return this.httpClient
      .post<AuthResponse>(`${environment.apiBaseUrl}/auth/login`, request)
      .pipe(
        tap((response) => this.persistSession(response)),
        map(() => void 0)
      );
  }

  logout(): void {
    this.session.set(null);

    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(AuthSessionService.storageKey);
    }
  }

  hasAnyRole(roles: readonly UserRoleCode[]): boolean {
    const roleSet = new Set(this.currentUserRoleCodes());
    return roles.some((role) => roleSet.has(role));
  }

  private persistSession(response: AuthResponse): void {
    const storedSession: StoredSession = {
      tokenType: response.tokenType,
      accessToken: response.accessToken,
      expiresAtUtc: response.expiresAtUtc,
      user: response.user
    };

    this.session.set(storedSession);

    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(AuthSessionService.storageKey, JSON.stringify(storedSession));
    }
  }

  private readSession(): StoredSession | null {
    if (typeof localStorage === 'undefined') {
      return null;
    }

    const rawSession = localStorage.getItem(AuthSessionService.storageKey);

    if (!rawSession) {
      return null;
    }

    try {
      return JSON.parse(rawSession) as StoredSession;
    } catch {
      localStorage.removeItem(AuthSessionService.storageKey);
      return null;
    }
  }

  private roleLabel(roleCode: string): string {
    switch (roleCode) {
      case 'ADMIN':
        return 'Administrador';
      case 'LIBRARIAN':
        return 'Bibliotecario';
      case 'TEACHER':
        return 'Profesor';
      case 'STUDENT':
        return 'Estudiante';
      default:
        return roleCode || 'Usuario';
    }
  }

  private isKnownRole(role: string): role is UserRoleCode {
    return role === 'ADMIN' || role === 'LIBRARIAN' || role === 'TEACHER' || role === 'STUDENT';
  }

  private initialsFrom(displayName: string): string {
    return displayName
      .split(/\s+/)
      .filter((segment) => segment.length > 0)
      .slice(0, 2)
      .map((segment) => segment[0]?.toUpperCase() ?? '')
      .join('');
  }
}
