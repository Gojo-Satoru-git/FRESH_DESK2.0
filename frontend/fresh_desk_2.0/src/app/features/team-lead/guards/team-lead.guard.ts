import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

export const teamLeadGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.parseUrl('/login');
  }

  // Use permissions instead of roles for Team Lead checks
  if (authService.hasPermission('ticket:view_group')) {
    return true;
  }

  // Fallback to role if permissions haven't loaded yet (though they should be hydrated)
  if (authService.currentUser()?.role === 'team_lead') {
    return true;
  }

  return router.parseUrl('/unauthorized');
};
