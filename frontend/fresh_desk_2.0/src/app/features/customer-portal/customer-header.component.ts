// shared/customer-header/customer-header.component.ts
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
      <aside class="w-80  bg-[#012A4A] px-4 py-6 text-white">
        <div class="mb-10">
          <span class="text-4xl ml-10 font-bold block">Adrenalin</span>
          <span class="text-xl ml-10 opacity-80">Support Portal</span>
        </div>

        <nav class="space-y-3 mt-25">
          <a
            routerLink="/customer-portal"
            routerLinkActive="bg-blue-500"
            [routerLinkActiveOptions]="{ exact: true }"
            class="block px-6 py-3 text-xl font-semibold rounded-xl
                   transition hover:bg-gray-700"
          >
            Dashboard
          </a>

          <a
            routerLink="/customer-portal/raise-ticket"
            routerLinkActive="bg-blue-500"
            class="block px-6 py-3 text-xl font-semibold rounded-xl
                   transition hover:bg-gray-700"
          >
            New Ticket
          </a>

          <a
            routerLink="/customer-portal/my-tickets"
            routerLinkActive="bg-blue-500"
            class="block px-6 py-3 text-xl font-semibold rounded-xl
                   transition hover:bg-gray-700"
          >
            My Tickets
          </a>

          <a
            
            routerLink="/customer-portal/knowledge-base"
            routerLinkActive="bg-blue-500"
            class="block px-6 py-3 text-xl font-semibold rounded-xl
                   transition hover:bg-gray-700"
          >
            Knowledge Base
          </a>
        </nav>
      </aside>

      <!-- ================= RIGHT SIDE ================= -->
      <div class="flex-1 flex flex-col">

        <!-- HEADER BAR -->
        <header class="flex items-center px-6 py-4 bg-white relative">
          <img src="log.png" class="h-48 w-48 -mt-10 object-contain" />

          <div class="flex-1"></div>

          <!-- Notification -->
          <button
            class="h-10 w-12 rounded-xl bg-gray-100 -mt-10 hover:bg-blue-500
                   flex items-center justify-center transition"
          >
            <img src="notification.png" class="h-5 w-5" />
          </button>

          <!-- Avatar -->
          <button
            (click)="toggleMenu()"
            class="ml-4  h-14 w-14 rounded-full -mt-10 bg-[#012A4A]
                   text-white flex items-center justify-center
                   font-semibold relative z-20"
          >
            A
          </button>

          <!-- PROFILE DROPDOWN -->
         <!-- PROFILE DROPDOWN -->
@if (isMenuOpen()) {
  <div
    class="
      absolute right-6 top-30
      w-56
      bg-white
      rounded-xl
      shadow-lg
      border border-gray-200
      z-10
    "
  >

    <!-- My Profile -->
    <a
      routerLink="/customer-portal/profile"
      (click)="closeMenu()"
      class="
        flex items-center gap-3
        px-5 py-3
        text-lg font-medium
        text-gray-700
        hover:bg-gray-100
        rounded-t-xl
      "
    >
      <img
        src="profile.png"
        alt="Profile"
        class="w-5 h-5 object-contain"
      />
      <span>My Profile</span>
    </a>

    <!-- Logout -->
    <button
      (click)="logout()"
      class="
        w-full
        flex items-center gap-3
        px-5 py-3
        text-lg font-medium
        text-red-600
        hover:bg-red-50
        rounded-b-xl
      "
    >
      <img
        src="logout.png"
        alt="Logout"
        class="w-5 h-5 object-contain"
      />
      <span>Logout</span>
    </button>

  </div>
}
        </header>

        <!-- PAGE CONTENT -->
        <main class="flex-1 p-6">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `
})
export class CustomerHeaderComponent {
  private router = inject(Router);
  private authService = inject(AuthService);
  isMenuOpen = signal(false);

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