import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface Ticket {
  id: string;
  subject: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  status: 'Open' | 'Pending' | 'Resolved' | 'Closed';
  lastUpdated: string;
}

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="px-6 mt-6">

      <!-- ================= HEADER ================= -->
      <div class="mb-8">
        <h2 class="text-5xl font-bold text-[#012A4A]">My Tickets</h2>
        <p class="font-medium text-gray-500 text-xl mt-4">
          View all open, pending, resolved and closed tickets
        </p>
      </div>

      <!-- ================= FILTER + SEARCH ROW ================= -->
      <div class="flex flex-col lg:flex-row items-center  justify-between gap-6 mb-6">

        <!-- Status Filters -->
        <div class="flex gap-3 flex-wrap bg-gray-200 rounded-xl p-2 text-gray-400 ">
          @for (s of statuses; track s) {
            <button
              (click)="selectedStatus.set(s)"
              class="px-6 py-2 rounded-xl text-xl  font-semibold transition"
              [class.bg-white]="selectedStatus() === s"
              [class.text-[#012A4A]]="selectedStatus() === s"
              [class.bg-gray-100]="selectedStatus() !== s"
              [class.text-gray-700]="selectedStatus() !== s"
              [class.hover:bg-gray-200]="selectedStatus() !== s"
            >
              {{ s }}
            </button>
          }
        </div>

        <!-- Search Box -->
        <div class="relative w-full shadow-2xl  rounded-xl lg:w-96">
          <!-- Search Icon -->
          <img
            src="search.png"
            alt="search"
            class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 opacity-60"
          />

          <input
            type="text"
            placeholder="Search tickets..."
            class="
              w-full
              pl-12 pr-4 py-3
              text-lg
              rounded-xl
              
              
            "
            (input)="searchTerm.set($any($event.target).value)"
          />
        </div>
      </div>

      <!-- ================= TICKETS TABLE ================= -->
      <div class="overflow-x-auto bg-white rounded-2xl shadow-md border border-gray-200">
        <table class="w-full text-left border-collapse">
          <thead class="bg-gray-100 text-gray-700">
            <tr>
              <th class="px-6 py-4 text-2xl font-bold">ID</th>
              <th class="px-6 py-4 text-2xl font-bold">Subject</th>
              <th class="px-6 py-4 text-2xl font-bold">Priority</th>
              <th class="px-6 py-4 text-2xl font-bold">Status</th>
              <th class="px-6 py-4 text-2xl font-bold">Last Updated</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-gray-200">
            @for (ticket of filteredTickets(); track ticket.id) {
              <tr class="hover:bg-gray-50 transition">

                <td class="px-6 py-4 text-xl font-medium text-[#012A4A]">
                  #{{ ticket.id }}
                </td>

                <td class="px-6 py-4">
                  <a
                    [routerLink]="['/customer-portal/ticket', ticket.id]"
                    class="text-black font-medium text-2xl hover:underline"
                  >
                    {{ ticket.subject }}
                  </a>
                </td>

                <!-- Priority -->
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

                <!-- Status -->
                <td class="px-6 py-4">
                  <span
                    class="px-3 py-1 rounded-full text-xl font-medium"
                    [class.bg-green-100]="ticket.status === 'Open'"
                    [class.text-green-700]="ticket.status === 'Open'"
                    [class.bg-yellow-100]="ticket.status === 'Pending'"
                    [class.text-yellow-700]="ticket.status === 'Pending'"
                    [class.bg-gray-200]="ticket.status === 'Resolved' || ticket.status === 'Closed'"
                    [class.text-gray-700]="ticket.status === 'Resolved' || ticket.status === 'Closed'"
                  >
                    {{ ticket.status }}
                  </span>
                </td>

                <td class="px-6 py-4 text-xl text-gray-500">
                  {{ ticket.lastUpdated }}
                </td>

              </tr>
            }
          </tbody>
        </table>
      </div>

    </section>
  `
})
export class MyTicketsComponent {

  // Tabs
  statuses = ['All', 'Open', 'Pending', 'Resolved', 'Closed'];

  selectedStatus = signal<string>('All');
  searchTerm = signal<string>('');

  tickets = signal<Ticket[]>([
    {
      id: 'TKT-1042',
      subject: 'Payroll calculation mismatch for May cycle',
      priority: 'High',
      status: 'Open',
      lastUpdated: '2 hours ago'
    },
    {
      id: 'TCK-0987',
      subject: 'Invoice mismatch for April',
      priority: 'Medium',
      status: 'Pending',
      lastUpdated: 'Yesterday'
    },
    {
      id: 'TCK-0911',
      subject: 'Password reset request',
      priority: 'Low',
      status: 'Resolved',
      lastUpdated: '3 days ago'
    }
  ]);

  // Combined filter (status + search)
  filteredTickets = computed(() => {
    return this.tickets().filter(ticket => {
      const statusMatch =
        this.selectedStatus() === 'All' ||
        ticket.status === this.selectedStatus();

      const search = this.searchTerm().toLowerCase();
      const searchMatch =
        ticket.id.toLowerCase().includes(search) ||
        ticket.subject.toLowerCase().includes(search);

      return statusMatch && searchMatch;
    });
  });
}