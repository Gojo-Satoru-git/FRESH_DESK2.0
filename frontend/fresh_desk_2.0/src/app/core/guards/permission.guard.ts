import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * A functional guard that checks if the user has AT LEAST ONE of the required permissions.
 * Use this in your app.routes.ts on the `canActivate` or `canMatch` arrays.
 */
export function permissionGuard(requiredPermissions: string[]): CanActivateFn | CanMatchFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    console.log('🛡️ GUARD TRIGGERED');
    console.log('1. Is Authenticated?', authService.isAuthenticated());
    console.log('2. User Has Permissions:', authService.permissions());
    console.log('3. Route Requires:', requiredPermissions);
    // Check if they are authenticated first
    if (!authService.isAuthenticated()) {
      return router.parseUrl('/login');
    }

    // Check if they have the required permissions
    if (authService.hasAnyPermission(requiredPermissions)) {
      return true; // Let them in!
    }

    // If they lack permission, bounce them to an unauthorized page (or back to dashboard)
    console.warn(`Access Denied. Missing one of: ${requiredPermissions.join(', ')}`);
    return router.parseUrl('/unauthorized'); // Make sure you have a basic unauthorized route!
  };
}
