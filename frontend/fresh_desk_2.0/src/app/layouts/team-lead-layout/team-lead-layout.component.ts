import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
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
            <span class="text-sm font-semibold text-text-main flex items-center gap-2">
              <div class="w-8 h-8 rounded-full bg-primary-blue/20 text-primary-blue flex items-center justify-center font-bold text-xs uppercase border border-primary-blue/30">
                {{ authService.currentUser()?.firstName?.[0] || 'L' }}{{ authService.currentUser()?.lastName?.[0] || 'D' }}
              </div>
              {{ authService.currentUser()?.fullName }} (Team Lead)
            </span>
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

  navItems: NavItem[] = [
    {
      label: 'Dashboard',
      route: '/team-lead/dashboard',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 3.055A9.001 9.001 0 1020.945 13H11V3.055z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.488 9H15V3.512A9.025 9.025 0 0120.488 9z"></path></svg>'
    },
    {
      label: 'Group Queue',
      route: '/team-lead/queue',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16"></path></svg>'
    },
    {
      label: 'Assignment Workspace',
      route: '/team-lead/assignment',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>'
    }
  ];
}
