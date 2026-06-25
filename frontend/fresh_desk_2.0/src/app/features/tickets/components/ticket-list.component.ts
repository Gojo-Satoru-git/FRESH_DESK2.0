import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TicketService } from '../services/ticket.service';
import { TicketListItem, PagedResult } from '../models/ticket.model';
import { CreateTicketModalComponent } from './create-ticket-modal.component';

const STATUS_OPTIONS = [
  'All',
  'New',
  'Active',
  'Open',
  'Assigned',
  'In_Progress',
  'Pending',
  'Resolved',
  'Closed',
  'Reopened',
];
const PRIORITY_OPTIONS = ['All', 'Urgent', 'High', 'Medium', 'Low'];

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, FormsModule, CreateTicketModalComponent],
  template: `
    <div class="h-full flex flex-col animate-fade-in">
      <!-- Top Bar -->
      <div
        class="flex flex-col sm:flex-row justify-between items-start sm:items-center py-4 border-b border-gray-200 dark:border-gray-800 gap-4 mb-4"
      >
        <!-- Left: Tabs -->
        <div class="flex items-center gap-1">
          <button
            (click)="setView('all')"
            [class]="
              'px-4 py-2 text-sm font-semibold rounded-lg transition-all ' +
              (currentView() === 'all'
                ? 'bg-primary text-white shadow-lg shadow-primary/30'
                : 'text-text-muted hover:bg-gray-100 dark:hover:bg-gray-800')
            "
          >
            All Tickets
          </button>
          <button
            (click)="setView('my')"
            [class]="
              'px-4 py-2 text-sm font-semibold rounded-lg transition-all ' +
              (currentView() === 'my'
                ? 'bg-primary text-white shadow-lg shadow-primary/30'
                : 'text-text-muted hover:bg-gray-100 dark:hover:bg-gray-800')
            "
          >
            My Tickets
          </button>
          <button
            (click)="setView('assigned')"
            [class]="
              'px-4 py-2 text-sm font-semibold rounded-lg transition-all ' +
              (currentView() === 'assigned'
                ? 'bg-primary text-white shadow-lg shadow-primary/30'
                : 'text-text-muted hover:bg-gray-100 dark:hover:bg-gray-800')
            "
          >
            Assigned to Me
          </button>
        </div>

        <!-- Right: Actions -->
        <div class="flex items-center gap-3">
          <!-- <button
            (click)="showCreateModal.set(true)"
            class="flex items-center gap-2 bg-primary hover:bg-primary-hover text-white px-4 py-2 rounded-lg text-sm font-semibold shadow-lg shadow-primary/30 transition-all"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 4v16m8-8H4"
              />
            </svg>
            New Ticket
          </button> -->

          <span class="text-sm text-text-muted">{{ rangeText() }}</span>

          <div class="flex items-center">
            <button
              (click)="prevPage()"
              [disabled]="currentPage() === 1"
              class="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M15 19l-7-7 7-7"
                />
              </svg>
            </button>
            <button
              (click)="nextPage()"
              [disabled]="currentPage() >= totalPages()"
              class="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 5l7 7-7 7"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Search Bar -->
      <div class="mb-4">
  <div class="relative">
    <svg
      class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-text-muted pointer-events-none"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
    >
      <path
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
      />
    </svg>
    <input
      type="text"
      [(ngModel)]="searchTerm"
      (ngModelChange)="searchTerm.set($event)"
      placeholder="Search tickets by title or ID…"
      class="w-full pl-10 pr-4 py-2.5 bg-surface border border-gray-200 dark:border-gray-700 rounded-xl text-sm text-text-main placeholder:text-text-muted focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all"
    />
  </div>
</div>

      <!-- Main Body -->
      <div class="flex flex-1 overflow-hidden gap-5 pb-6">
        <!-- Ticket List -->
        <div class="flex-1 overflow-y-auto space-y-2 pr-1 scrollbar-hide">
          @if (loading()) {
            @for (i of [1, 2, 3, 4, 5]; track i) {
              <div
                class="bg-surface border border-gray-200 dark:border-gray-800 rounded-xl p-4 animate-pulse"
              >
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
                <div class="h-3 bg-gray-100 dark:bg-gray-800 rounded w-1/2"></div>
              </div>
            }
          } @else if (filteredTickets().length === 0) {
            <div class="flex flex-col items-center justify-center py-20 text-center">
              <div
                class="w-16 h-16 bg-gray-100 dark:bg-gray-800 rounded-2xl flex items-center justify-center mb-4"
              >
                <svg
                  class="w-8 h-8 text-text-muted"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="1.5"
                    d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                  />
                </svg>
              </div>
              <p class="font-semibold text-text-main">No tickets found</p>
              <p class="text-sm text-text-muted mt-1">
                Try adjusting your filters or create a new ticket.
              </p>
              <button
                (click)="showCreateModal.set(true)"
                class="mt-4 px-4 py-2 bg-primary text-white text-sm font-semibold rounded-lg shadow-lg shadow-primary/30 hover:bg-primary-hover transition-all"
              >
                Create Ticket
              </button>
            </div>
          } @else {
            @for (ticket of filteredTickets(); track ticket.id) {
              <div
                class="bg-surface border border-gray-200 dark:border-gray-800 rounded-xl p-4 flex gap-4 hover:shadow-md hover:border-primary/30 transition-all cursor-pointer group"
                [class.border-l-4]="selectedId() === ticket.id"
                [class.border-l-primary]="selectedId() === ticket.id"
                (click)="selectTicket(ticket)"
              >
                <!-- Avatar -->
                <div
                  class="flex-shrink-0 w-9 h-9 rounded-lg flex items-center justify-center font-bold text-sm"
                  [class]="getAvatarColor(ticket.id)"
                >
                  {{ (ticket.title || '?').charAt(0).toUpperCase() }}
                </div>

                <!-- Content -->
                <div class="flex-1 min-w-0">
                  <div class="flex items-start gap-2 mb-1">
                    <span
                      [class]="
                        'flex-shrink-0 inline-flex items-center px-2 py-0.5 rounded text-[10px] font-bold uppercase ' +
                        getPriorityBadge(ticket.priority)
                      "
                    >
                      {{ ticket.priority }}
                    </span>
                    <h4
                      class="text-sm font-semibold text-text-main truncate group-hover:text-primary transition-colors leading-tight"
                    >
                      {{ ticket.title }}
                    </h4>
                  </div>
                  <div class="text-xs text-text-muted flex items-center gap-2 flex-wrap">
                    <span class="font-mono text-primary/70">{{ ticket.ticketNumber }}</span>
                    <span>•</span>
                    <span class="truncate max-w-xs">{{ ticket.descriptionPreview }}</span>
                  </div>
                </div>

                <!-- Right meta -->
                <div class="flex-shrink-0 flex flex-col items-end justify-between gap-1">
                  <span
                    [class]="
                      'inline-flex px-2 py-0.5 rounded-full text-[10px] font-semibold ' +
                      getStatusBadge(ticket.status)
                    "
                  >
                    {{ getStatusLabel(ticket.status) }}
                  </span>
                  <span class="text-[10px] text-text-muted">{{
                    formatDate(ticket.createdAt)
                  }}</span>
                </div>
              </div>
            }
          }
        </div>

        <!-- Filter Panel -->
        <div
          class="hidden lg:flex w-64 flex-col bg-surface border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden flex-shrink-0 shadow-sm"
        >
          <div
            class="p-4 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center"
          >
            <span class="text-xs font-bold text-text-muted uppercase tracking-wider">Filters</span>
            <button
              (click)="resetFilters()"
              class="text-xs text-primary hover:underline font-medium"
            >
              Reset
            </button>
          </div>

          <div class="p-4 space-y-5 flex-1 overflow-y-auto">
            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-text-main">Status</label>
              <select
                [(ngModel)]="filterStatus"
                (ngModelChange)="filterStatus.set($event)"
                class="w-full bg-background border border-gray-300 dark:border-gray-700 text-text-main text-sm rounded-lg px-3 py-2 outline-none focus:ring-2 focus:ring-primary focus:border-primary"
              >
                @for (opt of statusOptions; track opt) {
                  <option [value]="opt">{{ opt }}</option>
                }
              </select>
            </div>

            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-text-main">Priority</label>
              <select
                [(ngModel)]="filterPriority"
                (ngModelChange)="filterPriority.set($event)"
                class="w-full bg-background border border-gray-300 dark:border-gray-700 text-text-main text-sm rounded-lg px-3 py-2 outline-none focus:ring-2 focus:ring-primary focus:border-primary"
              >
                @for (opt of priorityOptions; track opt) {
                  <option [value]="opt">{{ opt }}</option>
                }
              </select>
            </div>
          </div>

          <div class="p-4 border-t border-gray-200 dark:border-gray-800">
            <div class="text-xs text-text-muted text-center">
              Showing {{ filteredTickets().length }} of {{ allTickets().length }} tickets
            </div>
          </div>
        </div>
      </div>

      <!-- Error -->
      @if (error()) {
        <div
          class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl p-4 flex items-center gap-3 mb-4"
        >
          <svg
            class="w-5 h-5 text-red-500 flex-shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
          <p class="text-sm text-red-700 dark:text-red-300">{{ error() }}</p>
        </div>
      }
    </div>

    <!-- Create Ticket Modal -->
    <app-create-ticket-modal
      [isOpen]="showCreateModal()"
      (closeModal)="onModalClose()"
      (ticketCreated)="onTicketCreated()"
    />

    <!-- Toast notification -->
    @if (toastMessage()) {
      <div
        class="fixed bottom-6 right-6 bg-gray-900 text-white text-sm font-medium px-5 py-3 rounded-xl shadow-2xl animate-fade-in z-50 flex items-center gap-2"
      >
        <div
          class="w-2.5 h-2.5 rounded-full"
          [class.bg-emerald-400]="
            !toastMessage()!.toLowerCase().includes('failed') &&
            !toastMessage()!.toLowerCase().includes('error')
          "
          [class.bg-red-400]="
            toastMessage()!.toLowerCase().includes('failed') ||
            toastMessage()!.toLowerCase().includes('error')
          "
        ></div>
        {{ toastMessage() }}
      </div>
    }
  `,
})
export class TicketListComponent implements OnInit {
  /** All tickets loaded from the API (unfiltered) */
  allTickets = signal<TicketListItem[]>([]);
  /** Client-side filtered subset for display */
  filteredTickets = computed(() => {
    let list = this.allTickets();
    const term = this.searchTerm().trim().toLowerCase();
    if (term) {
      list = list.filter(
        (t) =>
          t.title.toLowerCase().includes(term) ||
          (t.ticketNumber ?? '').toLowerCase().includes(term) ||
          (t.descriptionPreview ?? '').toLowerCase().includes(term),
      );
    }
    const status = this.filterStatus();
    if (status === 'Active') {
      const activeStatuses = ['new', 'open', 'inprogress', 'reopened'];
      list = list.filter((t) => activeStatuses.includes(t.status.toLowerCase().replace(/_/g, '')));
    } else if (status && status !== 'All') {
      list = list.filter((t) => t.status.toLowerCase() === status.toLowerCase());
    }
    const priority = this.filterPriority();
    if (priority && priority !== 'All') {
      list = list.filter((t) => t.priority.toLowerCase() === priority.toLowerCase());
    }
    return list;
  });

  loading = signal(true);
  error = signal<string | null>(null);
  currentView = signal<'all' | 'my' | 'assigned'>('assigned');
  currentPage = signal(1);
  totalCount = signal(0);
  pageSize = 20;
  selectedId = signal<string | null>(null);
  showCreateModal = signal(false);
  toastMessage = signal<string | null>(null);

  showToast(msg: string) {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3000);
  }

  onTicketCreated() {
    this.showToast('Ticket created successfully!');
  }

  searchTerm = signal('');
  filterStatus = signal('All');
  filterPriority = signal('All');

  statusOptions = STATUS_OPTIONS;
  priorityOptions = PRIORITY_OPTIONS;

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));
  rangeText = computed(() => {
    const total = this.totalCount();
    const filtered = this.filteredTickets().length;
    if (total === 0) return '0 tickets';
    if (filtered < total) return `${filtered} filtered / ${total} total`;
    const start = (this.currentPage() - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage() * this.pageSize, total);
    return `${start}–${end} of ${total}`;
  });

  constructor(
    private ticketService: TicketService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      if (params['status']) {
        this.filterStatus.set(params['status']);
      }
      this.loadTickets();
    });
  }

  setView(view: 'all' | 'my' | 'assigned') {
    this.currentView.set(view);
    this.currentPage.set(1);
    this.loadTickets();
  }

  loadTickets() {
    this.loading.set(true);
    this.error.set(null);

    let obs;
    if (this.currentView() === 'all') {
      obs = this.ticketService.searchTickets({ page: this.currentPage(), pageSize: this.pageSize });
    } else if (this.currentView() === 'my') {
      obs = this.ticketService.getMyTickets(this.currentPage(), this.pageSize);
    } else {
      obs = this.ticketService.getAssignedTickets(this.currentPage(), this.pageSize);
    }

    obs.subscribe({
      next: (result) => {
        this.allTickets.set(result.items ?? []);
        this.totalCount.set(result.totalCount ?? 0);
        this.loading.set(false);
      },
      error: (err) => {
        const detail = err?.error?.detail ?? err?.error?.title ?? null;
        this.error.set(detail ?? 'Failed to load tickets. The API may be unavailable.');
        this.allTickets.set([]);
        this.loading.set(false);
      },
    });
  }


  resetFilters() {
    this.filterStatus.set('All');
    this.filterPriority.set('All');
    this.searchTerm.set('');
  }

  prevPage() {
    if (this.currentPage() > 1) {
      this.currentPage.update((p) => p - 1);
      this.loadTickets();
    }
  }

  nextPage() {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update((p) => p + 1);
      this.loadTickets();
    }
  }

  selectTicket(ticket: TicketListItem) {
    this.selectedId.set(ticket.id);
    this.router.navigate(['/agent/tickets', ticket.id]);
  }

  onModalClose() {
    this.showCreateModal.set(false);
    this.loadTickets(); // Refresh the list after ticket creation
  }

  getAvatarColor(id: string): string {
    const colors = [
      'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
      'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
      'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400',
      'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400',
      'bg-pink-100 text-pink-700 dark:bg-pink-900/30 dark:text-pink-400',
      'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400',
    ];
    let hash = 0;
    for (let i = 0; i < id.length; i++) hash = id.charCodeAt(i) + ((hash << 5) - hash);
    return colors[Math.abs(hash) % colors.length];
  }

  getPriorityBadge(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'urgent':
        return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400';
      case 'high':
        return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400';
      case 'medium':
        return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400';
      case 'low':
        return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400';
      default:
        return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
    }
  }

  getStatusBadge(status: string): string {
    switch (status?.toLowerCase()) {
      case 'new':
        return 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400';
      case 'open':
        return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400';
      case 'inprogress':
        return 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400';
      case 'assigned':
        return 'bg-cyan-100 text-cyan-700 dark:bg-cyan-900/30 dark:text-cyan-400';
      case 'pending':
        return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400';
      case 'resolved':
        return 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400';
      case 'closed':
        return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
      case 'reopened':
        return 'bg-pink-100 text-pink-700 dark:bg-pink-900/30 dark:text-pink-400';
      default:
        return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
    }
  }

  getStatusLabel(status: string): string {
    switch (status?.toLowerCase()) {
      case 'new':
        return 'Open';
      case 'open':
        return 'Assigned';
      default:
        return status;
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
