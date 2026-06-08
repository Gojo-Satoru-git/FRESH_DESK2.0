import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { UiInputComponent } from '../../shared/components/ui-input/ui-input.component';
import { UiButtonComponent } from '../../shared/components/ui-button/ui-button.component';
import { passwordMatchValidator } from '../../core/validators/password-match.validator';
import { ThemeSwitcherComponent } from '../../core/theme/theme-switcher.component';
import { AuthService } from '../../core/auth/auth.service';
import { strongPasswordValidator } from '../../core/validators/strong-password.validator';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    UiInputComponent,
    UiButtonComponent,
    ThemeSwitcherComponent,
  ],
  template: `
    <div class="min-h-screen flex bg-background transition-colors duration-300">
      <div
        class="w-full lg:w-1/2 flex flex-col items-center justify-center p-8 sm:p-12 relative overflow-y-auto"
      >
        <div class="absolute top-6 left-6 lg:right-6 lg:left-auto">
          <app-theme-switcher></app-theme-switcher>
        </div>

        <div class="max-w-md w-full space-y-8 my-auto py-8">
          <div>
            <h2 class="text-3xl font-extrabold text-text-main tracking-tight">
              Create your account
            </h2>
            <p class="text-sm text-text-muted mt-2">
              Set up your profile to start submitting and tracking support requests.
            </p>
          </div>

          @if (errorMessage()) {
            <div class="bg-red-50 dark:bg-red-900/30 border-l-4 border-red-500 p-4 rounded-md">
              <p class="text-sm text-red-700 dark:text-red-400">{{ errorMessage() }}</p>
            </div>
          }
          @if (successMessage()) {
            <div
              class="bg-green-50 dark:bg-green-900/30 border-l-4 border-green-500 p-4 rounded-md"
            >
              <p class="text-sm text-green-700 dark:text-green-400">{{ successMessage() }}</p>
            </div>
          }

          <form [formGroup]="signupForm" (ngSubmit)="onSubmit()" class="space-y-6">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
              <app-ui-input
                id="firstName"
                label="First Name"
                placeholder="Jane"
                [control]="$any(signupForm.get('firstName'))"
              ></app-ui-input>
              <app-ui-input
                id="lastName"
                label="Last Name"
                placeholder="Doe"
                [control]="$any(signupForm.get('lastName'))"
              ></app-ui-input>
            </div>

            <app-ui-input
              id="email"
              label="Work Email"
              type="email"
              placeholder="jane.doe@company.com"
              [control]="$any(signupForm.get('email'))"
            ></app-ui-input>

            <div class="space-y-1">
              <app-ui-input
                id="phone"
                label="Phone Number"
                type="text"
                placeholder="1234567890"
                [control]="$any(signupForm.get('phone'))"
              ></app-ui-input>
              @if (
                signupForm.get('phone')?.touched &&
                signupForm.get('phone')?.invalid
              ) {
                <span class="text-xs text-red-500 dark:text-red-400">Phone number must be exactly 10 digits</span>
              }
            </div>

            <div class="space-y-5">
              <div class="space-y-1">
                <app-ui-input
                  id="password"
                  label="Password"
                  type="password"
                  placeholder="••••••••••••••"
                  [control]="$any(signupForm.get('password'))"
                ></app-ui-input>

                @if (
                  signupForm.get('password')?.touched &&
                  (signupForm.get('password')?.hasError('minlength') || signupForm.get('password')?.hasError('maxlength'))
                ) {
                  <span class="text-xs text-red-500 dark:text-red-400 block mt-1">Password must be between 12 and 14 characters</span>
                }

                @if (
                  signupForm.get('password')?.touched &&
                  signupForm.get('password')?.hasError('strongPassword')
                ) {
                  <div
                    class="text-xs text-red-500 dark:text-red-400 flex flex-col pl-2 border-l-2 border-red-500 mt-1 space-y-0.5"
                  >
                    @if (signupForm.get('password')?.errors?.['strongPassword'].missingUpper) {
                      <span>• Must contain at least 1 uppercase letter</span>
                    }
                    @if (signupForm.get('password')?.errors?.['strongPassword'].missingNumber) {
                      <span>• Must contain at least 1 number</span>
                    }
                    @if (signupForm.get('password')?.errors?.['strongPassword'].missingSpecial) {
                      <span>• Must contain at least 1 special character</span>
                    }
                  </div>
                }
              </div>

              <div class="space-y-1">
                <app-ui-input
                  id="confirmPassword"
                  label="Confirm Password"
                  type="password"
                  placeholder="••••••••••••••"
                  [control]="$any(signupForm.get('confirmPassword'))"
                ></app-ui-input>
                @if (
                  signupForm.get('confirmPassword')?.touched &&
                  signupForm.get('confirmPassword')?.hasError('passwordMismatch')
                ) {
                  <span class="text-xs text-red-500 dark:text-red-400">Passwords do not match</span>
                }
              </div>
            </div>

            <app-ui-button type="submit" [disabled]="isLoading() || signupForm.invalid">
              @if (isLoading()) {
                <svg class="animate-spin h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
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
                <span>Create Account</span>
              }
            </app-ui-button>
          </form>

          <div class="text-center mt-8 pt-6 border-t border-gray-200 dark:border-gray-800">
            <p class="text-sm text-text-muted">
              Already have an account?
              <a
                routerLink="/login"
                class="text-primary hover:text-primary-hover font-semibold transition-colors"
                >Log in here</a
              >
            </p>
          </div>
        </div>
      </div>

      <div
        class="hidden lg:flex lg:w-1/2 bg-surface relative overflow-hidden items-center justify-center border-l border-gray-200 dark:border-gray-800"
      >
        <div class="absolute inset-0 bg-primary/5"></div>
        <div class="absolute top-1/4 -left-24 w-80 h-80 bg-primary/20 rounded-full blur-3xl"></div>
        <div
          class="absolute bottom-1/4 -right-24 w-80 h-80 bg-primary/20 rounded-full blur-3xl"
        ></div>

        <div class="relative z-10 text-center max-w-md px-8">
          <div class="grid grid-cols-2 gap-4 mb-8 opacity-80">
            <div
              class="h-24 bg-background rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 flex flex-col justify-between"
            >
              <div class="h-2 w-1/2 bg-primary/40 rounded"></div>
              <div class="h-8 w-full bg-primary/10 rounded"></div>
            </div>
            <div
              class="h-24 bg-background rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 flex flex-col justify-between"
            >
              <div class="h-2 w-2/3 bg-primary/40 rounded"></div>
              <div class="h-8 w-full bg-primary/10 rounded"></div>
            </div>
          </div>
          <h3 class="text-3xl font-bold text-text-main mb-4">Always here to help</h3>
          <p class="text-lg text-text-muted">
            Get priority assistance, browse our knowledge base, and track your resolutions in
            real-time.
          </p>
        </div>
      </div>
    </div>
  `,
})
export class SignupComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // The Form Group with the custom validator attached to the parent group
  signupForm = new FormGroup(
    {
      firstName: new FormControl('', [Validators.required]),
      lastName: new FormControl('', [Validators.required]),
      email: new FormControl('', [Validators.required, Validators.email]),
      phone: new FormControl('', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]),
      password: new FormControl('', [
        Validators.required,
        Validators.minLength(12),
        Validators.maxLength(14),
        strongPasswordValidator
      ]),
      confirmPassword: new FormControl('', [Validators.required]),
    },
    { validators: passwordMatchValidator },
  );

  onSubmit() {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formValues = this.signupForm.value;
    const registerData = {
      firstName: formValues.firstName!,
      lastName: formValues.lastName!,
      email: formValues.email!,
      phone: formValues.phone!,
      password: formValues.password!,
      username: formValues.email!.split('@')[0] // auto-generate username from email prefix
    };

    this.authService.register(registerData).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.successMessage.set('Account created successfully! Redirecting to login...');
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.detail || err.error?.message || err.message || 'Registration failed. Please check your inputs.');
      }
    });
  }
}

