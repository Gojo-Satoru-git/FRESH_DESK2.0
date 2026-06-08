import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TicketService } from '../tickets/services/ticket.service';
import { AuthService } from '../../core/auth/auth.service';

// Ticket interface
interface Ticket {
  id: string;
  subject: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  status: 'Open' | 'Pending' | 'Resolved';
  lastUpdated: string;
}

@Component({
  selector: 'app-customer-portal',
  standalone: true,
  imports: [RouterLink],
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

    <!-- ================= DASHBOARD BANNER ================= -->
    <section
      class="
        mx-6 mt-4
        rounded-2xl
        px-6 py-8
        flex items-center justify-between
        text-white
        bg-[#003151]
      "
    >
      <div>
        <p class="text-gray-300 text-xl font-medium">WELCOME BACK</p>
        <p class="font-bold text-4xl mt-2">Hello, {{ authService.currentUser()?.fullName || authService.currentUser()?.email || 'Customer' }}</p>
        <p class="text-gray-200 text-lg mt-2">
          Here's what's happening. Raise your tickets today.
        </p>
      </div>

      <a
        routerLink="/customer-portal/raise-ticket"
        class="
          flex items-center gap-3
          px-5 py-3
          bg-blue-400
          rounded-xl
          font-bold text-xl
          hover:bg-[#025C94]
          transition
        "
      >
        <img src="plu.png" class="w-6 h-6" />
        Raise a New Ticket
      </a>
    </section>

    <!-- ================= STATUS CARDS ================= -->
    <section class="px-6 mt-10 grid grid-cols-1 md:grid-cols-3 gap-6">

      <!-- Open Tickets -->
      <div
        class="
          relative
          bg-white
          rounded-2xl
          shadow-lg
          p-6
          border border-gray-200
        "
      >
        <img
          src="open.png"
          class="absolute bg-blue-200 top-10 right-6 w-16 h-16 rounded-xl p-4"
        />

        <p class="text-gray-600 text-2xl font-bold">OPEN TICKETS</p>
        <p class="text-5xl font-bold text-[#012A4A] mt-4">
          {{ openCount() }}
        </p>
      </div>

      <!-- Pending Tickets -->
      <div
        class="
          relative
          bg-white
          rounded-2xl
          shadow-lg
          p-6
          border border-gray-200
        "
      >
        <img
          src="clock.png"
          class="absolute bg-orange-200 top-10 right-6 w-16 h-16 rounded-xl p-4"
        />

        <p class="text-gray-600 text-2xl font-bold">Pending Tickets</p>
        <p class="text-5xl font-bold text-yellow-600 mt-2">
          {{ pendingCount() }}
        </p>
      </div>

      <!-- Resolved Tickets -->
      <div
        class="
          relative
          bg-white
          rounded-2xl
          shadow-lg
          p-6
          border border-gray-200
        "
      >
        <img
          src="check.png"
          class="absolute bg-green-200 top-10 right-6 w-16 h-16 rounded-xl p-4"
        />

        <p class="text-gray-600 text-2xl font-bold">Resolved Tickets</p>
        <p class="text-5xl font-bold text-green-600 mt-2">
          {{ resolvedCount() }}
        </p>
      </div>

    </section>

    <!-- ================= RECENT TICKETS ================= -->
    <section class="px-6 mt-16">

      <!-- Header Row -->
      <div
        class="
          flex items-center justify-between
          px-6 py-4
          bg-gray-50
          border border-b-0 border-gray-200
          rounded-t-2xl
        "
      >
        <div>
          <p class="text-4xl font-bold text-[#012A4A]">
            Recent Tickets
          </p>
          <p class="text-gray-500 mt-2 text-2xl">
            Your latest tickets
          </p>
        </div>

        <a
          routerLink="/customer-portal/my-tickets"
          class="
            px-6 py-2
            rounded-xl
            text-black
            text-lg
            font-semibold
            hover:bg-blue-600
            hover:text-white
            transition
          "
        >
          View All
        </a>
      </div>

      <!-- Table -->
      <div
        class="
          overflow-x-auto
          bg-white
          rounded-b-2xl
          shadow-md
          border border-gray-200
        "
      >
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
            @if (tickets().length === 0) {
              <tr>
                <td colspan="5" class="px-6 py-8 text-center text-xl text-gray-500">
                  No tickets found. Raise a new ticket to get started!
                </td>
              </tr>
            } @else {
              @for (ticket of tickets(); track ticket.id) {
                <tr class="hover:bg-gray-50">
                  <td class="px-6 py-4 text-xl font-medium text-[#012A4A]">
                    {{ ticket.id }}
                  </td>
                  <td class="px-6 py-4">
                    <a
                      [routerLink]="['/customer-portal/my-tickets']"
                      [queryParams]="{ id: ticket.id }"
                      class="text-black font-medium text-2xl hover:underline"
                    >
                      {{ ticket.subject }}
                    </a>
                  </td>
                   <td class="px-6 py-4">
                    <span
                      class="px-3 py-1 rounded-full text-xl font-medium"
                      [class.bg-red-100]="ticket.priority === 'High' || ticket.priority === 'Critical'"
                      [class.text-red-700]="ticket.priority === 'High' || ticket.priority === 'Critical'"
                      [class.bg-yellow-100]="ticket.priority === 'Medium'"
                      [class.text-yellow-700]="ticket.priority === 'Medium'"
                      [class.bg-green-100]="ticket.priority === 'Low'"
                      [class.text-green-700]="ticket.priority === 'Low'"
                    >
                      {{ ticket.priority }}
                    </span>
                  </td>

                  <td class="px-6 py-4">
                    <span
                      class="px-3 py-1 rounded-full text-xl font-medium"
                      [class.bg-green-100]="ticket.status === 'Open'"
                      [class.text-green-700]="ticket.status === 'Open'"
                      [class.bg-yellow-100]="ticket.status === 'Pending'"
                      [class.text-yellow-700]="ticket.status === 'Pending'"
                      [class.bg-gray-200]="ticket.status === 'Resolved'"
                      [class.text-gray-700]="ticket.status === 'Resolved'"
                    >
                      {{ ticket.status }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-xl text-gray-500">
                    {{ ticket.lastUpdated }}
                  </td>
                </tr>
              }
            }
          </tbody>
        </table>
      </div>

    </section>
  `
})
export class CustomerPortalComponent implements OnInit {
  private ticketService = inject(TicketService);
  authService = inject(AuthService);

  tickets = signal<Ticket[]>([]);
  activeBanner = signal<any | null>(null);

  openCount = signal<number>(0);
  pendingCount = signal<number>(0);
  resolvedCount = signal<number>(0);

  ngOnInit(): void {
    this.loadTickets();
    this.loadActiveBanner();
  }

  private loadActiveBanner(): void {
    this.ticketService.getActiveBanners().subscribe({
      next: (banners) => {
        if (banners && banners.length > 0) {
          this.activeBanner.set(banners[0]);
        } else {
          this.activeBanner.set(null);
        }
      },
      error: () => this.activeBanner.set(null)
    });
  }

  private loadTickets(): void {
    this.ticketService.getMyTickets(1, 10).subscribe({
      next: (res) => {
        const items = res.items || [];
        
        // Calculate counts from all customer's tickets
        this.openCount.set(items.filter(t => !['resolved', 'closed'].includes(t.status.toLowerCase()) && t.status.toLowerCase() !== 'pending').length);
        this.pendingCount.set(items.filter(t => t.status.toLowerCase() === 'pending').length);
        this.resolvedCount.set(items.filter(t => ['resolved', 'closed'].includes(t.status.toLowerCase())).length);

        // Display latest 4 tickets on the table
        const mapped = items.slice(0, 4).map(t => {
          let priority: 'Low' | 'Medium' | 'High' | 'Critical' = 'Medium';
          const p = t.priority.toLowerCase();
          if (p === 'low') priority = 'Low';
          else if (p === 'high') priority = 'High';
          else if (p === 'critical') priority = 'Critical';

          let status: 'Open' | 'Pending' | 'Resolved' = 'Open';
          const s = t.status.toLowerCase();
          if (['resolved', 'closed'].includes(s)) status = 'Resolved';
          else if (s === 'pending') status = 'Pending';

          return {
            id: t.ticketNumber || t.id,
            subject: t.title,
            priority,
            status,
            lastUpdated: t.updatedAt ? this.formatTimeAgo(t.updatedAt) : this.formatTimeAgo(t.createdAt)
          };
        });

        this.tickets.set(mapped);
      }
    });
  }

  private formatTimeAgo(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} minutes ago`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} days ago`;
  }
}