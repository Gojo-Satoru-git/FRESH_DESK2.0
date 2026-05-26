import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Read the signal synchronously
  const user = authService.currentUser();

  // 1. If not logged in, kick them to login
  if (!user) {
    return router.createUrlTree(['/login']);
  }

  // 2. Check Role-Based Access Control (RBAC)
  // We will pass allowed roles in the route configuration data
  const allowedRoles = route.data['roles'] as Array<string>;

  if (allowedRoles && !allowedRoles.includes(user.role)) {
    // If they try to access a route they aren't allowed in, kick them to their safe home
    if (user.role === 'customer') {
      return router.createUrlTree(['/customer-portal']);
    } else {
      return router.createUrlTree(['/agent-dashboard']);
    }
  }
  // 3. User is logged in and has the right role. Let them pass.
  return true;
};
