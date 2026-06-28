import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ThemeSwitcherComponent } from '../../core/theme/theme-switcher.component';
import { Router } from '@angular/router';
import { UiButtonComponent } from '../../shared/components/ui-button/ui-button.component';
import { AuthService } from '../../core/auth/auth.service';
import { TicketService } from '../tickets/services/ticket.service';
import { PERMISSIONS } from '../../core/auth/permission.constants';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, ThemeSwitcherComponent, UiButtonComponent],
  // EXACT SAME HTML YOU PROVIDED
  template: `<div
    class="min-h-screen flex justify-center bg-background transition-colors duration-300"
  >
    <div class="w-full flex">
      <div class="hidden md:flex flex-col w-1/2 bg-primary text-white relative p-12 lg:p-20">
        <header class="flex flex-col">
          <img
            src="img.png"
            alt="Customer Portal Logo"
            class="object-contain h-32 w-32 sm:h-40 sm:w-40 lg:h-48 lg:w-48"
          />
        </header>
        <div class="flex flex-col justify-center items-start w-full space-y-6 my-auto max-w-2xl">
          <h1 class="text-5xl lg:text-6xl font-medium tracking-tight leading-tight">
            A smarter way to connect and solve.
          </h1>
          <p class="text-xl lg:text-2xl text-white/80">
            Raise tickets, track resolutions, or manage the support queue in one secure hub.
          </p>
        </div>
        <div class="mt-auto"></div>
      </div>

      <div class="w-full md:w-1/2 flex items-center justify-center p-8 relative">
        <div class="absolute top-4 right-4"><app-theme-switcher></app-theme-switcher></div>
        <div class="w-full max-w-md">
          @if (activeBanner(); as banner) {
            <div
              class="mb-6 p-4 bg-blue-50 border border-blue-200 text-blue-800 rounded-xl flex items-center gap-3 shadow-sm text-left animate-fade-in"
            >
              <span class="text-2xl">📢</span>
              <div>
                <p class="font-bold text-sm text-slate-800">{{ banner.title }}</p>
                <p class="text-xs text-slate-600 mt-0.5">{{ banner.message }}</p>
              </div>
            </div>
          }
          <div class="mb-8 text-center">
            <p class="text-4xl font-medium tracking-wide leading-tight text-primary">
              Welcome to Adrenalin
            </p>
            <h3 class="text-xl text-text-muted mt-2">Sign in to access your dashboard</h3>
          </div>

          <form
            [formGroup]="loginForm"
            (ngSubmit)="onSubmit()"
            autocomplete="off"
            class="space-y-6"
          >
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="test@adrenalin.com"
              class="w-full px-4 py-3 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent transition bg-background text-text"
            />
            <input
              id="password"
              type="password"
              formControlName="password"
              placeholder="••••••••"
              class="w-full px-4 py-3 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent transition bg-background text-text"
            />
            <div class="mt-5">
              <app-ui-button type="submit" [disabled]="isLoading() || loginForm.invalid">
                @if (isLoading()) {
                  <svg class="animate-spin h-6 w-6 text-white" fill="none" viewBox="0 0 24 24">
                    <circle
                      class="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      stroke-width="4"
                    ></circle>
                    <path
                      class="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    ></path>
                  </svg>
                } @else {
                  <span>Sign in</span>
                }
              </app-ui-button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>`,
})
export class LoginComponent implements OnInit {
  private router = inject(Router);
  private authService = inject(AuthService);
  private ticketService = inject(TicketService);

  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  activeBanner = signal<any | null>(null);

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)]), // Reduced to 6 to match 'password' mock
  });

  ngOnInit() {
    this.ticketService.getActiveBanners().subscribe({
      next: (banners) => {
        if (banners?.length) this.activeBanner.set(banners[0]);
      },
      error: () => this.activeBanner.set(null),
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const email = this.loginForm.value.email!.trim().toLowerCase();
    const password = this.loginForm.value.password!;

    this.authService.login({ email, password }).subscribe({
      next: (res) => {
        const token = res.userId.accessToken;
        const user = this.authService.getUserFromToken(token);
        localStorage.setItem('jwt_token', token);
        if (user) {
          this.authService.currentUser.set(user);
          this.authService.loadUserMetadata().subscribe({
            next: () => {
              
              console.log('✅ User metadata loaded, permissions:', this.authService.permissions());
              this.isLoading.set(false);
              if (this.authService.hasAnyPermission([
                PERMISSIONS.DASHBOARD.READ,
                PERMISSIONS.DASHBOARD.WRITE,
                PERMISSIONS.DASHBOARD.ADMIN,
                PERMISSIONS.DASHBOARD.TEAMLEAD,
                PERMISSIONS.DASHBOARD.MANAGER
              ])) {
                this.router.navigate(['/workspace/dashboard']);
              } else {
                this.router.navigate(['/customer-portal']);
              }
            },
            error: (err) => {
              this.isLoading.set(false);
              console.error('Failed to load user metadata:', err);
              this.errorMessage.set('Failed to load user data. Please try again.');
            },
          });
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Invalid email or password.');
      },
    });
  }
}
