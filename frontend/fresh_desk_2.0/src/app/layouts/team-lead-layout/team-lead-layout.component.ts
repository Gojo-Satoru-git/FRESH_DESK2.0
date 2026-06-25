import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AdrenalinSideNavComponent } from '../../shared/components/adrenalin-side-nav.component';
import { NavItem } from '../../shared/models/nav-item.interface';

@Component({
  selector: 'app-team-lead-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, AdrenalinSideNavComponent],
  template: `
    <div class="flex h-screen bg-bg-light dark:bg-bg-dark font-sans text-text-main">
      <app-adrenalin-side-nav [items]="navItems"></app-adrenalin-side-nav>

      <div class="flex-1 flex flex-col h-screen overflow-hidden relative">
        <header class="h-16 bg-surface border-b border-table-dark-gray dark:border-gray-800 flex items-center justify-end px-8 shrink-0">
          <div class="flex items-center gap-4">
            <!-- User Info -->
            <span class="text-sm font-semibold text-text-main flex items-center gap-2">
              <div class="w-8 h-8 rounded-full bg-primary-blue/20 text-primary-blue flex items-center justify-center font-bold text-xs uppercase border border-primary-blue/30">
                {{ authService.currentUser()?.firstName?.[0] || 'T' }}{{ authService.currentUser()?.lastName?.[0] || 'L' }}
              </div>
              {{ authService.currentUser()?.fullName }} (Team Lead)
            </span>

            <!-- Logout Button -->
            <button
              (click)="logout()"
              title="Logout"
              class="flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-text-light hover:text-error-red hover:bg-error-red/10 rounded-lg border border-transparent hover:border-error-red/20 transition-all">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
              Logout
            </button>
          </div>
        </header>

        <main class="flex-1 overflow-y-auto p-8 relative scroll-smooth">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `
})
export class TeamLeadLayoutComponent {
  authService = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  navItems: NavItem[] = [
    {
      label: 'Dashboard',
      route: '/team-lead/dashboard',
      iconSvg: `<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
          d="M11 3.055A9.001 9.001 0 1020.945 13H11V3.055z"></path>
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
          d="M20.488 9H15V3.512A9.025 9.025 0 0120.488 9z"></path>
      </svg>`
    },
    {
      label: 'My Tickets',
      route: '/team-lead/my-tickets',
      iconSvg: `<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
          d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"></path>
      </svg>`
    },
    {
      label: 'Group Queue',
      route: '/team-lead/queue',
      iconSvg: `<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
          d="M4 6h16M4 10h16M4 14h16M4 18h16"></path>
      </svg>`
    },
    {
      label: 'Assignment Workspace',
      route: '/team-lead/assignment',
      iconSvg: `<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
          d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
      </svg>`
    }
  ];
}