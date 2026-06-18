import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, UserSummary } from '../../services/admin.service';
import { UserCreateModalComponent } from './user-create-modal.component';
import { UiButtonComponent } from '../../../../shared/components/ui-button/ui-button.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule, UserCreateModalComponent, UiButtonComponent],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-2">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">User Management</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Manage agents, admins, and system access.
          </p>
        </div>

        <div class="w-40">
          <app-ui-button (click)="openCreateModal()">
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
              New User
            </div>
          </app-ui-button>
        </div>
      </div>

      <!-- Filters -->
      <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-4 flex gap-4 items-center">
        <div class="relative flex-1 max-w-sm">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg class="h-4 w-4 text-text-light" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input 
            type="text" 
            [(ngModel)]="searchEmail"
            (keyup.enter)="loadUsers()"
            placeholder="Search by email..."
            class="block w-full pl-10 pr-3 py-3 border border-gray-300 dark:border-gray-700 rounded-xl text-lg bg-surface text-text-main shadow-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-all"
          >
        </div>
        <div class="w-32">
          <app-ui-button (click)="loadUsers()">
            Search
          </app-ui-button>
        </div>
      </div>

      <!-- Users Table -->
      <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-table-dark-gray dark:divide-gray-800">
            <thead class="bg-table-light-gray dark:bg-gray-900/50">
              <tr>
                <th scope="col" class="px-6 py-4 text-left text-xs font-bold font-sans text-text-light dark:text-text-muted uppercase tracking-wider">User</th>
                <th scope="col" class="px-6 py-4 text-left text-xs font-bold font-sans text-text-light dark:text-text-muted uppercase tracking-wider">Status</th>
                <th scope="col" class="px-6 py-4 text-left text-xs font-bold font-sans text-text-light dark:text-text-muted uppercase tracking-wider">Email Verified</th>
                <th scope="col" class="px-6 py-4 text-left text-xs font-bold font-sans text-text-light dark:text-text-muted uppercase tracking-wider">Created</th>
                <th scope="col" class="px-6 py-4 text-right text-xs font-bold font-sans text-text-light dark:text-text-muted uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-table-dark-gray dark:divide-gray-800 bg-card-white dark:bg-surface">
              @if (isLoading()) {
                <tr>
                  <td colspan="5" class="px-6 py-8 text-center text-sm font-sans text-text-light">
                    <div class="flex items-center justify-center gap-2">
                      <div class="w-4 h-4 rounded-full border-2 border-primary-blue border-t-transparent animate-spin"></div>
                      Loading users...
                    </div>
                  </td>
                </tr>
              } @else if (users().length === 0) {
                <tr>
                  <td colspan="5" class="px-6 py-8 text-center text-sm font-sans text-text-light">
                    No users found matching your criteria.
                  </td>
                </tr>
              } @else {
                @for (user of users(); track user.id) {
                  <tr class="hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors">
                    <td class="px-6 py-4 whitespace-nowrap">
                      <div class="flex items-center">
                        <div class="flex-shrink-0 h-10 w-10 rounded-full bg-primary-blue/10 flex items-center justify-center text-primary-blue font-bold text-sm">
                          {{ user.firstName[0] }}{{ user.lastName[0] }}
                        </div>
                        <div class="ml-4">
                          <div class="text-sm font-bold font-sans text-text-dark dark:text-text-white">{{ user.firstName }} {{ user.lastName }}</div>
                          <div class="text-xs font-sans text-text-light">{{ user.email }}</div>
                        </div>
                      </div>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <span [class]="user.isActive ? 'bg-success-green/10 text-success-green' : 'bg-disabled-gray/20 text-text-light'" class="px-2.5 py-1 inline-flex text-xs leading-5 font-semibold rounded-full">
                        {{ user.isActive ? 'Active' : 'Inactive' }}
                      </span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-text-light">
                      @if (user.isEmailVerified) {
                        <svg class="w-5 h-5 text-success-green" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path></svg>
                      } @else {
                        <span class="text-xs text-text-light">Pending</span>
                      }
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-text-light">
                      {{ user.createdAt | date:'mediumDate' }}
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <a href="#" class="text-primary-blue hover:text-primary-hover hover:underline mr-4">Edit</a>
                    </td>
                  </tr>
                }
              }
            </tbody>
          </table>
        </div>
        <!-- Pagination -->
        <div class="bg-table-light-gray dark:bg-gray-900/50 px-6 py-3 flex items-center justify-between border-t border-table-dark-gray dark:border-gray-800">
          <div class="flex-1 flex justify-between sm:hidden">
            <button (click)="prevPage()" [disabled]="page() === 1" class="relative inline-flex items-center px-4 py-2 border border-outline-gray text-sm font-medium rounded-md text-text-dark bg-card-white hover:bg-disabled-gray disabled:opacity-50">Previous</button>
            <button (click)="nextPage()" [disabled]="page() === totalPages()" class="ml-3 relative inline-flex items-center px-4 py-2 border border-outline-gray text-sm font-medium rounded-md text-text-dark bg-card-white hover:bg-disabled-gray disabled:opacity-50">Next</button>
          </div>
          <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
            <div>
              <p class="text-sm font-sans text-text-light">
                Showing page <span class="font-bold text-text-dark dark:text-text-white">{{ page() }}</span> of <span class="font-bold text-text-dark dark:text-text-white">{{ totalPages() }}</span>
                ({{ totalCount() }} total users)
              </p>
            </div>
            <div>
              <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                <button (click)="prevPage()" [disabled]="page() === 1" class="relative inline-flex items-center px-2 py-2 rounded-l-md border border-outline-gray dark:border-gray-700 bg-card-white dark:bg-surface text-sm font-medium text-text-light hover:bg-table-light-gray dark:hover:bg-gray-800 disabled:opacity-50 transition-colors">
                  <span class="sr-only">Previous</span>
                  <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                    <path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd" />
                  </svg>
                </button>
                <button (click)="nextPage()" [disabled]="page() === totalPages() || totalPages() === 0" class="relative inline-flex items-center px-2 py-2 rounded-r-md border border-outline-gray dark:border-gray-700 bg-card-white dark:bg-surface text-sm font-medium text-text-light hover:bg-table-light-gray dark:hover:bg-gray-800 disabled:opacity-50 transition-colors">
                  <span class="sr-only">Next</span>
                  <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                    <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
                  </svg>
                </button>
              </nav>
            </div>
          </div>
        </div>
      </div>
    </div>

    <app-user-create-modal 
      [isOpen]="isModalOpen()" 
      (close)="isModalOpen.set(false)"
      (userCreated)="onUserCreated()">
    </app-user-create-modal>
  `
})
export class UserListComponent implements OnInit {
  private adminService = inject(AdminService);

  users = signal<UserSummary[]>([]);
  isLoading = signal(false);
  page = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);
  totalPages = signal(0);
  
  searchEmail = '';
  isModalOpen = signal(false);

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading.set(true);
    this.adminService.getUsers(this.page(), this.pageSize(), this.searchEmail).subscribe({
      next: (result) => {
        this.users.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading users', err);
        this.isLoading.set(false);
      }
    });
  }

  prevPage() {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      this.loadUsers();
    }
  }

  nextPage() {
    if (this.page() < this.totalPages()) {
      this.page.update(p => p + 1);
      this.loadUsers();
    }
  }

  openCreateModal() {
    this.isModalOpen.set(true);
  }

  onUserCreated() {
    this.isModalOpen.set(false);
    this.page.set(1);
    this.loadUsers();
  }
}
