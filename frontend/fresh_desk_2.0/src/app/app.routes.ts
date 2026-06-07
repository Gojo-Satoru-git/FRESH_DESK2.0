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
      import('./layouts/agent-layout/agent-layout.component').then((m) => m.AgentLayoutComponent),
    // 2. Define the child pages that render INSIDE the wrapper's <router-outlet>
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/agent-dashboard/agent-dashboard.component').then(
            (m) => m.AgentDashboardComponent,
          ),
      },
      {
        path: 'tickets',
        loadComponent: () =>
          import('./features/tickets/components/ticket-list.component').then(
            (m) => m.TicketListComponent,
          ),
      },
      {
        path: 'contacts',
        loadComponent: () =>
          import('./features/contacts/components/contact-list.component').then(
            (m) => m.ContactListComponent,
          ),
      },
      {
        path: 'knowledge-base',
        loadComponent: () =>
          import('./features/knowledgebase/components/knowledge-base.component').then(
            (m) => m.KnowledgeBaseComponent,
          ),
      },
      {
        path: 'reports',
        loadComponent: () =>
          import('./features/reports/components/reports.component').then((m) => m.ReportsComponent),
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
    loadComponent: () =>
      import('./layouts/customer-layout/customer-layout.component').then(
        (m) => m.CustomerLayoutComponent,
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/customer-portal/customer-portal.component').then(
            (m) => m.CustomerPortalComponent,
          ),
      },
      {
        path: 'raise-ticket',
        loadComponent: () =>
          import('./features/customer-portal/raise-ticket.component').then(
            (m) => m.RaiseTicketComponent,
          ),
      },
      {
        path: 'ticket/:id',
        loadComponent: () =>
          import('./features/customer-portal/ticket-details.component')
            .then(m => m.TicketDetailsComponent),
      },
      {
        path: 'my-tickets',
        loadComponent: () =>
           import('./features/customer-portal/my-tickets.component')
             .then(m => m.MyTicketsComponent)
      }
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
  
];
