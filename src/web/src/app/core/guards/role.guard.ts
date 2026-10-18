import { inject } from '@angular/core';
import { CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Ensures only users with the specified role(s) can access the route or any child routes.
 * If the user is unauthenticated, redirects to /login.
 * If the user has a different role, redirects to their own assigned dashboard.
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated) {
    return router.createUrlTree(['/login']);
  }

  const currentRole = auth.role;
  if (currentRole && allowedRoles.includes(currentRole)) {
    return true;
  }

  if (!currentRole) {
    auth.logout();
    return router.createUrlTree(['/login']);
  }

  return router.createUrlTree([auth.destinationFor(currentRole)]);
};

/**
 * Child route protection variant of roleGuard.
 */
export const roleChildGuard = (allowedRoles: string[]): CanActivateChildFn =>
  roleGuard(allowedRoles);

/**
 * Prevents already authenticated users from accessing login or register pages,
 * redirecting them directly to their appropriate dashboard.
 */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated) {
    return router.createUrlTree([auth.destinationFor(auth.role)]);
  }

  return true;
};

