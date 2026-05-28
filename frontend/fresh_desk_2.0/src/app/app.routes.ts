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
    path: 'agent',
    // 1. Load the Layout Wrapper
    loadComponent: () =>
      import('./layouts/agent-layout/agent-layout.component').then(
        (m) => m.AgentLayoutComponent,
      ),
    // 2. Define the child pages that render INSIDE the wrapper's <router-outlet>
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/agent-dashboard/agent-dashboard.component').then(
            (m) => m.AgentDashboardComponent,
          ),
      },
      // You can add 'tickets', 'contacts', 'settings' here later
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
    ],
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
