import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
  },
  {
    path: 'users',
    loadComponent: () => import('./components/user-management/user-list.component').then(m => m.UserListComponent)
  },
  {
    path: 'roles',
    loadComponent: () => import('./components/roles/role-list.component').then(m => m.RoleListComponent)
  },
  {
    path: 'groups',
    loadComponent: () => import('./components/groups/group-list.component').then(m => m.GroupListComponent)
  },
  {
    path: 'companies',
    loadComponent: () => import('./components/companies/company-list.component').then(m => m.CompanyListComponent)
  },
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  }
];
