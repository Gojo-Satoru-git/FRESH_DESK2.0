import { Routes } from '@angular/router';
import { teamLeadGuard } from './guards/team-lead.guard';

export const teamLeadRoutes: Routes = [
  {
    path: '',
    canActivate: [teamLeadGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'queue',
        loadComponent: () => import('./queue/queue.component').then(m => m.QueueComponent)
      },
      {
        path: 'assignment',
        loadComponent: () => import('./assignment/assignment.component').then(m => m.AssignmentComponent)
      },
      {
        path: 'tickets/:id',
        loadComponent: () => import('../tickets/components/ticket-detail.component').then(m => m.TicketDetailComponent)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  }
];
