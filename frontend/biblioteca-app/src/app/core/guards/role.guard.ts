import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserRoleCode } from '../../shared/models/user.model';
import { AuthSessionService } from '../services/auth-session.service';

export function roleGuard(allowedRoles: readonly UserRoleCode[]): CanActivateFn {
  return () => {
    const authSession = inject(AuthSessionService);
    const router = inject(Router);

    if (!authSession.isAuthenticated()) {
      return router.createUrlTree(['/login']);
    }

    return authSession.hasAnyRole(allowedRoles)
      ? true
      : router.createUrlTree(['/dashboard/prestamos']);
  };
}
