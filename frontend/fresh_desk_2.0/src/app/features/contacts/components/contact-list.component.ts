import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-contact-list',
  standalone: true,
  template: `
    <div class="h-full flex flex-col animate-fade-in relative">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
        <div>
          <h2 class="text-2xl font-bold text-text-main">Contacts</h2>
          <p class="text-sm text-text-muted mt-1">Manage your customers and organizations.</p>
        </div>

        <div class="flex items-center gap-3">
          <button
            class="px-4 py-2 bg-surface border border-gray-300 dark:border-gray-700 text-text-main font-medium rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors shadow-sm flex items-center gap-2"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"
              ></path>
            </svg>
            Import
          </button>
          <button
            class="px-4 py-2 bg-primary hover:bg-primary-hover text-white font-semibold rounded-lg transition-colors shadow-sm flex items-center gap-2"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 4v16m8-8H4"
              ></path>
            </svg>
            Add Contact
          </button>
        </div>
      </div>

      <div
        class="bg-surface border border-gray-200 dark:border-gray-800 rounded-t-xl p-4 flex flex-col sm:flex-row justify-between items-center gap-4"
      >
        <div class="relative w-full sm:w-96">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg
              class="w-4 h-4 text-text-muted"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
              ></path>
            </svg>
          </div>
          <input
            type="text"
            placeholder="Search contacts by name, email, or company..."
            class="w-full pl-10 pr-4 py-2 bg-background border border-gray-300 dark:border-gray-700 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary text-text-main placeholder:text-text-muted/60 transition-all"
          />
        </div>

        <div class="flex items-center gap-3 w-full sm:w-auto">
          <button
            class="flex items-center gap-2 text-sm font-medium text-text-muted hover:text-text-main px-3 py-2 border border-gray-200 dark:border-gray-700 rounded-lg transition-colors bg-background"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"
              ></path>
            </svg>
            Filter
          </button>
        </div>
      </div>

      <div
        class="bg-surface border-x border-b border-gray-200 dark:border-gray-800 rounded-b-xl overflow-hidden flex-1 flex flex-col shadow-sm"
      >
        <div class="overflow-x-auto flex-1">
          <table class="w-full text-sm text-left whitespace-nowrap">
            <thead
              class="text-xs text-text-muted uppercase bg-background border-b border-gray-200 dark:border-gray-800"
            >
              <tr>
                <th class="px-6 py-4 w-12">
                  <input
                    type="checkbox"
                    class="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary bg-transparent"
                  />
                </th>
                <th class="px-6 py-4 font-semibold">Name</th>
                <th class="px-6 py-4 font-semibold">Company</th>
                <th class="px-6 py-4 font-semibold">Phone</th>
                <th class="px-6 py-4 font-semibold">Open Tickets</th>
                <th class="px-6 py-4 font-semibold text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
              @for (contact of contacts(); track contact.id) {
                <tr class="hover:bg-background transition-colors group cursor-pointer">
                  <td class="px-6 py-4">
                    <input
                      type="checkbox"
                      class="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary bg-transparent"
                    />
                  </td>

                  <td class="px-6 py-4">
                    <div class="flex items-center gap-3">
                      <div
                        [class]="
                          'w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm ' +
                          contact.avatarColor
                        "
                      >
                        {{ contact.name.charAt(0) }}
                      </div>
                      <div>
                        <div
                          class="font-semibold text-text-main group-hover:text-primary transition-colors"
                        >
                          {{ contact.name }}
                        </div>
                        <div class="text-xs text-text-muted">{{ contact.email }}</div>
                      </div>
                    </div>
                  </td>

                  <td class="px-6 py-4">
                    <div class="flex items-center gap-2 text-text-main">
                      <svg
                        class="w-4 h-4 text-gray-400"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
                        ></path>
                      </svg>
                      {{ contact.company }}
                    </div>
                  </td>

                  <td class="px-6 py-4 text-text-muted">
                    {{ contact.phone }}
                  </td>

                  <td class="px-6 py-4">
                    @if (contact.openTickets > 0) {
                      <span
                        class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-xs font-semibold bg-red-50 text-red-700 dark:bg-red-900/30 dark:text-red-400 border border-red-100 dark:border-red-800/50"
                      >
                        <span class="w-1.5 h-1.5 rounded-full bg-red-500"></span>
                        {{ contact.openTickets }} Active
                      </span>
                    } @else {
                      <span class="text-text-muted text-xs">No active tickets</span>
                    }
                  </td>

                  <td class="px-6 py-4 text-right">
                    <button class="text-text-muted hover:text-primary transition-colors p-2">
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"
                        ></path>
                      </svg>
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <div
          class="border-t border-gray-200 dark:border-gray-800 p-4 flex items-center justify-between text-sm text-text-muted bg-background"
        >
          <div>Showing 1 to 5 of 24 contacts</div>
          <div class="flex items-center gap-2">
            <button
              class="px-3 py-1 border border-gray-300 dark:border-gray-700 rounded hover:bg-gray-200 dark:hover:bg-gray-800 disabled:opacity-50 transition-colors"
              disabled
            >
              Previous
            </button>
            <button
              class="px-3 py-1 border border-gray-300 dark:border-gray-700 rounded hover:bg-gray-200 dark:hover:bg-gray-800 transition-colors bg-background"
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ContactListComponent {
  // Mock Data perfectly formatted for the UI
  contacts = signal([
    {
      id: '1',
      name: 'Sarah Jenkins',
      email: 'sarah.j@techcorp.io',
      company: 'TechCorp Solutions',
      phone: '+1 (555) 123-4567',
      openTickets: 2,
      avatarColor: 'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-400',
    },
    {
      id: '2',
      name: 'Michael Chen',
      email: 'mchen@innovate.co',
      company: 'Innovate Logistics',
      phone: '+1 (555) 987-6543',
      openTickets: 0,
      avatarColor: 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-400',
    },
    {
      id: '3',
      name: 'Elena Rodriguez',
      email: 'erodriguez@designstudio.net',
      company: 'Creative Studio',
      phone: '+1 (555) 456-7890',
      openTickets: 1,
      avatarColor: 'bg-purple-100 text-purple-700 dark:bg-purple-900/40 dark:text-purple-400',
    },
    {
      id: '4',
      name: 'David Kim',
      email: 'dkim@healthplus.org',
      company: 'HealthPlus Medical',
      phone: '+1 (555) 222-3333',
      openTickets: 5,
      avatarColor: 'bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-400',
    },
    {
      id: '5',
      name: 'Amanda Foster',
      email: 'amanda@retailhub.com',
      company: 'Retail Hub Global',
      phone: '+1 (555) 888-9999',
      openTickets: 0,
      avatarColor: 'bg-teal-100 text-teal-700 dark:bg-teal-900/40 dark:text-teal-400',
    },
  ]);
}
