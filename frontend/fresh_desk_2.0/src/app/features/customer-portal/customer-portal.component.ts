import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CustomerHeaderComponent } from './customer-header.component';

// Define a safe interface for your ticket schema
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
  imports: [RouterLink, CustomerHeaderComponent], 
  template: `
    <!-- REUSABLE HEADER -->
    <app-customer-header></app-customer-header>

    <section
      class="
        h-50
        ml-30 mr-30 rounded-2xl
        px-6
        flex  
        items-center
        justify-between
        text-sm
        text-white
        bg-[#003151]
        border-b
      "
    >
      <div>
        <p class="text-gray-400 text-xl ml-4 mb-10 leading-tight font-medium">WELCOME BACK</p>
        <p class="font-bold  text-4xl ml-4 -mt-8 leading-tight ">Hello, CUSTOMER</p>
        <p class="text-gray-200 text-lg ml-4 mt-2">Here's what's happening raise your tickets today.</p>
      </div>
      <div [routerLink]="['/customer-portal/raise-ticket']"
        class="
          flex
          items-center
          gap-3 mr-8
          px-4 py-2
          bg-blue-400
          rounded-xl
          cursor-pointer
          hover:bg-[#025C94]
          transition
        "
      >
        <img src="plu.png" alt="Raise Ticket" class="w-6 h-6 object-contain " />
        <span class="font-bold text-xl text-white">Raise a New Ticket</span>
      </div>
    </section>

    <!-- MY TICKETS SECTION -->
    <section class="px-8 sm:px-12 md:px-16 lg:px-20 ml-10 mt-16">
      <div class="mb-6">
        <h2 class="text-5xl font-bold text-[#012A4A]">My Tickets</h2>
        <p class="font-medium text-gray-500 text-xl mt-4">View all open, pending, and resolved tickets</p>
      </div>

      <div class="overflow-x-auto bg-white rounded-2xl shadow-md border border-gray-200">
        <table class="w-full text-left border-collapse">
          <thead class="bg-gray-100 text-gray-700 text-lg">
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
              <tr class="hover:bg-gray-50 transition">
                <td class="px-6 py-4 text-xl font-medium text-[#012A4A]">
                  #{{ ticket.id }}
                </td>
                <td class="px-6 py-4">
                  <!-- Dynamically links directly to the detailed view using the item's custom ID -->
                  <a [routerLink]="['/customer-portal/ticket', ticket.id]" class="text-black font-medium text-2xl hover:underline cursor-pointer">
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
  `,
})
export class CustomerPortalComponent {
  private router = inject(Router);

  // Stored within an editable Signal Array to make future data changes effortless
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

  goToRaiseTicket() {
    this.router.navigate(['/customer-portal/raise-ticket']);
  }
}