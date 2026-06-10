import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeSwitcherComponent } from '../../core/theme/theme-switcher.component';
import { CreateTicketModalComponent } from '../../features/tickets/components/create-ticket-modal.component';
import { AuthService } from '../../core/auth/auth.service';
import { AdrenalinSideNavComponent } from '../../shared/components/adrenalin-side-nav.component';
import { NavItem } from '../../shared/models/nav-item.interface';

@Component({
  selector: 'app-agent-layout',
  standalone: true,
  imports: [RouterOutlet, ThemeSwitcherComponent, CreateTicketModalComponent, AdrenalinSideNavComponent],
  template: `
    <div
      class="h-screen w-full flex flex-col bg-background text-text-main overflow-hidden transition-colors duration-300"
    >

      <div class="flex flex-1 overflow-hidden">
        
        <app-adrenalin-side-nav [items]="agentMenu" class="h-full z-40 flex-shrink-0"></app-adrenalin-side-nav>

        <main class="flex-1 flex flex-col min-w-0 overflow-hidden">
          <header
            class="h-16 flex-shrink-0 bg-surface border-b border-disabled-gray dark:border-gray-800 flex items-center justify-between px-6"
          >
            <div class="flex items-center gap-4">
            </div>

            <div class="flex items-center gap-4 lg:gap-5">
              <button
                (click)="isCreateTicketModalOpen.set(true)"
                class="hidden sm:flex items-center gap-2 bg-primary hover:bg-primary-hover text-text-white px-4 py-2 rounded-full text-sm font-medium transition-colors shadow-sm"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 4v16m8-8H4"
                  ></path>
                </svg>
                New Ticket
              </button>

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
                  placeholder="Search tickets..."
                  class="w-full pl-10 pr-4 py-1.5 bg-background border border-disabled-gray dark:border-gray-700 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary-blue text-text-main placeholder:text-text-muted/60 transition-all"
                />
              </div>

              <button class="text-text-muted hover:text-primary-blue  transition-colors relative">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
                  ></path>
                </svg>
                <span
                  class="absolute top-0 right-0 block h-2 w-2 rounded-full bg-error-red ring-2 ring-surface"
                ></span>
              </button>

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
                A
              </button>
            </div>
          </header>
          
          <app-create-ticket-modal
            [isOpen]="isCreateTicketModalOpen()"
            (closeModal)="isCreateTicketModalOpen.set(false)"
          >
          </app-create-ticket-modal>

          <div class="flex-1 overflow-y-auto p-6 bg-background">
            <router-outlet></router-outlet>
          </div>
        </main>
      </div>
    </div>
  `,
})
export class AgentLayoutComponent {
  authService = inject(AuthService);
  showActivationAlert = signal<boolean>(true);
  isCreateTicketModalOpen = signal<boolean>(false);

  // The newly integrated Data-Driven Menu Array mapped to your exact routing paths
  agentMenu: NavItem[] = [
    {
      label: 'Dashboard',
      route: '/agent/dashboard',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z"></path></svg>'
    },
    {
      label: 'Tickets',
      route: '/agent/tickets',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 5v2m0 4v2m0 4v2M5 5a2 2 0 00-2 2v3a2 2 0 110 4v3a2 2 0 002 2h14a2 2 0 002-2v-3a2 2 0 110-4V7a2 2 0 00-2-2H5z"></path></svg>'
    },
    {
      label: 'Contacts',
      route: '/agent/contacts',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>'
    },
    {
      label: 'Knowledge Base',
      route: '/agent/knowledge-base',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path></svg>'
    },
    {
      label: 'Reports',
      route: '/agent/reports',
      iconSvg: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path></svg>'
    }
  ];

  dismissAlert() {
    this.showActivationAlert.set(false);
  }

  logout() {
    this.authService.logout();
  }
}