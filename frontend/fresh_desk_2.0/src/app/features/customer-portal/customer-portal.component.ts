import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

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
        <p class="font-bold text-4xl mt-2">Hello, CUSTOMER</p>
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
            Your latest 4 tickets
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
              <th class="px-6 py-4 text-2xl font-bold">ID</th>
              <th class="px-6 py-4 text-2xl font-bold">Subject</th>
              <th class="px-6 py-4 text-2xl font-bold">Priority</th>
              <th class="px-6 py-4 text-2xl font-bold">Status</th>
              <th class="px-6 py-4 text-2xl font-bold">Last Updated</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-gray-200">
            @for (ticket of tickets(); track ticket.id) {
              <tr class="hover:bg-gray-50">
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
          </tbody>
        </table>
      </div>

    </section>
  `
})
export class CustomerPortalComponent {
  private router = inject(Router);

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

  openCount = () =>
    this.tickets().filter(t => t.status === 'Open').length;

  pendingCount = () =>
    this.tickets().filter(t => t.status === 'Pending').length;

  resolvedCount = () =>
    this.tickets().filter(t => t.status === 'Resolved').length;
}