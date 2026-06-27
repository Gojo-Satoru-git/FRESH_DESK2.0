import { Routes } from '@angular/router';
import { permissionGuard } from './core/guards/permission.guard';
import { PERMISSIONS } from './core/auth/permission.constants';

export const routes: Routes = [
  // 1. Default Redirect
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'signup',
    loadComponent: () => import('./features/auth/signup.component').then((m) => m.SignupComponent),
  },
  // 2. The Login Route
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent),
  },

  // ==========================================
  // UNIFIED WORKSPACE (Agents, Leads, Admins)
  // ==========================================
  {
    path: 'workspace',
    canActivate: [
      permissionGuard([
        PERMISSIONS.DASHBOARD.READ,   // Replaced VIEW_AGENT
        PERMISSIONS.DASHBOARD.WRITE,  // Replaced VIEW_TEAM_LEAD
        PERMISSIONS.DASHBOARD.ADMIN,  // Replaced VIEW_ADMIN
      ]),
    ],
    loadComponent: () =>
      import('./layouts/workspace-layout.component').then((m) => m.WorkspaceLayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/workspace-dashboard').then(
            (m) => m.WorkspaceDashboardComponent,
          ),
      },
      {
        path: 'tickets',
        // These matched your backend perfectly, so they stay the same!
        canActivate: [
          permissionGuard([
            PERMISSIONS.TICKET.READ_ASSIGNED,
            PERMISSIONS.TICKET.READ_QUEUE,
            PERMISSIONS.TICKET.READ_TEAM,
            PERMISSIONS.TICKET.READ_ALL,
          ]),
        ],
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./features/tickets/components/ticket-dashboard.component').then(
                (m) => m.TicketDashboardComponent,
              ),
          },
          {
            path: 'list',
            loadComponent: () =>
              import('./features/tickets/components/ticket-list.component').then(
                (m) => m.TicketListComponent,
              ),
          },
          {
            path: ':id',
            loadComponent: () =>
              import('./features/tickets/components/ticket-detail.component').then(
                (m) => m.TicketDetailComponent,
              ),
          },
        ],
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
        canActivate: [permissionGuard([PERMISSIONS.KB.READ])],
        loadComponent: () =>
          import('./features/knowledgebase/components/knowledge-base.component').then(
            (m) => m.KnowledgeBaseComponent,
          ),
      },
      {
        path: 'reports',
        // UPDATED: Now uses your actual Report permissions!
        canActivate: [
          permissionGuard([PERMISSIONS.REPORT.READ, PERMISSIONS.REPORT.READ_ALL]),
        ],
        loadComponent: () =>
          import('./features/reports/components/reports.component').then((m) => m.ReportsComponent),
      },

      // --- PBAC PROTECTED ADMIN ROUTES ---
      {
        path: 'admin/queues',
        // UPDATED: Uses the new ADMIN constant
        canActivate: [permissionGuard([PERMISSIONS.DASHBOARD.ADMIN])],
        loadComponent: () =>
          import('./features/admin-panel/components/queue-master.component').then(
            (m) => m.QueueMasterComponent,
          ),
      },
      {
        path: 'admin/routing-rules',
        // UPDATED: Uses the new ADMIN constant
        canActivate: [permissionGuard([PERMISSIONS.DASHBOARD.ADMIN])],
        loadComponent: () =>
          import('./features/admin-panel/components/queue-routing-rules.component').then((m) => m.QueueRoutingRulesComponent),
      },
      // -----------------------------------

      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
    ],
  },

  // ==========================================
  // CUSTOMER PORTAL
  // ==========================================
  {
    path: 'customer-portal',
    // Note: Make sure you add CUSTOMER: 'dashboard:customer' to your DASHBOARD object in permission.constants.ts!
    canMatch: [permissionGuard([PERMISSIONS.DASHBOARD.CUSTOMER])],
    canActivate: [permissionGuard([PERMISSIONS.DASHBOARD.CUSTOMER])],
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
        canActivate: [permissionGuard([PERMISSIONS.TICKET.CREATE])],
        loadComponent: () =>
          import('./features/customer-portal/raise-ticket.component').then(
            (m) => m.RaiseTicketComponent,
          ),
      },
      {
        path: 'my-tickets',
        canActivate: [permissionGuard([PERMISSIONS.TICKET.READ_COMPANY])],
        loadComponent: () =>
          import('./features/customer-portal/my-tickets.component').then(
            (m) => m.MyTicketsComponent,
          ),
      },
      {
        path: 'knowledge-base',
        canActivate: [permissionGuard([PERMISSIONS.KB.READ])],
        loadComponent: () =>
          import('./features/customer-portal/knowledge-base.component').then(
            (m) => m.KnowledgeBaseComponent,
          ),
      },
      {
        path: 'knowledge-base/articles',
        canActivate: [permissionGuard([PERMISSIONS.KB.READ])],
        loadComponent: () =>
          import('./features/customer-portal/articles.component').then((m) => m.ArticlesComponent),
      },
    ],
  },

  // ==========================================
  // FALLBACKS
  // ==========================================
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./features/exceptions/unauthorized/unauthorized').then((m) => m.Unauthorized),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];