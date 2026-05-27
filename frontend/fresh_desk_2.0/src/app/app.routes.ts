import { Routes } from '@angular/router';

export const routes: Routes = [
  // 1. Default Redirect (If they just type localhost:4200, send to login)
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'signup',
    loadComponent: () => import('./features/auth/signup.component').then((m) => m.SignupComponent),
  },
  // 2. The Login Route (Lazy loaded)
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'agent-dashboard',
    // canActivate: [authGuard], <-- We will add the Guard back later!
    loadComponent: () =>
      import('./features/agent-dashboard/agent-dashboard.component').then(
        (m) => m.AgentDashboardComponent,
      ),
  },
  {
    path: 'customer-portal',
    // canActivate: [authGuard],
    loadComponent: () =>
      import('./features/customer-portal/customer-portal.component').then(
        (m) => m.CustomerPortalComponent,
      ),
  },

  // 3. Catch-All Fallback (If they type a garbage URL, send to login)
  {
    path: '**',
    redirectTo: 'login',
  },
];
