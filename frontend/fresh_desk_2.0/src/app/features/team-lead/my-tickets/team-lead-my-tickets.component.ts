import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface TicketItem {
  id: string;
  ticketNumber: string;
  title: string;
  status: string;
  priority: string;
  createdAt: string;
  assignedAgentName?: string;
}

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Component({
  selector: 'app-team-lead-my-tickets',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DatePipe],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">My Tickets</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Tickets assigned to you for resolution.
          </p>
        </div>
        <button
          (click)="loadTickets()"
          class="bg-surface border border-gray-300 dark:border-gray-700 text-text-main px-4 py-2 rounded-lg text-sm font-medium hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors shadow-sm flex items-center gap-2">
          <svg class="w-4 h-4" [class.animate-spin]="loading()" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
          </svg>
          Refresh
        </button>
      </div>

      <!-- Filter bar -->
      <div class="flex flex-wrap gap-3 items-center">
        <input
          type="text"
          [(ngModel)]="searchTerm"
          (ngModelChange)="onSearch()"
          placeholder="Search tickets..."
          class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg text-sm bg-surface text-text-main focus:ring-2 focus:ring-primary focus:border-primary transition-all w-56"
        />
        <select
          [(ngModel)]="filterStatus"
          (change)="onFilterChange()"
          class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg text-sm bg-surface text-text-main focus:ring-2 focus:ring-primary transition-all"
        >
          <option value="">All Statuses</option>
          <option value="New">New</option>
          <option value="Open">Open</option>
          <option value="InProgress">In Progress</option>
          <option value="Resolved">Resolved</option>
          <option value="Closed">Closed</option>
        </select>
        <span class="text-sm text-text-muted ml-auto">{{ totalCount() }} total</span>
      </div>

      <!-- Table -->
      <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm overflow-hidden">
        @if (loading()) {
          <div class="py-16 flex justify-center">
            <div class="w-8 h-8 rounded-full border-4 border-primary-blue border-t-transparent animate-spin"></div>
          </div>
        } @else if (error()) {
          <div class="py-8 text-center text-error-red">{{ error() }}</div>
        } @else {
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm text-text-main">
              <thead class="sticky top-0 bg-table-light-gray dark:bg-gray-800 text-xs uppercase text-text-light dark:text-text-muted border-b border-table-dark-gray dark:border-gray-800 shadow-sm">
                <tr>
                  <th class="px-6 py-4 font-semibold">#</th>
                  <th class="px-6 py-4 font-semibold">Subject</th>
                  <th class="px-6 py-4 font-semibold">Status</th>
                  <th class="px-6 py-4 font-semibold">Priority</th>
                  <th class="px-6 py-4 font-semibold">Created</th>
                  <th class="px-6 py-4 font-semibold text-right">Action</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-table-dark-gray dark:divide-gray-800">
                @for (ticket of tickets(); track ticket.id) {
                  <tr class="hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors">
                    <td class="px-6 py-4 text-xs text-text-light font-medium">{{ ticket.ticketNumber }}</td>
                    <td class="px-6 py-4">
                      <a [routerLink]="['/team-lead/tickets', ticket.id]" class="text-primary-blue hover:underline font-medium">
                        {{ ticket.title }}
                      </a>
                    </td>
                    <td class="px-6 py-4">
                      <span [class]="'px-2.5 py-1 text-xs font-bold rounded-full border ' + getStatusClass(ticket.status)">
                        {{ ticket.status }}
                      </span>
                    </td>
                    <td class="px-6 py-4">
                      <span [class]="'px-2.5 py-1 text-xs font-bold rounded-full border ' + getPriorityClass(ticket.priority)">
                        {{ ticket.priority }}
                      </span>
                    </td>
                    <td class="px-6 py-4 text-text-light whitespace-nowrap">{{ ticket.createdAt | date:'MMM d, y' }}</td>
                    <td class="px-6 py-4 text-right">
                      <a [routerLink]="['/team-lead/tickets', ticket.id]" class="text-primary-blue hover:underline font-medium text-xs">View</a>
                    </td>
                  </tr>
                }
                @if (tickets().length === 0) {
                  <tr>
                    <td colspan="6" class="px-6 py-8 text-center text-text-light">No tickets assigned to you.</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Pagination -->
      @if (totalPages() > 1 && !loading()) {
        <div class="flex justify-center gap-2 mt-6">
          <button [disabled]="page() === 1" (click)="changePage(page() - 1)"
            class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg disabled:opacity-50 text-text-main hover:bg-disabled-gray/30 transition-colors">
            Prev
          </button>
          <span class="px-4 py-2 text-text-main font-semibold">Page {{ page() }} of {{ totalPages() }}</span>
          <button [disabled]="page() === totalPages()" (click)="changePage(page() + 1)"
            class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg disabled:opacity-50 text-text-main hover:bg-disabled-gray/30 transition-colors">
            Next
          </button>
        </div>
      }
    </div>
  `
})
export class TeamLeadMyTicketsComponent implements OnInit {
  private http = inject(HttpClient);

  tickets = signal<TicketItem[]>([]);
  totalCount = signal(0);
  page = signal(1);
  readonly pageSize = 15;
  loading = signal(true);
  error = signal<string | null>(null);

  searchTerm = '';
  filterStatus = '';
  private searchTimeout: any;

  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize) || 1);

  ngOnInit() {
    this.loadTickets();
  }

  onSearch() {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.page.set(1);
      this.loadTickets();
    }, 300);
  }

  onFilterChange() {
    this.page.set(1);
    this.loadTickets();
  }

  loadTickets() {
    this.loading.set(true);
    this.error.set(null);

    let params = new HttpParams()
      .set('page', this.page().toString())
      .set('pageSize', this.pageSize.toString());

    if (this.filterStatus) params = params.set('status', this.filterStatus);
    if (this.searchTerm) params = params.set('term', this.searchTerm);

    this.http
      .get<PagedResult<TicketItem>>(`${environment.apiBaseUrl}/api/tickets/my-tickets`, { params })
      .subscribe({
        next: (res) => {
          this.tickets.set(res.items ?? []);
          this.totalCount.set(res.totalCount ?? 0);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to load your tickets.');
          console.error(err);
          this.loading.set(false);
        },
      });
  }

  changePage(newPage: number) {
    this.page.set(newPage);
    this.loadTickets();
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'new': return 'bg-primary-blue/10 text-primary-blue border-primary-blue/20';
      case 'open': return 'bg-blue-100 text-blue-800 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-800';
      case 'inprogress': return 'bg-warning-yellow/10 text-warning-yellow border-warning-yellow/20';
      case 'resolved': return 'bg-success-green/10 text-success-green border-success-green/20';
      case 'closed': return 'bg-gray-100 text-gray-800 border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  getPriorityClass(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'urgent': return 'bg-error-red text-white border-error-red';
      case 'high': return 'bg-error-red/10 text-error-red border-error-red/20';
      case 'medium': return 'bg-warning-yellow/10 text-warning-yellow border-warning-yellow/20';
      default: return 'bg-gray-100 text-gray-800 border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700';
    }
  }
}