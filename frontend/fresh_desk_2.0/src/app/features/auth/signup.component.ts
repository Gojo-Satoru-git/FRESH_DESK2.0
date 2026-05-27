import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { UiInputComponent } from '../../shared/components/ui-input/ui-input.component';
import { UiButtonComponent } from '../../shared/components/ui-button/ui-button.component';
import { passwordMatchValidator } from '../../core/validators/password-match.validator';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, UiInputComponent, UiButtonComponent],
  template: `t
    <div class="min-h-screen flex items-center justify-center bg-background p-4 relative transition-colors duration-300">
      

      <div class="max-w-xl w-full bg-surface rounded-xl shadow-lg p-8 space-y-6 transition-colors duration-300">
        
        <div class="text-center">
          <h2 class="text-2xl font-bold text-text-main">Create an Account</h2>
          <p class="text-sm text-text-muted mt-2">Register to access the customer support portal</p>
        </div>

        @if (errorMessage()) {
          <div class="bg-red-50 dark:bg-red-900/30 border-l-4 border-red-500 p-4 rounded-md">
            <p class="text-sm text-red-700 dark:text-red-400">{{ errorMessage() }}</p>
          </div>
        }

        @if (successMessage()) {
          <div class="bg-green-50 dark:bg-green-900/30 border-l-4 border-green-500 p-4 rounded-md">
            <p class="text-sm text-green-700 dark:text-green-400">{{ successMessage() }}</p>
          </div>
        }

        <form [formGroup]="signupForm" (ngSubmit)="onSubmit()" class="space-y-5">
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
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

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <app-ui-input
              id="password"
              label="Password"
              type="password"
              placeholder="••••••••"
              [control]="$any(signupForm.get('password'))"
            ></app-ui-input>

            <div class="space-y-1">
              <app-ui-input
                id="confirmPassword"
                label="Confirm Password"
                type="password"
                placeholder="••••••••"
                [control]="$any(signupForm.get('confirmPassword'))"
              ></app-ui-input>
              
              @if (signupForm.get('confirmPassword')?.touched && signupForm.get('confirmPassword')?.hasError('passwordMismatch')) {
                <span class="text-xs text-red-500 dark:text-red-400">Passwords do not match</span>
              }
            </div>
          </div>

          <app-ui-button 
            type="submit" 
            [disabled]="isLoading() || signupForm.invalid"
          >
            @if (isLoading()) {
              <svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            } @else {
              <span>CREATE ACCOUNT</span>
            }
          </app-ui-button>
          
          <div class="text-center mt-4">
            <p class="text-sm text-text-muted">
              Already have an account? 
              <a routerLink="/login" class="text-primary hover:text-primary-hover font-semibold transition-colors">
                Log in here
              </a>
            </p>
          </div>

        </form>
      </div>
    </div>
  `
})
export class SignupComponent {
  private router = inject(Router);

  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // The Form Group with the custom validator attached to the parent group
  signupForm = new FormGroup({
    firstName: new FormControl('', [Validators.required]),
    lastName: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(14)]),
    confirmPassword: new FormControl('', [Validators.required])
  }, { validators: passwordMatchValidator });

  onSubmit() {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    // MOCK API CALL
    setTimeout(() => {
      this.isLoading.set(false);
      
      // Simulate success and route them to login
      this.successMessage.set('Account created successfully! Redirecting to login...');
      
      setTimeout(() => {
        this.router.navigate(['/login']);
      }, 2000);

    }, 1500);
  }
}