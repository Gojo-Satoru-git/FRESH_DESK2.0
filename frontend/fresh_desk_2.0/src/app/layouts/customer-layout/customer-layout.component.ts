import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-customer-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="flex min-h-screen">

      <!-- ================= SIDEBAR ================= -->
      <aside
        class="bg-[#012A4A] text-white transition-all duration-300 flex-shrink-0"
        [style.width]="isSidebarOpen() ? '260px' : '0px'"
        [class.overflow-hidden]="!isSidebarOpen()"
      >
        <!-- ☰ BUTTON (ONLY WHEN SIDEBAR OPEN) -->
        @if (isSidebarOpen()) {
          <div class="px-4 pt-4">
            <button
              (click)="toggleSidebar()"
              class="h-10 w-10 rounded-lg bg-white text-[#012A4A]
                     flex items-center justify-center font-bold"
            >
              ☰
            </button>
          </div>
        }

        <!-- NAV -->
        <nav class="space-y-3 mt-16 px-4">
          <a routerLink="/customer-portal" routerLinkActive="bg-blue-500"
             [routerLinkActiveOptions]="{ exact: true }"
             class="block px-6 py-3 text-xl font-semibold rounded-xl hover:bg-gray-700 whitespace-nowrap">
            Dashboard
          </a>

          <a routerLink="/customer-portal/my-tickets" routerLinkActive="bg-blue-500"
             class="block px-6 py-3 text-xl font-semibold rounded-xl hover:bg-gray-700 whitespace-nowrap">
            My Tickets
          </a>

          <a routerLink="/customer-portal/knowledge-base" routerLinkActive="bg-blue-500"
             class="block px-6 py-3 text-xl font-semibold rounded-xl hover:bg-gray-700 whitespace-nowrap">
            Knowledge Base
          </a>
        </nav>
      </aside>

      <!-- ================= RIGHT SIDE ================= -->
      <div class="flex-1 flex flex-col min-w-0">

        <!-- HEADER -->
        <header class="flex items-center px-4 py-1 bg-white border-b border-gray-100 shadow-sm relative h-30 flex-shrink-0">

          <!-- ☰ BUTTON (ONLY WHEN SIDEBAR CLOSED) -->
          @if (!isSidebarOpen()) {
            <button
              (click)="toggleSidebar()"
              class="h-10 w-10 mr-3 rounded-lg bg-gray-100
                     hover:bg-gray-200 flex items-center justify-center flex-shrink-0"
            >
              ☰
            </button>
          }

          <!-- LOGO — bigger, vertically centered -->
          <img src="log.png" class="h-18 w-auto object-contain" style="max-width: 200px;" />

          <div class="flex-1"></div>

          <!-- Avatar -->
          <button
            (click)="toggleMenu()"
            class="h-12 w-12 rounded-full bg-[#012A4A]
                   text-white flex items-center justify-center font-semibold text-lg flex-shrink-0"
          >
            A
          </button>

          <!-- PROFILE DROPDOWN -->
          @if (isMenuOpen()) {
            <div class="absolute right-6 top-16 w-56 bg-white rounded-xl shadow-lg border z-50">
              <a routerLink="/customer-portal/profile"
                 (click)="closeMenu()"
                 class="flex items-center gap-3 px-5 py-3 hover:bg-gray-100">
                <img src="profile.png" class="w-5 h-5" />
                My Profile
              </a>

              <button
                (click)="logout()"
                class="w-full flex items-center gap-3 px-5 py-3
                       text-red-600 hover:bg-red-50"
              >
                <img src="logout.png" class="w-5 h-5" />
                Logout
              </button>
            </div>
          }
        </header>

        <!-- CONTENT -->
        <main class="flex-1 p-6 overflow-auto">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `
})
export class CustomerLayoutComponent  {
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
    if (!target.closest('button')) {
      this.isMenuOpen.set(false);
    }
  }
}