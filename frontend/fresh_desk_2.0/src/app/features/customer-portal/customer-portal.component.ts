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
      <div class="mx-6 mt-4 p-5 bg-blue-50 border border-blue-200 rounded-2xl flex items-center gap-4 shadow-sm animate-fade-in">
        <span class="text-3xl">📢</span>
        <div>
          <h4 class="font-bold text-slate-800 text-xl">{{ banner.title }}</h4>
          <p class="text-slate-600 text-base mt-1">{{ banner.message }}</p>
        </div>
      </div>
    }

    <section
      class="mx-6 mt-4 rounded-2xl px-6 py-8 flex items-center justify-between text-white bg-[#003151]"
    >
      <div>
        <p class="text-gray-300 text-xl font-medium">WELCOME BACK</p>
        <p class="font-bold text-4xl mt-2">
          Hello,
          {{ authService.currentUser()?.fullName ||
             authService.currentUser()?.email ||
             'Customer' }}
        </p>
        <p class="text-gray-200 text-lg mt-2">
          Here's what's happening. Raise your tickets today.
        </p>
      </div>

      <a
        (click)="openRaiseTicket()"
        class="flex items-center gap-3 px-5 py-3 bg-blue-400 rounded-xl font-bold text-xl hover:bg-[#025C94] transition cursor-pointer"
      >
        <img src="plu.png" class="w-6 h-6" />
        Raise a New Ticket
      </a>
    </section>

    <section class="px-6 mt-10 grid grid-cols-1 md:grid-cols-3 gap-6">
      <div class="relative bg-white rounded-2xl shadow-lg p-6 border border-gray-200">
        <img src="open.png" class="absolute bg-blue-200 top-10 right-6 w-16 h-16 rounded-xl p-4" />
        <p class="text-gray-600 text-2xl font-bold">OPEN TICKETS</p>
        <p class="text-5xl font-bold text-[#012A4A] mt-4">{{ openCount() }}</p>
      </div>

      <div class="relative bg-white rounded-2xl shadow-lg p-6 border border-gray-200">
        <img src="clock.png" class="absolute bg-orange-200 top-10 right-6 w-16 h-16 rounded-xl p-4" />
        <p class="text-gray-600 text-2xl font-bold">Pending Tickets</p>
        <p class="text-5xl font-bold text-yellow-600 mt-2">{{ pendingCount() }}</p>
      </div>

      <div class="relative bg-white rounded-2xl shadow-lg p-6 border border-gray-200">
        <img src="check.png" class="absolute bg-green-200 top-10 right-6 w-16 h-16 rounded-xl p-4" />
        <p class="text-gray-600 text-2xl font-bold">Resolved Tickets</p>
        <p class="text-5xl font-bold text-green-600 mt-2">{{ resolvedCount() }}</p>
      </div>
    </section>

    <section class="px-6 mt-16">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 px-6 py-4 bg-gray-50 border border-b-0 border-gray-200 rounded-t-2xl">
        <div>
          <p class="text-4xl font-bold text-[#012A4A]">Tickets by Organisation</p>
          <p class="text-gray-500 mt-2 text-2xl">Organisation latest tickets</p>
        </div>

        <div class="flex flex-wrap items-center gap-6">
          
          <div class="flex items-center gap-2">
            <label class="text-lg font-bold text-slate-500 uppercase tracking-wider">Status:</label>
            <select
              [(ngModel)]="selectedStatus"
              class="h-11 px-3 text-base border rounded-xl shadow-sm focus:outline-none font-bold min-w-[140px] cursor-pointer transition-colors duration-200"
              [ngClass]="{
                'bg-slate-100 border-slate-300 text-slate-700': selectedStatus() === 'All',
                'bg-emerald-100 border-emerald-300 text-emerald-800': selectedStatus() === 'Open',
                'bg-amber-100 border-amber-300 text-amber-800': selectedStatus() === 'Pending',
                'bg-blue-100 border-blue-300 text-blue-800': selectedStatus() === 'Resolved'
              }"
            >
              <option value="All" class="bg-white text-slate-700">All Statuses</option>
              <option value="Open" class="bg-white text-emerald-600 font-semibold">Open</option>
              <option value="Pending" class="bg-white text-amber-600 font-semibold">Pending</option>
              <option value="Resolved" class="bg-white text-blue-600 font-semibold">Resolved</option>
            </select>
          </div>

          <div class="flex items-center gap-2">
            <label class="text-lg font-bold text-slate-500 uppercase tracking-wider">Priority:</label>
            <select
              [(ngModel)]="selectedPriority"
              class="h-11 px-3 text-base border rounded-xl shadow-sm focus:outline-none font-bold min-w-[140px] cursor-pointer transition-colors duration-200"
              [ngClass]="{
                'bg-slate-100 border-slate-300 text-slate-700': selectedPriority() === 'All',
                'bg-green-50 border-green-200 text-green-700': selectedPriority() === 'Low',
                'bg-blue-50 border-blue-200 text-blue-700': selectedPriority() === 'Medium',
                'bg-orange-50 border-orange-200 text-orange-700': selectedPriority() === 'High',
                'bg-red-100 border-red-300 text-red-700': selectedPriority() === 'Critical'
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
            <label class="text-lg font-bold text-slate-500 uppercase tracking-wider">Created On:</label>
            <input
              type="date"
              [ngModel]="selectedDateFilter()"
              (ngModelChange)="selectedDateFilter.set($event)"
              class="h-11 px-3 text-base border border-slate-300 rounded-xl shadow-sm focus:outline-none font-bold min-w-[160px] cursor-pointer transition-colors duration-200"
              [ngClass]="{
                'bg-slate-100 text-slate-700': !selectedDateFilter(),
                'bg-purple-100 border-purple-300 text-purple-800': !!selectedDateFilter()
              }"
            />
            @if (selectedDateFilter()) {
              <button
                (click)="selectedDateFilter.set('')"
                class="text-slate-400 hover:text-slate-700 text-xl font-bold leading-none"
                title="Clear date filter"
              >✕</button>
            }
          </div>
          
        </div>
      </div>

      <div class="overflow-x-auto bg-white rounded-b-2xl shadow-md border border-gray-200">
        <table class="w-full text-left border-collapse">
          <thead class="bg-gray-100">
            <tr>
              <th class="px-6 py-4 text-2xl font-bold">ID/Number</th>
              <th class="px-6 py-4 text-2xl font-bold">Subject</th>
              <th class="px-6 py-4 text-2xl font-bold">Priority</th>
              <th class="px-6 py-4 text-2xl font-bold">Status</th>
              <th class="px-6 py-4 text-2xl font-bold">Last Updated</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-gray-200">
            @for (ticket of filteredTickets(); track ticket.id) {
              <tr class="hover:bg-gray-50">
                <td class="px-6 py-4 text-xl font-medium text-[#012A4A]">
                  {{ ticket.id }}
                </td>
                <td class="px-6 py-4 text-2xl font-medium">
                  {{ ticket.subject }}
                </td>
                <td class="px-6 py-4">{{ ticket.priority }}</td>
                <td class="px-6 py-4">{{ ticket.status }}</td>
                <td class="px-6 py-4 text-xl text-gray-500">
                  {{ ticket.lastUpdated }}
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="5" class="px-6 py-10 text-center text-xl text-gray-400">
                  No tickets match the selected filter criteria.
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </section>

    @if (showRaiseTicket()) {
      <div class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40" (click)="closeRaiseTicket()"></div>

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

          <div class="overflow-y-auto flex-1 [&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]">
            <app-raise-ticket (ticketCreated)="closeRaiseTicket()"></app-raise-ticket>
          </div>
        </div>
      </div>
    }
  `
})
export class CustomerPortalComponent implements OnInit {
  private ticketService = inject(TicketService);
  authService = inject(AuthService);

  tickets = signal<Ticket[]>([]);
  activeBanner = signal<any | null>(null);

  openCount = signal(0);
  pendingCount = signal(0);
  resolvedCount = signal(0);

  showRaiseTicket = signal(false);

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
      list = list.filter(t => t.status === statusFilter);
    }

    if (priorityFilter !== 'All') {
      list = list.filter(t => t.priority === priorityFilter);
    }

    if (dateFilter) {
      // dateFilter is a date string like '2025-06-10' from the date picker
      list = list.filter(t => {
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
      error: () => this.activeBanner.set(null)
    });
  }

  private loadTickets(): void {
    this.ticketService.getMyTickets(1, 10).subscribe(res => {
      const items = res.items || [];

      this.openCount.set(items.filter(t => t.status.toLowerCase() === 'open').length);
      this.pendingCount.set(items.filter(t => t.status.toLowerCase() === 'pending').length);
      this.resolvedCount.set(
        items.filter(t => ['resolved', 'closed'].includes(t.status.toLowerCase())).length
      );

      this.tickets.set(
        items.map(t => {
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
            rawDate: new Date(t.createdAt || t.updatedAt || new Date().toISOString()) // Safely applied current date fallback
          };
        })
      );
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