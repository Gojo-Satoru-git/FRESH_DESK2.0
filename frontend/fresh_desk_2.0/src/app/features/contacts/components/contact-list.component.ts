import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { TicketService } from '../../tickets/services/ticket.service';
import { PagedResult, TicketListItem } from '../../tickets/models/ticket.model';
import { environment } from '../../../../environments/environment';

interface UserSummaryDto {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  isActive: boolean;
  phone?: string;
}

interface ContactItem {
  id: string;
  name: string;
  email: string;
  company: string;
  phone: string;
  openTickets: number;
  avatarColor: string;
}

@Component({
  selector: 'app-contact-list',
  standalone: true,
  imports: [CommonModule],
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
            [value]="searchTerm()"
            (input)="onSearch($event)"
            placeholder="Search contacts by name or email..."
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
                    {{ contact.phone || 'None' }}
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
          <div>Showing {{ startIndex() }} to {{ endIndex() }} of {{ totalCount() }} contacts</div>
          <div class="flex items-center gap-2">
            <button
              [disabled]="page() === 1"
              (click)="onPrevious()"
              class="px-3 py-1 border border-gray-300 dark:border-gray-700 rounded hover:bg-gray-200 dark:hover:bg-gray-800 disabled:opacity-50 transition-colors"
            >
              Previous
            </button>
            <button
              [disabled]="page() * pageSize >= totalCount()"
              (click)="onNext()"
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
export class ContactListComponent implements OnInit {
  private http = inject(HttpClient);
  private ticketService = inject(TicketService);

  contacts = signal<ContactItem[]>([]);
  searchTerm = signal<string>('');
  page = signal<number>(1);
  pageSize = 10;
  totalCount = signal<number>(0);

  startIndex = signal<number>(0);
  endIndex = signal<number>(0);

  ngOnInit(): void {
    this.loadContacts();
  }

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.page.set(1);
    this.loadContacts();
  }

  onPrevious(): void {
    if (this.page() > 1) {
      this.page.update((p) => p - 1);
      this.loadContacts();
    }
  }

  onNext(): void {
    if (this.page() * this.pageSize < this.totalCount()) {
      this.page.update((p) => p + 1);
      this.loadContacts();
    }
  }

  private loadContacts(): void {
    let url = `${environment.apiBaseUrl}/api/rbac/users?pageNumber=${this.page()}&pageSize=${this.pageSize}`;
    if (this.searchTerm()) {
      url += `&email=${encodeURIComponent(this.searchTerm())}`;
    }

    // Fetch users and active tickets in parallel to calculate open tickets counts
    this.http.get<{ items: UserSummaryDto[]; totalCount: number }>(url).subscribe({
      next: (userResponse) => {
        const users = userResponse.items || [];
        this.totalCount.set(userResponse.totalCount || 0);

        this.startIndex.set(users.length > 0 ? (this.page() - 1) * this.pageSize + 1 : 0);
        this.endIndex.set(Math.min(this.page() * this.pageSize, this.totalCount()));

        this.ticketService.searchTickets({ page: 1, pageSize: 100 }).subscribe({
          next: (ticketResponse: PagedResult<TicketListItem>) => {
            const tickets = ticketResponse.items || [];
            // ticketResponse is now PagedResult<TicketListItem> — no 'any' cast needed
            const activeTickets = tickets.filter(
              (t) => !['resolved', 'closed'].includes(t.status.toLowerCase()),
            );

            const mappedContacts = users.map((user, index) => {
              const name = [user.firstName, user.lastName].filter(Boolean).join(' ') || user.email;
              const company = this.getCompanyFromEmail(user.email);
              const avatarColor = this.getAvatarColor(index);

              // Count unresolved tickets assigned to this user/agent
              const openTickets = activeTickets.filter((t) => t.assignedAgentId === user.id).length;

              return {
                id: user.id,
                name,
                email: user.email,
                company,
                phone: user.phone || '',
                openTickets,
                avatarColor,
              };
            });

            this.contacts.set(mappedContacts);
          },
          error: () => {
            // Fallback: If tickets fails, map with 0 open tickets
            const mappedContacts = users.map((user, index) => {
              const name = [user.firstName, user.lastName].filter(Boolean).join(' ') || user.email;
              return {
                id: user.id,
                name,
                email: user.email,
                company: this.getCompanyFromEmail(user.email),
                phone: user.phone || '',
                openTickets: 0,
                avatarColor: this.getAvatarColor(index),
              };
            });
            this.contacts.set(mappedContacts);
          },
        });
      },
    });
  }

  private getCompanyFromEmail(email: string): string {
    const domain = email.split('@')[1]?.toLowerCase() || '';
    if (domain.includes('techcorp')) return 'TechCorp Solutions';
    if (domain.includes('innovate')) return 'Innovate Logistics';
    if (domain.includes('designstudio')) return 'Creative Studio';
    if (domain.includes('healthplus')) return 'HealthPlus Medical';
    if (domain.includes('retailhub')) return 'Retail Hub Global';

    const namePart = domain.split('.')[0];
    if (namePart) {
      return namePart.charAt(0).toUpperCase() + namePart.slice(1) + ' Corp';
    }
    return 'External Partner';
  }

  private getAvatarColor(index: number): string {
    const colors = [
      'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-400',
      'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-400',
      'bg-purple-100 text-purple-700 dark:bg-purple-900/40 dark:text-purple-400',
      'bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-400',
      'bg-teal-100 text-teal-700 dark:bg-teal-900/40 dark:text-teal-400',
    ];
    return colors[index % colors.length];
  }
}
