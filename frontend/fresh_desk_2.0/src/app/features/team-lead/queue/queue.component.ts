import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TeamLeadService, GroupDashboard, TicketListItem } from '../services/team-lead.service';

@Component({
  selector: 'app-team-lead-queue',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DatePipe],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">Group Queue</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Monitor and prioritize tickets for your managed groups.
          </p>
        </div>

        <div class="flex items-center gap-3 flex-wrap">
          @if (groups().length > 1) {
            <select
              [(ngModel)]="selectedGroupId"
              (change)="onGroupChange()"
              class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg text-sm bg-surface text-text-main focus:ring-2 focus:ring-primary transition-all"
            >
              <option value="">All Group Tickets</option>
              @for (group of groups(); track group.groupId) {
                <option [value]="group.groupId">{{ group.groupName }}</option>
              }
            </select>
          } @else if (groups().length === 1) {
            <span class="px-4 py-2 bg-surface border border-gray-300 dark:border-gray-700 rounded-lg text-sm text-text-main">
              {{ groups()[0].groupName }}
            </span>
          }

          <select
            [(ngModel)]="queueType"
            (change)="loadQueue()"
            class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg text-sm bg-surface text-text-main focus:ring-2 focus:ring-primary transition-all"
          >
            <option value="all">All Tickets</option>
            <option value="unassigned">Unassigned</option>
            <option value="overdue">Overdue</option>
          </select>
        </div>
      </div>

      <!-- Ticket Table -->
      <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm overflow-hidden">
        @if (loading()) {
          <div class="py-16 flex justify-center">
            <div class="w-8 h-8 rounded-full border-4 border-primary-blue border-t-transparent animate-spin"></div>
          </div>
        } @else {
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm text-text-main">
              <thead class="sticky top-0 bg-table-light-gray dark:bg-gray-800 text-xs uppercase text-text-light dark:text-text-muted border-b border-table-dark-gray dark:border-gray-800 shadow-sm z-10">
                <tr>
                  <th scope="col" class="px-6 py-4 font-semibold">#</th>
                  <th scope="col" class="px-6 py-4 font-semibold">Subject</th>
                  <th scope="col" class="px-6 py-4 font-semibold">Status</th>
                  <th scope="col" class="px-6 py-4 font-semibold">Priority</th>
                  <th scope="col" class="px-6 py-4 font-semibold">Assigned To</th>
                  <th scope="col" class="px-6 py-4 font-semibold">Created</th>
                  <th scope="col" class="px-6 py-4 font-semibold text-right">Action</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-table-dark-gray dark:divide-gray-800">
                @for (ticket of tickets(); track ticket.id) {
                  <tr class="hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors">
                    <td class="px-6 py-4 font-medium text-xs text-text-light">{{ ticket.ticketNumber }}</td>
                    <td class="px-6 py-4">
                      <a [routerLink]="['/team-lead/tickets', ticket.id]" class="text-primary-blue hover:underline font-medium">
                        {{ ticket.title }}
                      </a>
                    </td>
                    <td class="px-6 py-4">
                      <span [class]="'px-2.5 py-1 text-xs font-bold rounded-full border ' + getStatusClass(ticket.status)">
                        {{ ticket.status }}
                      </span>
                    </td>
                    <td class="px-6 py-4">
                      <span [class]="'px-2.5 py-1 text-xs font-bold rounded-full border ' + getPriorityClass(ticket.priority)">
                        {{ ticket.priority }}
                      </span>
                    </td>
                    <td class="px-6 py-4">
                      @if (ticket.assignedAgentName) {
                        <div class="flex items-center gap-2">
                          <div class="w-6 h-6 rounded-full bg-primary-blue/20 text-primary-blue flex items-center justify-center text-[10px] font-bold">
                            {{ ticket.assignedAgentName.charAt(0) }}
                          </div>
                          <span>{{ ticket.assignedAgentName }}</span>
                        </div>
                      } @else {
                        <span class="text-warning-yellow font-medium text-xs bg-warning-yellow/10 px-2 py-0.5 rounded border border-warning-yellow/20">Unassigned</span>
                      }
                    </td>
                    <td class="px-6 py-4 text-text-light whitespace-nowrap">{{ ticket.createdAt | date:'MMM d, y, h:mm a' }}</td>
                    <td class="px-6 py-4 text-right">
                      <a [routerLink]="['/team-lead/tickets', ticket.id]" class="text-primary-blue hover:underline font-medium text-xs">View</a>
                    </td>
                  </tr>
                }
                @if (tickets().length === 0) {
                  <tr>
                    <td colspan="7" class="px-6 py-8 text-center text-text-light">
                      No tickets found in this queue.
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Pagination -->
      @if (totalPages() > 1 && !loading()) {
        <div class="flex justify-center gap-2 mt-6">
          <button
            [disabled]="page() === 1"
            (click)="changePage(page() - 1)"
            class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg disabled:opacity-50 text-text-main hover:bg-disabled-gray/30 transition-colors">
            Prev
          </button>
          <span class="px-4 py-2 text-text-main font-semibold">Page {{ page() }} of {{ totalPages() }}</span>
          <button
            [disabled]="page() === totalPages()"
            (click)="changePage(page() + 1)"
            class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg disabled:opacity-50 text-text-main hover:bg-disabled-gray/30 transition-colors">
            Next
          </button>
        </div>
      }
    </div>
  `
})
export class QueueComponent implements OnInit {
  private teamLeadService = inject(TeamLeadService);

  groups = signal<GroupDashboard[]>([]);
  selectedGroupId = '';
  queueType = 'all';

  tickets = signal<TicketListItem[]>([]);
  totalCount = signal(0);
  page = signal(1);
  pageSize = signal(15);
  loading = signal(true);

  ngOnInit() {
    this.teamLeadService.getLeadDashboard().subscribe({
      next: (res) => {
        this.groups.set(res.groupDashboards ?? []);
        if (res.groupDashboards?.length > 0) {
          this.selectedGroupId = res.groupDashboards[0].groupId;
          this.loadQueue();
        } else {
          this.loading.set(false);
        }
      },
      error: (err) => {
        console.error('Error loading groups', err);
        this.loading.set(false);
      },
    });
  }

  onGroupChange() {
    this.page.set(1);
    this.loadQueue();
  }

  loadQueue() {
    this.loading.set(true);
    this.teamLeadService
      .getGroupQueue(this.selectedGroupId, this.queueType, this.page(), this.pageSize())
      .subscribe({
        next: (res) => {
          this.tickets.set(res.tickets?.items ?? []);
          this.totalCount.set(res.tickets?.totalCount ?? 0);
          this.loading.set(false);
        },
        error: (err) => {
          console.error('Error loading queue', err);
          this.tickets.set([]);
          this.loading.set(false);
        },
      });
  }

  changePage(newPage: number) {
    this.page.set(newPage);
    this.loadQueue();
  }

  totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize()) || 1;
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'new': return 'bg-primary-blue/10 text-primary-blue border-primary-blue/20';
      case 'open': return 'bg-blue-100 text-blue-800 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-800';
      case 'inprogress': return 'bg-warning-yellow/10 text-warning-yellow border-warning-yellow/20';
      case 'resolved': return 'bg-success-green/10 text-success-green border-success-green/20';
      case 'closed': return 'bg-gray-100 text-gray-800 border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  getPriorityClass(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'urgent': return 'bg-error-red text-white border-error-red';
      case 'high': return 'bg-error-red/10 text-error-red border-error-red/20';
      case 'medium': return 'bg-warning-yellow/10 text-warning-yellow border-warning-yellow/20';
      case 'low': return 'bg-gray-100 text-gray-800 border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }
}