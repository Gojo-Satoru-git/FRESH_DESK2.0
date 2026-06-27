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
    path: 'workspace', // Changed from 'agent'
    canActivate: [
      permissionGuard([
        PERMISSIONS.DASHBOARD.VIEW_AGENT,
        PERMISSIONS.DASHBOARD.VIEW_TEAM_LEAD,
        PERMISSIONS.DASHBOARD.VIEW_ADMIN,
      ]),
    ],
    // Make sure this matches the exported class name in your workspace-layout.component.ts file
    loadComponent: () =>
      import('./layouts/workspace-layout.component').then((m) => m.WorkspaceLayoutComponent),
    children: [
      {
        path: 'dashboard',
        // Make sure this points to the new unified dashboard we built!
        loadComponent: () =>
          import('./features/dashboard/workspace-dashboard').then(
            (m) => m.WorkspaceDashboardComponent,
          ),
      },
      {
        path: 'tickets',
        // Require at least ONE of these ticket read permissions to access the tickets module
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
        // Example: Restricting reports to Leads and Admins
        canActivate: [
          permissionGuard([PERMISSIONS.DASHBOARD.VIEW_TEAM_LEAD, PERMISSIONS.DASHBOARD.VIEW_ADMIN]),
        ],
        loadComponent: () =>
          import('./features/reports/components/reports.component').then((m) => m.ReportsComponent),
      },

      // --- PBAC PROTECTED ADMIN ROUTES ---
      {
        path: 'admin/queues',
        // GATEKEEPER: Only users with Admin View permissions can load this component
        canActivate: [permissionGuard([PERMISSIONS.DASHBOARD.VIEW_ADMIN])],
        loadComponent: () =>
          import('./features/admin-panel/components/queue-master.component').then(
            (m) => m.QueueMasterComponent,
          ),
      },
      {
        path: 'admin/routing-rules',
        canActivate: [permissionGuard([PERMISSIONS.DASHBOARD.VIEW_ADMIN])],
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
  // CUSTOMER PORTAL (Untouched per instructions)
  // ==========================================
  {
    path: 'customer-portal',
    canMatch: [permissionGuard([PERMISSIONS.DASHBOARD.VIEW_CUSTOMER])],
    canActivate: [permissionGuard([PERMISSIONS.DASHBOARD.VIEW_CUSTOMER])],
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
