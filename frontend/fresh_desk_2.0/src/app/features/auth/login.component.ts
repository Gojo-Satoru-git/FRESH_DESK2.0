import { Component, Inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ThemeSwitcherComponent } from '../../core/theme/theme-switcher.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, ThemeSwitcherComponent], // Built-in Angular package
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private router = Inject(Router);
  // Local UI State using Signals
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
 
  // Strongly typed Reactive Form
  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(8)]),
  });

  onSubmit() {
    // 1. Stop if the form is invalid
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    // 2. Set loading state and clear old messages
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    // FAKE API CALL (Simulating the Traffic Cop)
    setTimeout(() => {
      this.isLoading.set(false);
      const enteredEmail = this.loginForm.value.email;

      // 1. Simulate an Agent Logging In
      if (enteredEmail === 'agent@adrenalin.com') {
        this.router.navigate(['/agent-dashboard']);
      }
      // 2. Simulate a Customer Logging In
      else if (enteredEmail === 'customer@company.com') {
        this.router.navigate(['/customer-portal']);
      }
      // 3. Error state
      else {
        this.errorMessage.set('Try "agent@adrenalin.com" or "customer@company.com"');
      }
    }, 1000); // 1 second fake delay to test the loading spinner
  }
}
