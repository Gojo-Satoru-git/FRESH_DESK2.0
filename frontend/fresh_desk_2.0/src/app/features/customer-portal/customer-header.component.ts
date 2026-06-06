// shared//customer-header/customer-header.component.ts
import { Component, HostListener, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-customer-header',
  standalone: true,
  template: `
    <header class="flex items-center justify-between px-4 sm:px-8 md:px-12 lg:px-16 py-4">
      <!-- LEFT: LOGO -->
      <img
        src="log.png"
        alt="Customer Portal Logo"
        class="
          object-contain
          h-32 w-32
          sm:h-40 sm:w-40
          md:h-48 md:w-48
          lg:h-64 lg:w-64
          -mt-6 sm:-mt-8 md:-mt-10 lg:-mt-14
        "
      />

      <!-- RIGHT: ACTIONS -->
      <div class="flex items-center gap-4 relative">

        <!-- Notification Button -->
        <button
           class="
            h-10 w-12
            rounded-xl
            bg-gray-100
            hover:bg-blue-500
            active:bg-blue-500
            flex items-center justify-center
            mr-2
            -mt-6 sm:-mt-8 md:-mt-10 lg:-mt-20
            transition-colors duration-200
            focus:outline-none
          "
        >
          <img src="notification.png" alt="Notifications" class="h-5 w-5" />
        </button>

        <!-- Avatar -->
        <button
          (click)="toggleMenu()"
          class="
            h-14 w-14
            rounded-full
            bg-[#012A4A]
            text-white
            flex items-center justify-center
            font-semibold text-lg
            -mt-6 sm:-mt-8 md:-mt-10 lg:-mt-20
            focus:outline-none
          "
        >
          A
        </button>

        <!-- DROPDOWN -->
        @if (isMenuOpen()) {
          <div
            class="
              absolute right-0 top-2
              w-44
              bg-white
              rounded-lg
              shadow-lg
              border
              z-50
            "
          >
           <!-- My Profile -->
          <button
            class="
              w-full px-4 py-2
              flex items-center gap-3
              text-left
              hover:bg-gray-100
            "
            (click)="goToProfile()"
          >
            <img
              src="profile.png"
              alt="Profile"
              class="h-6 w-6 object-contain"
            />
            <span class="font-bold">My Profile</span>
          </button>

          <!-- Logout -->
          <button
            class="
              w-full px-4 py-2
              flex items-center gap-3
              text-left
              text-red-600
              hover:bg-gray-100
            "
            (click)="logout()"
          >
            <img
              src="logout.png"
              alt="Logout"
              class="h-6 w-6 object-contain"
            />
            <span class="font-bold">Logout</span>
          </button>
            
          </div>
        }
      </div>
    </header>
  `
})
export class CustomerHeaderComponent {
  private router = inject(Router);
  isMenuOpen = signal(false);

  toggleMenu() {
    this.isMenuOpen.update(v => !v);
  }

  goToProfile() {
    this.isMenuOpen.set(false);
    this.router.navigate(['/customer/profile']);
  }

  logout() {
    this.isMenuOpen.set(false);
    this.router.navigate(['/login']);
  }

  @HostListener('document:click', ['$event'])
  closeOnOutsideClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('button') && !target.closest('.shadow-lg')) {
      this.isMenuOpen.set(false);
    }
  }
}