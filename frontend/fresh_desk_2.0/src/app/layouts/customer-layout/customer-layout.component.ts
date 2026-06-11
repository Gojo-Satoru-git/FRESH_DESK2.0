import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-customer-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="h-screen w-full flex bg-slate-50 text-text-main overflow-hidden transition-colors duration-300">

      <aside
        class="bg-[#012A4A] text-white transition-all duration-300 h-full flex flex-col flex-shrink-0 relative z-40"
        [style.width]="isSidebarOpen() ? '260px' : '64px'"
        [class.rounded-tr-3xl]="!isSidebarOpen()"
      >
        <div class="h-30 flex items-center px-4 flex-shrink-0 border-b border-white/10">
          <button
            (click)="toggleSidebar()"
            class="h-10 w-10 rounded-lg bg-white/10 hover:bg-white/20 text-white flex items-center justify-center font-bold transition-colors cursor-pointer outline-none"
          >
            ☰
          </button>
        </div>

        <nav class="flex-1 py-6 flex flex-col space-y-2 overflow-y-auto overflow-x-hidden">
          
          <a routerLink="/customer-portal" 
             routerLinkActive="bg-blue-500"
             [routerLinkActiveOptions]="{ exact: true }"
             class="flex items-center h-12 px-4 mx-2 text-lg font-semibold rounded-xl hover:bg-gray-700/50 transition-all whitespace-nowrap overflow-hidden"
             title="Dashboard"
          >
            <div class="w-8 h-8 flex-shrink-0 flex items-center justify-center">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"></path>
              </svg>
            </div>
            <span class="ml-4 transition-all duration-200 text-base truncate"
                  [class.opacity-0]="!isSidebarOpen()"
                  [class.pointer-events-none]="!isSidebarOpen()">
              Dashboard
            </span>
          </a>

          <a routerLink="/customer-portal/my-tickets" 
             routerLinkActive="bg-blue-500"
             class="flex items-center h-12 px-4 mx-2 text-lg font-semibold rounded-xl hover:bg-gray-700/50 transition-all whitespace-nowrap overflow-hidden"
             title="My Tickets"
          >
            <div class="w-8 h-8 flex-shrink-0 flex items-center justify-center">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 5v2m0 4v2m0 4v2M5 5a2 2 0 00-2 2v3a2 2 0 110 4v3a2 2 0 002 2h14a2 2 0 002-2v-3a2 2 0 110-4V7a2 2 0 00-2-2H5z"></path>
              </svg>
            </div>
            <span class="ml-4 transition-all duration-200 text-base truncate"
                  [class.opacity-0]="!isSidebarOpen()"
                  [class.pointer-events-none]="!isSidebarOpen()">
              My Tickets
            </span>
          </a>

          <a routerLink="/customer-portal/knowledge-base" 
             routerLinkActive="bg-blue-500"
             class="flex items-center h-12 px-4 mx-2 text-lg font-semibold rounded-xl hover:bg-gray-700/50 transition-all whitespace-nowrap overflow-hidden"
             title="Knowledge Base"
          >
            <div class="w-8 h-8 flex-shrink-0 flex items-center justify-center">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
              </svg>
            </div>
            <span class="ml-4 transition-all duration-200 text-base truncate"
                  [class.opacity-0]="!isSidebarOpen()"
                  [class.pointer-events-none]="!isSidebarOpen()">
              Knowledge Base
            </span>
          </a>
        </nav>
      </aside>

      <div class="flex-1 flex flex-col min-w-0 h-full overflow-hidden">

        <header class="flex items-center px-6 bg-white border-b border-gray-100 shadow-sm relative h-25 flex-shrink-0 z-30">

          <img src="log.png" class="h-30 w-auto object-contain" style="max-width: 200px;" />

          <div class="flex-1"></div>

          <button
            (click)="toggleMenu()"
            class="h-12 w-12 rounded-full bg-[#012A4A] text-white flex items-center justify-center font-semibold text-lg flex-shrink-0 cursor-pointer"
          >
            A
          </button>

          @if (isMenuOpen()) {
            <div class="absolute right-6 top-20 w-56 bg-white rounded-xl shadow-lg border z-50">
              <a routerLink="/customer-portal/profile"
                 (click)="closeMenu()"
                 class="flex items-center gap-3 px-5 py-3 hover:bg-gray-100">
                <img src="profile.png" class="w-5 h-5" />
                My Profile
              </a>

              <button
                (click)="logout()"
                class="w-full flex items-center gap-3 px-5 py-3 text-red-600 hover:bg-red-50 text-left cursor-pointer"
              >
                <img src="logout.png" class="w-5 h-5" />
                Logout
              </button>
            </div>
          }
        </header>

        <main class="flex-1 p-6 overflow-y-auto bg-slate-50">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `
})
export class CustomerLayoutComponent {
  private authService = inject(AuthService);

  isSidebarOpen = signal(true);
  isMenuOpen = signal(false);

  toggleSidebar() {
    this.isSidebarOpen.update(v => !v);
  }

  toggleMenu() {
    this.isMenuOpen.update(v => !v);
  }

  closeMenu() {
    this.isMenuOpen.set(false);
  }

  logout() {
    this.isMenuOpen.set(false);
    this.authService.logout();
  }

  @HostListener('document:click', ['$event'])
  closeOnOutsideClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('button') && !target.closest('a')) {
      this.isMenuOpen.set(false);
    }
  }
}