import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-customer-portal',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div
      class="min-h-screen bg-background flex flex-col items-center justify-center p-4 transition-colors duration-300"
    >
      <div
        class="bg-surface p-8 rounded-xl shadow-lg text-center max-w-md w-full border-t-4 border-primary"
      >
        <div
          class="w-16 h-16 bg-primary/10 text-primary rounded-full flex items-center justify-center mx-auto mb-4 text-2xl"
        >
          🏢
        </div>
        <h1 class="text-2xl font-bold text-text-main mb-2">Customer Portal</h1>
        <p class="text-text-muted mb-6">Welcome to the client self-service area.</p>

        <a
          routerLink="/login"
          class="text-primary hover:text-primary-hover font-semibold transition-colors"
        >
          &larr; Back to Login
        </a>
      </div>
    </div>
  `,
})
export class CustomerPortalComponent {}
