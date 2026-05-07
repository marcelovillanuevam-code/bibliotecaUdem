export interface AuthenticatedUser {
  id: string;
  username: string;
  statusCode: string;
  preferredLocale: string;
  displayName: string;
  email: string;
  roles: string[];
}

export interface AuthResponse {
  tokenType: string;
  accessToken: string;
  expiresAtUtc: string;
  user: AuthenticatedUser;
}

export interface LoginRequest {
  username: string;
  password: string;
}
