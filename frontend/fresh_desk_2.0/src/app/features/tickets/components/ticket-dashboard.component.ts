import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TicketService } from '../services/ticket.service';
import { TicketDashboard, TicketListItem } from '../models/ticket.model';

@Component({
  selector: 'app-ticket-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="max-w-7xl mx-auto space-y-8 animate-fade-in pb-12">

      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 class="text-2xl font-bold text-text-main">Ticket Dashboard</h2>
          <p class="text-sm text-text-muted mt-1">Live support queue overview across all teams</p>
        </div>
        <button
          (click)="refresh()"
          class="bg-surface border border-gray-300 dark:border-gray-700 text-text-main px-4 py-2 rounded-lg text-sm font-medium hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors shadow-sm flex items-center gap-2">
          <svg class="w-4 h-4" [class.animate-spin]="loading()" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
          </svg>
          Refresh
        </button>
      </div>

      <!-- KPI Cards -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-5">
        <div class="bg-surface rounded-2xl p-6 border border-gray-200 dark:border-gray-800 shadow-sm hover:shadow-md transition-all cursor-pointer group"
             (click)="navigateToList('active')">
          <div class="flex items-center justify-between mb-3">
            <div class="w-10 h-10 rounded-xl bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
              </svg>
            </div>
            <svg class="w-4 h-4 text-text-muted opacity-0 group-hover:opacity-100 transition-opacity" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </div>
          <div class="text-3xl font-bold text-text-main">
            @if (loading()) { <span class="animate-pulse">—</span> } @else { {{ dashboard()?.totalActive ?? 0 }} }
          </div>
          <div class="text-sm text-text-muted mt-1 font-medium">Total Active</div>
        </div>

        <div class="bg-surface rounded-2xl p-6 border border-gray-200 dark:border-gray-800 shadow-sm hover:shadow-md transition-all cursor-pointer group"
             (click)="navigateToList('inprogress')">
          <div class="flex items-center justify-between mb-3">
            <div class="w-10 h-10 rounded-xl bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center">
              <svg class="w-5 h-5 text-indigo-600 dark:text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/>
              </svg>
            </div>
            <svg class="w-4 h-4 text-text-muted opacity-0 group-hover:opacity-100 transition-opacity" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </div>
          <div class="text-3xl font-bold text-indigo-600 dark:text-indigo-400">
            @if (loading()) { <span class="animate-pulse">—</span> } @else { {{ dashboard()?.inProgress ?? 0 }} }
          </div>
          <div class="text-sm text-text-muted mt-1 font-medium">In Progress</div>
        </div>

        <div class="bg-surface rounded-2xl p-6 border border-gray-200 dark:border-gray-800 shadow-sm hover:shadow-md transition-all cursor-pointer group"
             (click)="navigateToList('pending')">
          <div class="flex items-center justify-between mb-3">
            <div class="w-10 h-10 rounded-xl bg-yellow-100 dark:bg-yellow-900/30 flex items-center justify-center">
              <svg class="w-5 h-5 text-yellow-600 dark:text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
            </div>
            <svg class="w-4 h-4 text-text-muted opacity-0 group-hover:opacity-100 transition-opacity" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </div>
          <div class="text-3xl font-bold text-yellow-600 dark:text-yellow-400">
            @if (loading()) { <span class="animate-pulse">—</span> } @else { {{ dashboard()?.pendingReply ?? 0 }} }
          </div>
          <div class="text-sm text-text-muted mt-1 font-medium">Pending Reply</div>
        </div>

        <div class="bg-surface rounded-2xl p-6 border border-gray-200 dark:border-gray-800 shadow-sm hover:shadow-md transition-all cursor-pointer group"
             (click)="navigateToList('resolved')">
          <div class="flex items-center justify-between mb-3">
            <div class="w-10 h-10 rounded-xl bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
              <svg class="w-5 h-5 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
            </div>
            <svg class="w-4 h-4 text-text-muted opacity-0 group-hover:opacity-100 transition-opacity" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </div>
          <div class="text-3xl font-bold text-green-600 dark:text-green-400">
            @if (loading()) { <span class="animate-pulse">—</span> } @else { {{ dashboard()?.resolvedClosed ?? 0 }} }
          </div>
          <div class="text-sm text-text-muted mt-1 font-medium">Resolved / Closed</div>
        </div>
      </div>

      <!-- Recent Tickets -->
      <div class="bg-surface rounded-2xl border border-gray-200 dark:border-gray-800 shadow-sm">
        <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex items-center justify-between">
          <h3 class="text-lg font-bold text-text-main">Recent Tickets</h3>
          <button (click)="navigateToList()" class="text-sm font-medium text-primary hover:underline">View all</button>
        </div>

        @if (loadingRecent()) {
          <div class="p-8 text-center text-text-muted">
            <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin mx-auto mb-3"></div>
            <p class="text-sm">Loading tickets…</p>
          </div>
        } @else if (recentTickets().length === 0) {
          <div class="p-12 text-center">
            <div class="w-16 h-16 bg-gray-100 dark:bg-gray-800 rounded-2xl flex items-center justify-center mx-auto mb-4">
              <svg class="w-8 h-8 text-text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
              </svg>
            </div>
            <p class="text-text-muted font-medium">No tickets yet</p>
            <p class="text-sm text-text-muted mt-1">Create the first ticket to get started.</p>
          </div>
        } @else {
          <div class="divide-y divide-gray-100 dark:divide-gray-800">
            @for (ticket of recentTickets(); track ticket.id) {
              <div class="px-6 py-4 flex items-center gap-4 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors cursor-pointer group"
                   (click)="openTicket(ticket.id)">
                <div class="flex-shrink-0">
                  <span [class]="'inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-bold ' + getPriorityBadge(ticket.priority)">
                    {{ ticket.priority }}
                  </span>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-semibold text-text-main truncate group-hover:text-primary transition-colors">{{ ticket.title }}</p>
                  <p class="text-xs text-text-muted mt-0.5 truncate">{{ ticket.ticketNumber }} • {{ ticket.descriptionPreview }}</p>
                </div>
                <div class="flex-shrink-0 flex items-center gap-3">
                  <span [class]="'inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ' + getStatusBadge(ticket.status)">
                    {{ ticket.status }}
                  </span>
                  <span class="text-xs text-text-muted hidden sm:block">{{ formatDate(ticket.createdAt) }}</span>
                </div>
              </div>
            }
          </div>
        }
      </div>

      <!-- Error State -->
      @if (error()) {
        <div class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl p-4 flex items-center gap-3">
          <svg class="w-5 h-5 text-red-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
          </svg>
          <p class="text-sm text-red-700 dark:text-red-300">{{ error() }}</p>
        </div>
      }
    </div>
  `
})
export class TicketDashboardComponent implements OnInit {
  dashboard = signal<TicketDashboard | null>(null);
  recentTickets = signal<TicketListItem[]>([]);
  loading = signal(true);
  loadingRecent = signal(true);
  error = signal<string | null>(null);

  constructor(
    private ticketService: TicketService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  refresh() {
    this.loadData();
  }

  private loadData() {
    this.loading.set(true);
    this.loadingRecent.set(true);
    this.error.set(null);

    this.ticketService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load dashboard data. Please try again.');
        this.loading.set(false);
      }
    });

    this.ticketService.searchTickets({ page: 1, pageSize: 8 }).subscribe({
      next: (result) => {
        this.recentTickets.set(result.items ?? []);
        this.loadingRecent.set(false);
      },
      error: () => {
        this.loadingRecent.set(false);
      }
    });
  }

  navigateToList(filter?: string) {
    this.router.navigate(['/agent/tickets/list'], filter ? { queryParams: { status: filter } } : {});
  }

  openTicket(id: string) {
    this.router.navigate(['/agent/tickets', id]);
  }

  getPriorityBadge(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'critical': return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400';
      case 'high': return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400';
      case 'medium': return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400';
      case 'low': return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400';
      default: return 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-400';
    }
  }

  getStatusBadge(status: string): string {
    switch (status?.toLowerCase()) {
      case 'new': return 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400';
      case 'open': return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400';
      case 'inprogress': return 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400';
      case 'pending': return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400';
      case 'resolved': return 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400';
      case 'closed': return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
      case 'reopened': return 'bg-pink-100 text-pink-700 dark:bg-pink-900/30 dark:text-pink-400';
      default: return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
    }
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    const now = new Date();
    const diff = Math.floor((now.getTime() - d.getTime()) / 1000);
    if (diff < 60) return 'just now';
    if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
    return `${Math.floor(diff / 86400)}d ago`;
  }
}
