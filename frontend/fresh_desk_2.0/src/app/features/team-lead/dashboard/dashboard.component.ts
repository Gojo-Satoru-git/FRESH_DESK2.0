import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TeamLeadService, LeadDashboard, AgentWorkloadApiDto } from '../services/team-lead.service';

@Component({
  selector: 'app-team-lead-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      <div class="mb-6">
        <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">Lead Dashboard</h2>
        <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
          Overview of ticket metrics across all your managed groups.
        </p>
      </div>

      @if (loading()) {
        <div class="py-12 flex justify-center">
          <div class="w-8 h-8 rounded-full border-4 border-primary-blue border-t-transparent animate-spin"></div>
        </div>
      } @else if (error()) {
        <div class="py-8 text-center text-error-red bg-error-red/5 border border-error-red/20 rounded-xl">
          {{ error() }}
        </div>
      } @else if (dashboard()) {
        <!-- Global Stats -->
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-5">
            <h3 class="text-xs font-semibold text-text-light dark:text-text-muted uppercase tracking-wider mb-1">Groups Managed</h3>
            <p class="text-2xl font-bold text-text-dark dark:text-text-white">{{ dashboard()!.totalGroupsManaged }}</p>
          </div>
          <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-5">
            <h3 class="text-xs font-semibold text-text-light dark:text-text-muted uppercase tracking-wider mb-1">Total Active Tickets</h3>
            <p class="text-2xl font-bold text-primary-blue">{{ getTotalActive() }}</p>
          </div>
          <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-5">
            <h3 class="text-xs font-semibold text-text-light dark:text-text-muted uppercase tracking-wider mb-1">Unassigned</h3>
            <p class="text-2xl font-bold text-warning-yellow">{{ dashboard()!.totalUnassigned }}</p>
          </div>
          <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-5">
            <h3 class="text-xs font-semibold text-text-light dark:text-text-muted uppercase tracking-wider mb-1">SLA Breached</h3>
            <p class="text-2xl font-bold text-error-red">{{ dashboard()!.totalOverdue }}</p>
          </div>
        </div>

        <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white mb-4">Group Breakdown</h3>

        <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">
          @for (group of dashboard()!.groupDashboards; track group.groupId) {
            <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 flex flex-col hover:shadow-md transition-shadow">
              <div class="flex justify-between items-start mb-4">
                <h4 class="text-lg font-bold text-text-dark dark:text-text-white">{{ group.groupName }}</h4>
                <div class="text-xs font-bold px-2 py-1 bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300 rounded">
                  {{ group.totalTickets }} Total
                </div>
              </div>

              <div class="grid grid-cols-3 gap-4">
                <div class="bg-bg-light dark:bg-bg-dark rounded-lg p-3 text-center border border-table-dark-gray dark:border-gray-700">
                  <span class="block text-xs font-medium text-text-light mb-1">Assigned</span>
                  <span class="block text-xl font-bold text-primary-blue">{{ group.totalActive }}</span>
                </div>
                <div class="bg-bg-light dark:bg-bg-dark rounded-lg p-3 text-center border border-table-dark-gray dark:border-gray-700">
                  <span class="block text-xs font-medium text-text-light mb-1">Unassigned</span>
                  <span class="block text-xl font-bold text-warning-yellow">{{ group.unassigned }}</span>
                </div>
                <div class="bg-bg-light dark:bg-bg-dark rounded-lg p-3 text-center border border-table-dark-gray dark:border-gray-700">
                  <span class="block text-xs font-medium text-text-light mb-1">Overdue</span>
                  <span class="block text-xl font-bold text-error-red">{{ group.slasBreached }}</span>
                </div>
              </div>

              <!-- Agent workloads -->
              @if (group.agentWorkloads.length > 0) {
                <div class="mt-4 border-t border-table-dark-gray dark:border-gray-700 pt-4">
                  <h5 class="text-xs font-semibold text-text-light uppercase tracking-wider mb-2">Agent Workload</h5>
                  <div class="space-y-1.5">
                    @for (agent of group.agentWorkloads; track agent.agentId) {
                      <div class="flex justify-between items-center text-sm">
                        <span class="text-text-main truncate max-w-[55%]">{{ agent.agentName }}</span>
                        <span class="text-xs text-text-muted whitespace-nowrap">
                          <span class="font-medium">{{ agent.openTickets }}</span> open &nbsp;·&nbsp;
                          <span [class]="agent.overdueTickets > 0 ? 'text-error-red font-semibold' : 'text-text-muted'">
                            {{ agent.overdueTickets }} overdue
                          </span>
                        </span>
                      </div>
                    }
                  </div>
                </div>
              }
            </div>
          }

          @if (dashboard()!.groupDashboards.length === 0) {
            <div class="col-span-full py-8 text-center text-text-light bg-surface border border-dashed border-gray-300 dark:border-gray-700 rounded-xl">
              You do not manage any groups yet.
            </div>
          }
        </div>
      }
    </div>
  `
})
export class DashboardComponent implements OnInit {
  private teamLeadService = inject(TeamLeadService);

  dashboard = signal<LeadDashboard | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit() {
    this.teamLeadService.getLeadDashboard().subscribe({
      next: (res) => {
        this.dashboard.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load dashboard. Please try again.');
        console.error('Error loading lead dashboard', err);
        this.loading.set(false);
      },
    });
  }

  getTotalActive() {
    return this.dashboard()?.groupDashboards.reduce((sum, g) => sum + g.totalActive, 0) ?? 0;
  }
}