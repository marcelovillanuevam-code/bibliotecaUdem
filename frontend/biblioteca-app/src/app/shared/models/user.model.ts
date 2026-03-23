export type UserRoleCode = 'ADMIN' | 'LIBRARIAN' | 'TEACHER' | 'STUDENT';
export type UserRole = 'Administrador' | 'Bibliotecario' | 'Profesor' | 'Estudiante';
export type UserStatusCode = 'active' | 'inactive' | 'pending_verification' | 'suspended';
export type UserStatus = 'Activo' | 'Pendiente' | 'Suspendido' | 'Inactivo';
export type AccentTone = 'blue' | 'violet' | 'green' | 'amber' | 'slate';

export interface UserRecord {
  id: string;
  username: string;
  universityId: string;
  firstName: string;
  lastName: string;
  displayName: string;
  email: string;
  roleCode: UserRoleCode;
  role: UserRole;
  statusCode: UserStatusCode;
  status: UserStatus;
  preferredLocale: string;
  documentType: string | null;
  documentNumber: string | null;
  metadataJson: string | null;
  initials: string;
  accent: AccentTone;
}

export interface UserSaveRequestBase {
  username: string;
  firstName: string;
  lastName: string;
  displayName?: string | null;
  email: string;
  roleCode: UserRoleCode;
  statusCode: UserStatusCode;
  preferredLocale: string;
  documentType?: string | null;
  documentNumber?: string | null;
  metadataJson?: string | null;
}

export interface UserCreateRequest extends UserSaveRequestBase {
  password: string;
}

export interface UserUpdateRequest extends UserSaveRequestBase {
  password?: string | null;
}
