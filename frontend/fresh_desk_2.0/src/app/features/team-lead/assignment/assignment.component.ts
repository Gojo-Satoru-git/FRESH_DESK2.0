import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';
import { TeamLeadService, GroupDashboard, TicketListItem, GroupMember, AgentWorkload } from '../services/team-lead.service';
import { UiButtonComponent } from '../../../shared/components/ui-button/ui-button.component';

@Component({
  selector: 'app-team-lead-assignment',
  standalone: true,
  imports: [CommonModule, FormsModule, UiButtonComponent],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-2">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">Assignment Workspace</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Bulk assign tickets to agents in your group and manage workloads.
          </p>
        </div>

        <div class="flex items-center gap-4">
          @if (groups().length > 0) {
            <select 
              [(ngModel)]="selectedGroupId" 
              (change)="loadWorkspace()"
              class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg text-sm bg-surface text-text-main focus:ring-2 focus:ring-primary focus:border-primary transition-all">
              @for (group of groups(); track group.groupId) {
                <option [value]="group.groupId">{{ group.groupName }}</option>
              }
            </select>
          }
        </div>
      </div>

      <!-- Bulk Actions Bar -->
      @if (selectedTicketIds.size > 0) {
        <div class="bg-primary-blue/10 border border-primary-blue/30 rounded-xl p-4 flex items-center justify-between animate-fade-in">
          <div class="flex items-center gap-4">
            <span class="w-8 h-8 rounded-full bg-primary-blue flex items-center justify-center text-white font-bold text-sm">
              {{ selectedTicketIds.size }}
            </span>
            <span class="font-semibold text-primary-blue">Tickets Selected</span>
          </div>
          <div class="flex items-center gap-3">
            <select 
              [(ngModel)]="bulkAssignAgentId" 
              class="px-4 py-2 border border-primary-blue/30 rounded-lg text-sm bg-surface text-text-main focus:ring-2 focus:ring-primary focus:border-primary transition-all">
              <option value="">Select Agent to Assign...</option>
              @for (agent of groupAgents(); track agent.userId) {
                <option [value]="agent.userId">{{ agent.firstName }} {{ agent.lastName }}</option>
              }
            </select>
            <div class="w-32">
              <app-ui-button (click)="bulkAssign()" [disabled]="!bulkAssignAgentId">Assign</app-ui-button>
            </div>
          </div>
        </div>
      }

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Tickets Panel (Span 2) -->
        <div class="lg:col-span-2 bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm overflow-hidden flex flex-col h-[600px]">
          <div class="p-4 border-b border-table-dark-gray dark:border-gray-800 flex justify-between items-center bg-bg-light dark:bg-bg-dark">
            <h3 class="font-bold text-text-main">Unassigned Tickets</h3>
            <span class="text-xs font-bold px-2 py-1 bg-gray-200 dark:bg-gray-700 rounded-full text-text-light">{{ tickets().length }} items</span>
          </div>
          
          <div class="flex-1 overflow-y-auto">
            @if (loadingTickets()) {
              <div class="py-12 flex justify-center">
                <div class="w-8 h-8 rounded-full border-4 border-primary-blue border-t-transparent animate-spin"></div>
              </div>
            } @else {
              <table class="w-full text-left text-sm text-text-main">
                <thead class="sticky top-0 bg-table-light-gray dark:bg-gray-800 text-xs uppercase text-text-light dark:text-text-muted border-b border-table-dark-gray dark:border-gray-800 shadow-sm z-10">
                  <tr>
                    <th scope="col" class="px-4 py-3 w-12 text-center">
                      <input type="checkbox" (change)="toggleAll($event)" [checked]="allSelected()" class="rounded border-gray-300 text-primary-blue focus:ring-primary-blue">
                    </th>
                    <th scope="col" class="px-4 py-3 font-semibold">Ticket</th>
                    <th scope="col" class="px-4 py-3 font-semibold">Priority</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-table-dark-gray dark:divide-gray-800">
                  @for (ticket of tickets(); track ticket.id) {
                    <tr [class.bg-primary-blue_10]="selectedTicketIds.has(ticket.id)" class="hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors cursor-pointer" (click)="toggleSelection(ticket.id)">
                      <td class="px-4 py-3 text-center" (click)="$event.stopPropagation()">
                        <input type="checkbox" [checked]="selectedTicketIds.has(ticket.id)" (change)="toggleSelection(ticket.id)" class="rounded border-gray-300 text-primary-blue focus:ring-primary-blue">
                      </td>
                      <td class="px-4 py-3">
                        <div class="font-medium text-text-dark dark:text-text-white truncate max-w-[300px]">{{ ticket.title }}</div>
                        <div class="text-xs text-text-light mt-0.5">{{ ticket.ticketNumber }}</div>
                      </td>
                      <td class="px-4 py-3">
                        <span [class]="'px-2 py-0.5 text-[10px] font-bold rounded-full border ' + getPriorityClass(ticket.priority)">
                          {{ ticket.priority }}
                        </span>
                      </td>
                    </tr>
                  }
                  @if (tickets().length === 0) {
                    <tr>
                      <td colspan="3" class="px-6 py-12 text-center text-text-light">
                        No unassigned tickets found.
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
        </div>

        <!-- Agents Panel (Span 1) -->
        <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm overflow-hidden flex flex-col h-[600px]">
          <div class="p-4 border-b border-table-dark-gray dark:border-gray-800 bg-bg-light dark:bg-bg-dark">
            <h3 class="font-bold text-text-main">Agent Workload</h3>
          </div>
          
          <div class="flex-1 overflow-y-auto p-4 space-y-4">
            @if (loadingAgents()) {
              <div class="py-12 flex justify-center">
                <div class="w-8 h-8 rounded-full border-4 border-primary-blue border-t-transparent animate-spin"></div>
              </div>
            } @else {
              @for (agent of workloads(); track agent.userId) {
                <div class="p-4 rounded-xl border border-table-dark-gray dark:border-gray-800 bg-bg-light dark:bg-bg-dark hover:border-primary-blue/50 transition-colors">
                  <div class="flex items-center gap-3 mb-3">
                    <div class="w-10 h-10 rounded-full bg-primary-blue text-white flex items-center justify-center font-bold">
                      {{ agent.firstName.charAt(0) }}{{ agent.lastName.charAt(0) }}
                    </div>
                    <div>
                      <h4 class="font-bold text-text-dark dark:text-text-white text-sm leading-tight">{{ agent.firstName }} {{ agent.lastName }}</h4>
                      <div class="text-xs text-text-light mt-0.5 flex items-center gap-2">
                        <span>{{ agent.activeTicketsCount }} Active</span>
                        <span class="w-1 h-1 rounded-full bg-gray-400"></span>
                        <span [class]="getCapacityColor(agent.capacityPercentage)">{{ agent.capacityPercentage }}% Cap</span>
                      </div>
                    </div>
                  </div>
                  
                  <!-- Mini Progress Bar -->
                  <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-1.5 mb-1">
                    <div class="h-1.5 rounded-full" [class]="getCapacityBg(agent.capacityPercentage)" [style.width.%]="agent.capacityPercentage > 100 ? 100 : agent.capacityPercentage"></div>
                  </div>
                </div>
              }
              @if (workloads().length === 0) {
                <div class="py-8 text-center text-text-light">
                  No agents in this group.
                </div>
              }
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class AssignmentComponent implements OnInit {
  private http = inject(HttpClient);
  private teamLeadService = inject(TeamLeadService);
  
  groups = signal<GroupDashboard[]>([]);
  selectedGroupId = '';
  
  tickets = signal<TicketListItem[]>([]);
  groupAgents = signal<GroupMember[]>([]);
  workloads = signal<AgentWorkload[]>([]);
  
  loadingTickets = signal(true);
  loadingAgents = signal(true);
  
  selectedTicketIds = new Set<string>();
  bulkAssignAgentId = '';

  ngOnInit() {
    this.teamLeadService.getLeadDashboard().subscribe({
      next: (res) => {
        this.groups.set(res.groupDashboards);
        if (res.groupDashboards.length > 0) {
          this.selectedGroupId = res.groupDashboards[0].groupId;
          this.loadWorkspace();
        } else {
          this.loadingTickets.set(false);
          this.loadingAgents.set(false);
        }
      }
    });
  }

  loadWorkspace() {
    if (!this.selectedGroupId) return;
    
    this.selectedTicketIds.clear();
    this.bulkAssignAgentId = '';
    
    this.loadTickets();
    this.loadAgents();
  }

  loadTickets() {
    this.loadingTickets.set(true);
    // Fetch specifically unassigned queue for the active group
    this.teamLeadService.getGroupQueue(this.selectedGroupId, 'unassigned', 1, 50).subscribe({
      next: (res) => {
        this.tickets.set(res.tickets.items);
        this.loadingTickets.set(false);
      },
      error: () => this.loadingTickets.set(false)
    });
  }

  loadAgents() {
    this.loadingAgents.set(true);
    
    // Fetch members and workloads in parallel or sequence
    this.teamLeadService.getGroupMembers(this.selectedGroupId).subscribe({
      next: (res) => {
        this.groupAgents.set(res.members);
        
        // Also fetch workload
        this.teamLeadService.getGroupWorkload(this.selectedGroupId).subscribe({
          next: (workloads) => {
            this.workloads.set(workloads);
            this.loadingAgents.set(false);
          },
          error: () => this.loadingAgents.set(false)
        });
      },
      error: () => this.loadingAgents.set(false)
    });
  }

  toggleSelection(ticketId: string) {
    if (this.selectedTicketIds.has(ticketId)) {
      this.selectedTicketIds.delete(ticketId);
    } else {
      this.selectedTicketIds.add(ticketId);
    }
  }

  toggleAll(event: any) {
    if (event.target.checked) {
      this.tickets().forEach(t => this.selectedTicketIds.add(t.id));
    } else {
      this.selectedTicketIds.clear();
    }
  }

  allSelected(): boolean {
    return this.tickets().length > 0 && this.selectedTicketIds.size === this.tickets().length;
  }

  bulkAssign() {
    if (!this.bulkAssignAgentId || this.selectedTicketIds.size === 0) return;

    const payload = {
      ticketIds: Array.from(this.selectedTicketIds),
      agentId: this.bulkAssignAgentId
    };

    this.http.post(`${environment.apiUrl}/api/tickets/bulk-assign`, payload).subscribe({
      next: () => {
        this.loadWorkspace(); // Reload everything
      },
      error: (err) => {
        console.error('Bulk assign failed', err);
        alert('Failed to assign tickets.');
      }
    });
  }

  getPriorityClass(priority: string): string {
    const p = priority.toLowerCase();
    switch (p) {
      case 'urgent': return 'bg-error-red text-white border-error-red';
      case 'high': return 'bg-error-red/10 text-error-red border-error-red/20';
      case 'medium': return 'bg-warning-yellow/10 text-warning-yellow border-warning-yellow/20';
      case 'low': return 'bg-gray-100 text-gray-800 border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  getCapacityColor(cap: number): string {
    if (cap >= 90) return 'text-error-red font-bold';
    if (cap >= 75) return 'text-warning-yellow font-bold';
    return 'text-success-green font-bold';
  }

  getCapacityBg(cap: number): string {
    if (cap >= 90) return 'bg-error-red';
    if (cap >= 75) return 'bg-warning-yellow';
    return 'bg-success-green';
  }
}
