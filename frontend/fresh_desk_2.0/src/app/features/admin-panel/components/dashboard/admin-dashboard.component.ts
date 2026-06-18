import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-2">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">Admin Dashboard</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            System overview and quick links
          </p>
        </div>
      </div>

      <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 flex flex-col items-center justify-center text-center">
        <svg class="w-16 h-16 text-primary-blue mb-4 opacity-80" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
        </svg>
        <h3 class="text-xl font-bold font-heading text-text-dark dark:text-text-white mb-2">Welcome to the Adrenalin Admin Portal</h3>
        <p class="text-sm font-sans text-text-light dark:text-text-muted max-w-lg mb-6">
          Use the sidebar navigation to manage users, roles, permissions, routing groups, and registered companies.
        </p>
      </div>
    </div>
  `
})
export class AdminDashboardComponent {

}
