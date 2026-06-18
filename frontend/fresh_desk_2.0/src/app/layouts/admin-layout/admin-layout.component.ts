import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { ThemeSwitcherComponent } from '../../core/theme/theme-switcher.component';
import { AuthService } from '../../core/auth/auth.service';
import { AdrenalinSideNavComponent } from '../../shared/components/adrenalin-side-nav.component';
import { NavItem } from '../../shared/models/nav-item.interface';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, ThemeSwitcherComponent, AdrenalinSideNavComponent],
  template: `
    <div
      class="h-screen w-full flex flex-col bg-background text-text-main overflow-hidden transition-colors duration-300"
    >
      <div class="flex flex-1 overflow-hidden">
        
        <app-adrenalin-side-nav [items]="adminMenu" class="h-full z-40 flex-shrink-0"></app-adrenalin-side-nav>

        <main class="flex-1 flex flex-col min-w-0 overflow-hidden">
          <header
            class="h-16 flex-shrink-0 bg-surface border-b border-disabled-gray dark:border-gray-800 flex items-center justify-between px-6"
          >
            <div class="flex items-center gap-4">
               <span class="ml-3 text-xl font-bold font-heading text-text-dark dark:text-text-white tracking-tight hidden sm:block">Adrenalin <span class="text-primary-blue">Admin</span></span>
            </div>

            <div class="flex items-center gap-4 lg:gap-5">
              <div class="relative hidden md:block w-64">
                <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <svg
                    class="w-4 h-4 text-text-muted"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                    ></path>
                  </svg>
                </div>
                <input
                  type="text"
                  placeholder="Global Search..."
                  class="w-full pl-10 pr-4 py-1.5 bg-background border border-disabled-gray dark:border-gray-700 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary-blue text-text-main placeholder:text-text-muted/60 transition-all"
                />
              </div>

              <app-theme-switcher></app-theme-switcher>

              <button
                (click)="logout()"
                class="flex items-center gap-1.5 text-xs font-semibold bg-table-light-gray hover:bg-disabled-gray dark:bg-gray-800 dark:hover:bg-gray-700 text-text-main px-3 py-1.5 rounded-lg border border-disabled-gray dark:border-gray-700 transition-all cursor-pointer"
                title="Logout"
              >
                <svg class="w-4 h-4 text-error-red" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                <span class="hidden md:inline">Logout</span>
              </button>

              <button
                class="h-8 w-8 rounded-full bg-primary-blue text-text-white flex items-center justify-center font-bold text-sm hover:ring-2 hover:ring-primary-blue hover:ring-offset-2 hover:ring-offset-surface transition-all"
              >
                {{ userInitials }}
              </button>
            </div>
          </header>
          
          <div class="flex-1 overflow-y-auto p-6 bg-background">
            <router-outlet></router-outlet>
          </div>
        </main>
      </div>
    </div>
  `
})
export class AdminLayoutComponent {
  authService = inject(AuthService);
  router = inject(Router);

  adminMenu: NavItem[] = [
    {
      label: 'Dashboard',
      route: '/admin/dashboard',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"></path></svg>'
    },
    {
      label: 'Users',
      route: '/admin/users',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"></path></svg>'
    },
    {
      label: 'Roles',
      route: '/admin/roles',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path></svg>'
    },
    {
      label: 'Groups',
      route: '/admin/groups',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path></svg>'
    },
    {
      label: 'Companies',
      route: '/admin/companies',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"></path></svg>'
    }
  ];

  get userName(): string {
    const user = this.authService.currentUser();
    return user?.fullName || user?.firstName || user?.email || 'Admin User';
  }

  get userInitials(): string {
    const name = this.userName;
    if (!name) return 'A';
    const parts = name.split(' ');
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
