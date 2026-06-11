import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms'; // <-- Imported for [(ngModel)] binding
import { TicketService } from '../tickets/services/ticket.service';
import { AuthService } from '../../core/auth/auth.service';
import { RaiseTicketComponent } from '../customer-portal/raise-ticket.component';

interface Ticket {
  id: string;
  subject: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  status: 'Open' | 'Pending' | 'Resolved';
  lastUpdated: string;
  rawDate: Date; // Added to handle precise runtime date filtering
}

@Component({
  selector: 'app-customer-portal',
  standalone: true,
  imports: [CommonModule, RaiseTicketComponent, FormsModule], // Added FormsModule here
  template: `
    @if (activeBanner(); as banner) {
      <div
        class="mx-4 md:mx-6 mt-4 p-5 bg-blue-50 border border-blue-200 rounded-2xl flex flex-col sm:flex-row items-start sm:items-center gap-4 shadow-sm animate-fade-in"
      >
        <span class="text-3xl">📢</span>
        <div>
          <h4 class="font-bold text-slate-800 text-xl">{{ banner.title }}</h4>
          <p class="text-slate-600 text-base mt-1">{{ banner.message }}</p>
        </div>
      </div>
    }

    <section
      class="mx-4 md:mx-6 mt-4 rounded-2xl px-4 md:px-6 py-6 md:py-8 flex flex-col sm:flex-row sm:items-center justify-between text-white bg-[#003151]"
    >
      <div>
        <p class="text-gray-300 text-xl font-medium">WELCOME BACK</p>
        <p class="font-bold text-4xl mt-2">
          Hello,
          {{
            authService.currentUser()?.fullName || authService.currentUser()?.email || 'Customer'
          }}
        </p>
        <p class="text-gray-200 text-lg mt-2">Here's what's happening today.</p>
      </div>
    </section>

    <section
      class="px-4 md:px-6 mt-10 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6"
    >
      <div
        (click)="selectedStatus.set('All')"
        class="bg-white rounded-2xl shadow-lg p-6 border-2 cursor-pointer transition-all hover:shadow-xl flex items-center justify-between"
        [ngClass]="
          selectedStatus() === 'All'
            ? 'border-gray-500 scale-[1.02]'
            : 'border-gray-100 hover:border-gray-300'
        "
      >
        <div>
          <p class="text-gray-500 text-sm font-bold uppercase tracking-wider">ALL TICKETS</p>
          <p class="text-5xl font-bold text-gray-800 mt-2">{{ tickets().length }}</p>
        </div>
      </div>

      <div
        (click)="selectedStatus.set('Open')"
        class="bg-white rounded-2xl shadow-lg p-6 border-2 cursor-pointer transition-all hover:shadow-xl flex items-center justify-between"
        [ngClass]="
          selectedStatus() === 'Open'
            ? 'border-blue-500 scale-[1.02]'
            : 'border-gray-100 hover:border-blue-300'
        "
      >
        <div>
          <p class="text-gray-500 text-sm font-bold uppercase tracking-wider">OPEN TICKETS</p>
          <p class="text-5xl font-bold text-[#012A4A] mt-2">{{ openCount() }}</p>
        </div>
        <img src="open.png" class="bg-blue-100 w-14 h-14 rounded-xl p-3 shadow-sm" />
      </div>

      <div
        (click)="selectedStatus.set('Pending')"
        class="bg-white rounded-2xl shadow-lg p-6 border-2 cursor-pointer transition-all hover:shadow-xl flex items-center justify-between"
        [ngClass]="
          selectedStatus() === 'Pending'
            ? 'border-amber-500 scale-[1.02]'
            : 'border-gray-100 hover:border-amber-300'
        "
      >
        <div>
          <p class="text-gray-500 text-sm font-bold uppercase tracking-wider">PENDING TICKETS</p>
          <p class="text-5xl font-bold text-yellow-600 mt-2">{{ pendingCount() }}</p>
        </div>
        <img src="clock.png" class="bg-amber-100 w-14 h-14 rounded-xl p-3 shadow-sm" />
      </div>

      <div
        (click)="selectedStatus.set('Resolved')"
        class="bg-white rounded-2xl shadow-lg p-6 border-2 cursor-pointer transition-all hover:shadow-xl flex items-center justify-between"
        [ngClass]="
          selectedStatus() === 'Resolved'
            ? 'border-emerald-500 scale-[1.02]'
            : 'border-gray-100 hover:border-emerald-300'
        "
      >
        <div>
          <p class="text-gray-500 text-sm font-bold uppercase tracking-wider">RESOLVED TICKETS</p>
          <p class="text-5xl font-bold text-emerald-600 mt-2">{{ resolvedCount() }}</p>
        </div>
        <img src="check.png" class="bg-emerald-100 w-14 h-14 rounded-xl p-3 shadow-sm" />
      </div>
    </section>

    <section class="px-4 md:px-6 mt-16">
      <div
        class="flex flex-col md:flex-row md:items-center justify-between gap-4 px-4 md:px-6 py-4 bg-gray-50 border border-b-0 border-gray-200 rounded-t-2xl"
      >
        <div>
          <p class="text-4xl font-bold text-[#012A4A]">Tickets by Organisation</p>
          <p class="text-gray-500 mt-2 text-2xl">Organisation latest tickets</p>
        </div>

        <div class="flex flex-wrap items-center gap-6">
          <!-- Status filter removed because the summary cards above act as status filters -->

          <div class="flex items-center gap-2">
            <label class="text-lg font-bold text-slate-500 uppercase tracking-wider"
              >Priority:</label
            >
            <select
              [(ngModel)]="selectedPriority"
              class="h-11 px-3 text-base border rounded-xl shadow-sm focus:outline-none font-bold min-w-[140px] cursor-pointer transition-colors duration-200"
              [ngClass]="{
                'bg-slate-100 border-slate-300 text-slate-700': selectedPriority() === 'All',
                'bg-green-50 border-green-200 text-green-700': selectedPriority() === 'Low',
                'bg-blue-50 border-blue-200 text-blue-700': selectedPriority() === 'Medium',
                'bg-orange-50 border-orange-200 text-orange-700': selectedPriority() === 'High',
                'bg-red-100 border-red-300 text-red-700': selectedPriority() === 'Critical',
              }"
            >
              <option value="All" class="bg-white text-slate-700">All Priorities</option>
              <option value="Low" class="bg-white text-green-600">Low</option>
              <option value="Medium" class="bg-white text-blue-600">Medium</option>
              <option value="High" class="bg-white text-orange-600">High</option>
              <option value="Critical" class="bg-white text-red-600">Critical</option>
            </select>
          </div>

          <div class="flex items-center gap-2">
            <label class="text-lg font-bold text-slate-500 uppercase tracking-wider"
              >Created On:</label
            >
            <input
              type="date"
              [ngModel]="selectedDateFilter()"
              (ngModelChange)="selectedDateFilter.set($event)"
              class="h-11 px-3 text-base border border-slate-300 rounded-xl shadow-sm focus:outline-none font-bold min-w-[160px] cursor-pointer transition-colors duration-200"
              [ngClass]="{
                'bg-slate-100 text-slate-700': !selectedDateFilter(),
                'bg-purple-100 border-purple-300 text-purple-800': !!selectedDateFilter(),
              }"
            />
            @if (selectedDateFilter()) {
              <button
                (click)="selectedDateFilter.set('')"
                class="text-slate-400 hover:text-slate-700 text-xl font-bold leading-none"
                title="Clear date filter"
              >
                ✕
              </button>
            }
          </div>
        </div>
      </div>

      <div class="overflow-x-auto bg-white rounded-b-2xl shadow-sm border border-gray-200">
        <table class="w-full text-left border-collapse">
          <thead class="bg-gray-50/80 border-b border-gray-200">
            <tr>
              <th
                scope="col"
                class="px-6 py-4 text-sm font-bold text-gray-500 uppercase tracking-wider"
              >
                Ticket Details
              </th>
              <th
                scope="col"
                class="px-6 py-4 text-sm font-bold text-gray-500 uppercase tracking-wider"
              >
                Priority
              </th>
              <th
                scope="col"
                class="px-6 py-4 text-sm font-bold text-gray-500 uppercase tracking-wider"
              >
                Status
              </th>
              <th
                scope="col"
                class="px-6 py-4 text-sm font-bold text-gray-500 uppercase tracking-wider"
              >
                Activity
              </th>
              <th scope="col" class="px-6 py-4 relative"><span class="sr-only">Action</span></th>
            </tr>
          </thead>

          <tbody class="divide-y divide-gray-100 bg-white">
            @if (isLoading()) {
              @for (i of [1, 2, 3, 4, 5]; track i) {
                <tr class="animate-pulse">
                  <td class="px-6 py-5 whitespace-nowrap">
                    <div class="flex items-center gap-4">
                      <div class="h-11 w-11 rounded-full bg-slate-200"></div>
                      <div>
                        <div class="h-5 w-32 bg-slate-200 rounded-md mb-1.5"></div>
                        <div class="h-4 w-16 bg-slate-200 rounded-md"></div>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-5 whitespace-nowrap">
                    <div class="h-5 w-16 bg-slate-200 rounded-md"></div>
                  </td>
                  <td class="px-6 py-5 whitespace-nowrap">
                    <div class="h-6 w-20 bg-slate-200 rounded-full"></div>
                  </td>
                  <td class="px-6 py-5 whitespace-nowrap">
                    <div class="h-5 w-24 bg-slate-200 rounded-md"></div>
                  </td>
                  <td class="px-6 py-5 whitespace-nowrap text-right">
                    <div class="h-5 w-12 bg-slate-200 rounded-md ml-auto"></div>
                  </td>
                </tr>
              }
            } @else {
              @for (ticket of filteredTickets(); track ticket.id) {
                <tr class="hover:bg-blue-50/40 transition-colors group">
                  <!-- Col 1: Avatar + Title + ID -->
                  <td class="px-6 py-5 whitespace-nowrap">
                    <div class="flex items-center gap-4">
                      <div
                        class="h-11 w-11 flex-shrink-0 rounded-full bg-blue-50 flex items-center justify-center border border-blue-100 group-hover:border-blue-300 transition-colors text-blue-600 text-xl"
                      >
                        🎫
                      </div>
                      <div>
                        <div class="font-bold text-gray-900 text-lg">{{ ticket.subject }}</div>
                        <div class="text-gray-500 text-sm mt-0.5">#{{ ticket.id }}</div>
                      </div>
                    </div>
                  </td>

                  <!-- Col 2: Priority -->
                  <td class="px-6 py-5 whitespace-nowrap">
                    <div class="font-bold text-gray-900 text-base">{{ ticket.priority }}</div>
                    <div class="text-gray-500 text-sm mt-0.5">Level</div>
                  </td>

                  <!-- Col 3: Status Pill -->
                  <td class="px-6 py-5 whitespace-nowrap">
                    <span
                      class="inline-flex items-center rounded-md px-2.5 py-1 text-sm font-bold ring-1 ring-inset"
                      [ngClass]="{
                        'bg-blue-50 text-blue-700 ring-blue-600/20': ticket.status === 'Open',
                        'bg-amber-50 text-amber-700 ring-amber-600/20': ticket.status === 'Pending',
                        'bg-emerald-50 text-emerald-700 ring-emerald-600/20':
                          ticket.status === 'Resolved',
                      }"
                    >
                      {{ ticket.status }}
                    </span>
                  </td>

                  <!-- Col 4: Last Updated -->
                  <td class="px-6 py-5 whitespace-nowrap text-sm text-gray-500 font-medium">
                    {{ ticket.lastUpdated }}
                  </td>

                  <!-- Col 5: Action Link -->
                  <td class="px-6 py-5 whitespace-nowrap text-right text-sm font-bold">
                    <a
                      (click)="selectedTicketData.set(ticket)"
                      class="text-blue-600 hover:text-blue-800 transition-colors cursor-pointer tracking-wide"
                    >
                      View<span class="sr-only">, {{ ticket.id }}</span>
                    </a>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="5" class="py-16 text-center">
                    <span class="text-5xl block mb-4 text-gray-300">📭</span>
                    <p class="text-xl font-bold text-gray-800">No tickets found</p>
                    <p class="text-gray-500 mt-2 text-lg">
                      Adjust your filters above to see more results.
                    </p>
                  </td>
                </tr>
              }
            }
          </tbody>
        </table>
      </div>
    </section>

    @if (showRaiseTicket()) {
      <div
        class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40"
        (click)="closeRaiseTicket()"
      ></div>

      <div class="fixed inset-0 flex items-center justify-center z-50 pointer-events-none">
        <div
          class="bg-white w-[95%] md:w-[860px] max-h-[92vh] overflow-hidden rounded-2xl shadow-2xl border-2 border-slate-700 p-8 relative pointer-events-auto flex flex-col"
          (click)="$event.stopPropagation()"
        >
          <button
            class="absolute top-4 right-4 text-2xl font-bold text-gray-500 hover:text-black z-10"
            (click)="closeRaiseTicket()"
          >
            ✕
          </button>

          <div
            class="overflow-y-auto flex-1 [&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]"
          >
            <app-raise-ticket (ticketCreated)="closeRaiseTicket()"></app-raise-ticket>
          </div>
        </div>
      </div>
    }

    @if (selectedTicketData()) {
      <div
        class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40"
        (click)="selectedTicketData.set(null)"
      ></div>

      <div class="fixed inset-0 flex items-center justify-center z-50 pointer-events-none p-4">
        <div
          class="bg-white w-full max-w-2xl rounded-2xl shadow-2xl border border-gray-200 overflow-hidden relative pointer-events-auto flex flex-col"
          (click)="$event.stopPropagation()"
        >
          <!-- Header -->
          <div class="bg-[#012A4A] px-6 py-4 flex justify-between items-center text-white">
            <h2 class="text-xl font-bold">Ticket Details</h2>
            <button
              class="text-gray-300 hover:text-white transition-colors text-2xl font-bold"
              (click)="selectedTicketData.set(null)"
            >
              ✕
            </button>
          </div>

          <!-- Body -->
          <div class="p-8">
            <div class="flex items-center gap-5 mb-8">
              <div
                class="h-16 w-16 rounded-full bg-blue-50 border border-blue-100 flex items-center justify-center text-blue-600 text-3xl"
              >
                🎫
              </div>
              <div>
                <h3 class="text-3xl font-bold text-gray-900">{{ selectedTicketData().subject }}</h3>
                <p class="text-gray-500 text-lg font-medium mt-1">
                  Ticket #{{ selectedTicketData().id }}
                </p>
              </div>
            </div>

            <div
              class="grid grid-cols-1 sm:grid-cols-2 gap-4 sm:gap-6 bg-gray-50 p-4 sm:p-6 rounded-xl border border-gray-100"
            >
              <div>
                <p class="text-sm font-bold text-gray-500 uppercase tracking-wider">Priority</p>
                <p class="text-xl font-bold text-gray-900 mt-1">
                  {{ selectedTicketData().priority }}
                </p>
              </div>
              <div>
                <p class="text-sm font-bold text-gray-500 uppercase tracking-wider">Status</p>
                <p class="text-xl font-bold text-gray-900 mt-1">
                  <span
                    class="inline-flex items-center rounded-md px-3 py-1 text-sm font-bold ring-1 ring-inset"
                    [ngClass]="{
                      'bg-blue-50 text-blue-700 ring-blue-600/20':
                        selectedTicketData().status === 'Open',
                      'bg-amber-50 text-amber-700 ring-amber-600/20':
                        selectedTicketData().status === 'Pending',
                      'bg-emerald-50 text-emerald-700 ring-emerald-600/20':
                        selectedTicketData().status === 'Resolved',
                    }"
                  >
                    {{ selectedTicketData().status }}
                  </span>
                </p>
              </div>
              <div>
                <p class="text-sm font-bold text-gray-500 uppercase tracking-wider">Last Updated</p>
                <p class="text-xl font-bold text-gray-900 mt-1">
                  {{ selectedTicketData().lastUpdated }}
                </p>
              </div>
              <div>
                <p class="text-sm font-bold text-gray-500 uppercase tracking-wider">Created Date</p>
                <p class="text-xl font-bold text-gray-900 mt-1">
                  {{ selectedTicketData().rawDate | date: 'mediumDate' }}
                </p>
              </div>
            </div>

            @if (selectedTicketData().originalTicket?.description) {
              <div class="mt-8">
                <p class="text-sm font-bold text-gray-500 uppercase tracking-wider mb-2">
                  Description
                </p>
                <div
                  class="bg-gray-50 p-5 rounded-xl border border-gray-100 text-gray-800 whitespace-pre-wrap text-lg"
                >
                  {{ selectedTicketData().originalTicket.description }}
                </div>
              </div>
            }

            <div class="mt-8 flex justify-end">
              <button
                class="px-6 py-3 bg-gray-100 hover:bg-gray-200 text-gray-800 font-bold text-lg rounded-xl transition-colors"
                (click)="selectedTicketData.set(null)"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      </div>
    }
  `,
})
export class CustomerPortalComponent implements OnInit {
  private ticketService = inject(TicketService);
  authService = inject(AuthService);

  tickets = signal<Ticket[]>([]);
  activeBanner = signal<any | null>(null);
  isLoading = signal(true);

  openCount = signal(0);
  pendingCount = signal(0);
  resolvedCount = signal(0);

  showRaiseTicket = signal(false);
  selectedTicketData = signal<any | null>(null);

  // Active filter state elements
  selectedStatus = signal<string>('All');
  selectedPriority = signal<string>('All');
  selectedDateFilter = signal<string>(''); // Empty = no filter; date string = filter by that day

  // Computed array tracking automated runtime UI filters
  filteredTickets = computed(() => {
    let list = this.tickets();
    const statusFilter = this.selectedStatus();
    const priorityFilter = this.selectedPriority();
    const dateFilter = this.selectedDateFilter();

    if (statusFilter !== 'All') {
      list = list.filter((t) => t.status === statusFilter);
    }

    if (priorityFilter !== 'All') {
      list = list.filter((t) => t.priority === priorityFilter);
    }

    if (dateFilter) {
      // dateFilter is a date string like '2025-06-10' from the date picker
      list = list.filter((t) => {
        const ticketDate = t.rawDate.toISOString().slice(0, 10); // 'YYYY-MM-DD'
        return ticketDate === dateFilter;
      });
    }

    return list;
  });

  ngOnInit(): void {
    this.loadTickets();
    this.loadActiveBanner();
  }

  openRaiseTicket() {
    this.showRaiseTicket.set(true);
  }

  closeRaiseTicket() {
    this.showRaiseTicket.set(false);
  }

  private loadActiveBanner(): void {
    this.ticketService.getActiveBanners().subscribe({
      next: (b) => this.activeBanner.set(b?.[0] ?? null),
      error: () => this.activeBanner.set(null),
    });
  }

  private loadTickets(): void {
    this.isLoading.set(true);
    this.ticketService.getMyTickets(1, 100).subscribe((res) => {
      const items = res.items || [];

      const mappedTickets = items.map((t) => {
        let priority: Ticket['priority'] = 'Medium';
        const p = (t.priority || '').toLowerCase();
        if (p === 'low') priority = 'Low';
        else if (p === 'high') priority = 'High';
        else if (p === 'critical') priority = 'Critical';

        let status: Ticket['status'] = 'Open';
        const s = (t.status || '').toLowerCase();
        if (s === 'pending') status = 'Pending';
        else if (s === 'resolved' || s === 'closed') status = 'Resolved';

        return {
          id: t.ticketNumber || t.id,
          subject: t.title,
          priority,
          status,
          lastUpdated: this.formatTimeAgo(t.updatedAt || t.createdAt || new Date().toISOString()),
          rawDate: new Date(t.createdAt || t.updatedAt || new Date().toISOString()),
          originalTicket: t,
        };
      });

      this.tickets.set(mappedTickets);

      this.openCount.set(mappedTickets.filter((t) => t.status === 'Open').length);
      this.pendingCount.set(mappedTickets.filter((t) => t.status === 'Pending').length);
      this.resolvedCount.set(mappedTickets.filter((t) => t.status === 'Resolved').length);

      this.isLoading.set(false);
    });
  }

  private formatTimeAgo(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();

    const diffMins = Math.floor((now.getTime() - date.getTime()) / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} minutes ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;

    return `${Math.floor(diffHours / 24)} days ago`;
  }
}
