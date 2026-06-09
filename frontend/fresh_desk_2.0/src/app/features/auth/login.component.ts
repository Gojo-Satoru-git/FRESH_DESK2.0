import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ThemeSwitcherComponent } from '../../core/theme/theme-switcher.component';
import { Router, RouterLink } from '@angular/router';
import { UiButtonComponent } from '../../shared/components/ui-button/ui-button.component';
import { UiInputComponent } from '../../shared/components/ui-input/ui-input.component';
import { AuthService } from '../../core/auth/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ThemeSwitcherComponent,
    UiButtonComponent,
    UiInputComponent,
    RouterLink,
  ],
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnInit {
  private router = inject(Router);
  private authService = inject(AuthService);
  private http = inject(HttpClient);

  // Local UI State using Signals
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  activeBanner = signal<any | null>(null);

  ngOnInit() {
    this.loadActiveBanner();
  }

  private loadActiveBanner() {
    this.http.get<any[]>(`${environment.apiUrl}/api/kb/banners/active`).subscribe({
      next: (banners) => {
        if (banners && banners.length > 0) {
          this.activeBanner.set(banners[0]);
        }
      },
      error: () => this.activeBanner.set(null)
    });
  }

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

    const email = this.loginForm.value.email!;
    const password = this.loginForm.value.password!;

    this.authService.login({ email, password }).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        const token = res.userId.accessToken;
        
        // Decode token to build User object
        const user = this.authService.getUserFromToken(token);
        if (user) {
          this.authService.handleLoginSuccess(token, user);
        } else {
          this.errorMessage.set('Invalid token received from server.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || err.message || 'Invalid email or password.');
      }
    });
  }
}
