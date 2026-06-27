import {
  Component,
  signal,
  ViewChild,
  ElementRef,
  AfterViewInit,
  OnInit,
  OnDestroy,
  inject,
} from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { Chart, registerables } from 'chart.js';
import { Router } from '@angular/router';
import { Subscription, forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

// Services
import { DashboardService } from './services/dashboard.service';
// Assuming you have this service somewhere, adjust path if needed:
import { NotificationService } from './services/notification.service';

// PBAC
import { HasPermissionDirective } from '../../core/directives/has-permission.directive';
import { PERMISSIONS } from '../../core/auth/permission.constants';

Chart.register(...registerables);

@Component({
  selector: 'app-workspace-dashboard', // Renamed selector!
  standalone: true,
  imports: [CommonModule, TitleCasePipe, HasPermissionDirective],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-2">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">
            Workspace Overview
          </h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Here is what is happening across the desk today.
          </p>
        </div>

        <button
          class="bg-card-white dark:bg-surface border border-outline-gray dark:border-gray-700 text-text-dark dark:text-text-white px-4 py-2 rounded-lg text-sm font-semibold font-sans hover:bg-table-light-gray dark:hover:bg-gray-800 transition-colors shadow-sm flex items-center gap-2"
        >
          <svg
            class="w-4 h-4 text-text-light"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
            ></path>
          </svg>
          Today
        </button>
      </div>

      <div
        *appHasPermission="PERMISSIONS.DASHBOARD.VIEW_ADMIN"
        class="bg-primary-blue/5 border border-primary-blue/20 rounded-xl p-5 shadow-sm"
      >
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-sm font-bold font-heading text-primary-blue">Global Admin Analytics</h3>
          <span
            class="text-[10px] font-mono bg-primary-blue text-white px-2 py-0.5 rounded uppercase tracking-wider"
            >Full Access</span
          >
        </div>
        <div class="flex gap-4">
          <select
            class="px-4 py-2 bg-card-white dark:bg-surface border border-outline-gray dark:border-gray-700 rounded-lg text-sm font-bold text-text-dark dark:text-text-white shadow-sm focus:ring-2 focus:ring-primary-blue outline-none"
          >
            <option>All Regions</option>
            <option>EMEA</option>
            <option>APAC</option>
          </select>
          <select
            class="px-4 py-2 bg-card-white dark:bg-surface border border-outline-gray dark:border-gray-700 rounded-lg text-sm font-bold text-text-dark dark:text-text-white shadow-sm focus:ring-2 focus:ring-primary-blue outline-none"
          >
            <option>All Queues (Global)</option>
            <option>Network Operations</option>
          </select>
          <button
            class="px-4 py-2 bg-primary-blue text-white rounded-lg text-sm font-bold shadow-sm hover:bg-primary-hover"
          >
            Generate Global Report
          </button>
        </div>
      </div>

      <div
        *appHasPermission="PERMISSIONS.DASHBOARD.VIEW_TEAM_LEAD"
        class="bg-light-yellow/30 border border-yellow/40 rounded-xl p-5 shadow-sm"
      >
        <div class="flex items-center justify-between mb-2">
          <h3 class="text-sm font-bold font-heading text-revision-amber">Team Queue Overview</h3>
          <span
            class="text-[10px] font-mono bg-revision-amber text-white px-2 py-0.5 rounded uppercase tracking-wider"
            >Manager View</span
          >
        </div>
        <p class="text-xs text-text-dark dark:text-text-muted mb-4">
          You are viewing metrics for your assigned queues.
        </p>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div
            class="bg-card-white dark:bg-surface p-4 rounded-lg border border-outline-gray dark:border-gray-700"
          >
            <div class="text-xs text-text-muted">Unassigned in Queue</div>
            <div class="text-2xl font-bold font-mono mt-1 text-error-red">
              {{ kpiMetrics()[6].value }}
            </div>
          </div>
          <div
            class="bg-card-white dark:bg-surface p-4 rounded-lg border border-outline-gray dark:border-gray-700"
          >
            <div class="text-xs text-text-muted">SLA Breaches (Today)</div>
            <div class="text-2xl font-bold font-mono mt-1 text-revision-amber">2</div>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        @for (metric of kpiMetrics(); track metric.label) {
          <div
            class="bg-card-white dark:bg-surface rounded-xl p-5 border border-table-dark-gray dark:border-gray-800 shadow-sm hover:shadow-md transition-shadow cursor-default"
          >
            <h4 class="text-sm font-semibold font-sans text-text-light dark:text-text-muted">
              {{ metric.label }}
            </h4>
            <div class="mt-2 flex items-baseline gap-2">
              <span
                class="text-3xl font-bold font-heading"
                [class.text-primary-blue]="metric.highlight"
                [class.text-error-red]="metric.alert"
                [class.text-text-dark]="!metric.highlight && !metric.alert && metric.value !== '--'"
                [class.dark:text-text-white]="
                  !metric.highlight && !metric.alert && metric.value !== '--'
                "
                [class.text-disabled-gray]="metric.value === '--'"
              >
                {{ metric.value }}
              </span>
            </div>
          </div>
        }
      </div>

      <div
        class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6"
      >
        <div class="flex flex-col lg:flex-row gap-8">
          <div class="flex-1 min-w-0 flex flex-col">
            <div class="flex justify-between items-center mb-6">
              <div>
                <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">
                  Today's trends
                </h3>
                <p class="text-xs font-sans text-text-light dark:text-text-muted mt-1">
                  Ticket volume compared to yesterday
                </p>
              </div>
              <button
                (click)="isReportModalOpen.set(true)"
                class="text-sm font-bold font-sans text-primary-blue hover:text-primary-hover hover:underline"
              >
                View report
              </button>
            </div>
            <div class="h-80 w-full relative">
              <canvas #trendChart></canvas>
            </div>
          </div>

          <div
            class="w-full lg:w-64 flex-shrink-0 flex flex-col justify-between gap-6 lg:border-l border-table-dark-gray dark:border-gray-800 lg:pl-8 py-2"
          >
            @for (metric of performanceMetrics(); track metric.label) {
              <div>
                <p
                  class="text-sm font-semibold font-sans text-text-light dark:text-text-muted mb-1"
                >
                  {{ metric.label }}
                </p>
                <p class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">
                  {{ metric.value }}
                </p>
              </div>
            }
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div
          class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 flex flex-col"
        >
          <div class="flex justify-between items-center mb-6">
            <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">
              Unresolved tickets
            </h3>
            <a
              href="#"
              class="text-sm font-bold font-sans text-primary-blue hover:text-primary-hover hover:underline"
              >View details</a
            >
          </div>

          <div
            class="flex text-xs font-bold font-sans text-text-light uppercase tracking-wider border-b border-table-light-gray dark:border-gray-800 pb-2 mb-3"
          >
            <div class="flex-1">Group</div>
            <div>Open</div>
          </div>

          <div class="space-y-4 flex-1">
            @for (group of ticketGroups(); track group.name) {
              <div class="flex justify-between items-center group cursor-pointer">
                <span
                  class="text-sm font-semibold font-sans text-text-dark dark:text-text-white group-hover:text-primary-blue transition-colors"
                >
                  {{ group.name }}
                </span>
                <span class="text-sm font-bold font-heading text-text-dark dark:text-text-white">{{
                  group.count
                }}</span>
              </div>
            }
          </div>
        </div>

        <div
          class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 flex flex-col"
        >
          <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white mb-1">
            Customer satisfaction
          </h3>
          <p class="text-xs font-sans text-text-light dark:text-text-muted mb-6">
            Across helpdesk this month
          </p>

          <div class="grid grid-cols-2 gap-6 mb-6">
            <div>
              <p class="text-sm font-semibold font-sans text-text-light mb-1">Responses</p>
              <p class="text-3xl font-bold font-heading text-text-dark dark:text-text-white">--</p>
            </div>
            <div>
              <p class="text-sm font-semibold font-sans text-text-light mb-1">Positive</p>
              <p class="text-3xl font-bold font-heading text-success-green flex items-center gap-2">
                --
              </p>
              <div
                class="w-full bg-disabled-gray dark:bg-gray-700 h-1.5 mt-2 rounded-full overflow-hidden"
              >
                <div class="bg-success-green h-full w-[0%] rounded-full"></div>
              </div>
            </div>
          </div>

          <div
            class="grid grid-cols-2 gap-6 mt-auto pt-6 border-t border-table-light-gray dark:border-gray-800"
          >
            <div>
              <p class="text-sm font-semibold font-sans text-text-light mb-1">Neutral</p>
              <p class="text-xl font-bold font-heading text-revision-amber">--</p>
            </div>
            <div>
              <p class="text-sm font-semibold font-sans text-text-light mb-1">Negative</p>
              <p class="text-xl font-bold font-heading text-error-red">--</p>
            </div>
          </div>
        </div>

        <div
          class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 flex flex-col"
        >
          <div class="flex justify-between items-center mb-6">
            <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">
              To-do
            </h3>
          </div>

          <button
            class="w-full flex items-center justify-center gap-2 text-sm font-bold font-sans text-primary-blue hover:bg-table-light-gray dark:hover:bg-primary-blue/20 py-2.5 rounded-full transition-colors border border-dashed border-primary-blue/50 mb-4"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 4v16m8-8H4"
              ></path>
            </svg>
            Add a to-do
          </button>

          <div class="space-y-3 flex-1 overflow-y-auto max-h-48">
            @for (task of todos(); track task.id) {
              <label
                class="flex items-start gap-3 p-2 hover:bg-table-light-gray dark:hover:bg-gray-800/50 rounded-lg cursor-pointer transition-colors group"
              >
                <div class="pt-0.5">
                  <input
                    type="checkbox"
                    class="w-4 h-4 rounded border-outline-gray text-primary-blue focus:ring-primary-blue bg-transparent"
                  />
                </div>
                <div>
                  <p
                    class="text-sm font-semibold font-sans text-text-dark dark:text-text-white group-hover:text-primary-blue transition-colors"
                  >
                    {{ task.title }}
                  </p>
                  <p
                    class="text-xs font-sans text-text-light dark:text-text-muted mt-0.5 flex items-center gap-1"
                    [class.text-error-red]="task.due === 'critical' || task.due === 'high'"
                  >
                    <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                      ></path>
                    </svg>
                    {{ task.due | titlecase }}
                  </p>
                </div>
              </label>
            }
          </div>
        </div>
      </div>
    </div>

    @if (isReportModalOpen()) {
      <div
        class="fixed inset-0 z-50 flex items-center justify-center bg-text-dark/50 backdrop-blur-sm p-4 transition-opacity animate-fade-in"
      >
        <div
          class="bg-card-white dark:bg-surface w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[85vh]"
        >
          <div
            class="px-6 py-4 border-b border-table-dark-gray dark:border-gray-800 flex justify-between items-center bg-table-light-gray dark:bg-gray-900/50"
          >
            <div>
              <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">
                Ticket Volume Report
              </h3>
              <p class="text-xs font-sans text-text-light dark:text-text-muted mt-0.5">
                Raw data comparison for today's active hours
              </p>
            </div>
            <button
              (click)="isReportModalOpen.set(false)"
              class="text-text-light hover:text-text-dark hover:bg-disabled-gray/30 p-2 rounded-lg transition-colors"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                ></path>
              </svg>
            </button>
          </div>

          <div class="p-0 overflow-y-auto flex-1">
            <table class="w-full text-sm text-left font-sans">
              <thead
                class="text-xs text-text-light uppercase tracking-wider bg-table-light-gray dark:bg-gray-900 sticky top-0 z-10 border-b border-table-dark-gray dark:border-gray-800"
              >
                <tr>
                  <th class="px-6 py-4 font-bold">Time Interval</th>
                  <th class="px-6 py-4 font-bold text-right">Today</th>
                  <th class="px-6 py-4 font-bold text-right">Yesterday</th>
                  <th class="px-6 py-4 font-bold text-right">Trend</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-table-dark-gray dark:divide-gray-800">
                @for (row of reportData(); track row.time) {
                  <tr class="hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors">
                    <td class="px-6 py-4 font-semibold text-text-dark dark:text-text-white">
                      {{ row.time }}
                    </td>
                    <td class="px-6 py-4 text-right font-bold text-text-dark dark:text-text-white">
                      {{ row.today }}
                    </td>
                    <td class="px-6 py-4 text-right font-semibold text-text-light">
                      {{ row.yesterday }}
                    </td>
                    <td
                      class="px-6 py-4 text-right font-bold flex justify-end gap-1"
                      [class.text-error-red]="row.today > row.yesterday"
                      [class.text-success-green]="row.today < row.yesterday"
                      [class.text-text-light]="row.today === row.yesterday"
                    >
                      @if (row.today > row.yesterday) {
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="2"
                            d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6"
                          ></path>
                        </svg>
                      } @else if (row.today < row.yesterday) {
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="2"
                            d="M13 17h8m0 0V9m0 8l-8-8-4 4-6-6"
                          ></path>
                        </svg>
                      }
                      {{ row.today > row.yesterday ? '+' : '' }}{{ row.today - row.yesterday }}
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <div
            class="px-6 py-4 border-t border-table-dark-gray dark:border-gray-800 flex justify-end gap-3 bg-table-light-gray dark:bg-gray-900/50"
          >
            <button
              (click)="isReportModalOpen.set(false)"
              class="px-5 py-2.5 text-sm font-bold font-sans text-text-dark hover:bg-disabled-gray/30 border border-outline-gray rounded-full transition-colors"
            >
              Close
            </button>
            <button
              class="px-5 py-2.5 text-sm font-bold font-sans bg-primary-blue hover:bg-primary-hover text-text-white rounded-full shadow-lg transition-all flex items-center gap-2"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
                ></path>
              </svg>
              Export CSV
            </button>
          </div>
        </div>
      </div>
    }

    @if (toastMessage()) {
      <div
        (click)="navigateToTicket()"
        class="fixed bottom-6 right-6 bg-text-dark hover:bg-gray-800 text-white text-sm font-semibold px-5 py-3 rounded-xl shadow-2xl animate-fade-in z-50 flex items-center gap-3 cursor-pointer transition-all duration-200 select-none group"
      >
        <div class="w-2.5 h-2.5 rounded-full bg-error-red"></div>
        {{ toastMessage() }}
      </div>
    }
  `,
})
export class WorkspaceDashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  PERMISSIONS = PERMISSIONS; // Required for HTML directives

  @ViewChild('trendChart') trendChartRef!: ElementRef<HTMLCanvasElement>;
  private chartInstance: Chart | null = null;

  // Using the new DashboardService!
  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService, { optional: true });
  private router = inject(Router);

  toastMessage = signal<string | null>(null);
  activeToastTicketId = signal<string | null>(null);

  kpiMetrics = signal<any[]>([
    { label: 'Total Tickets', value: 0, highlight: true, alert: false },
    { label: 'Unresolved', value: 0, highlight: false, alert: false },
    { label: 'Overdue', value: '--', highlight: false, alert: true },
    { label: 'Due Today', value: '--', highlight: false, alert: false },
    { label: 'Open', value: '--', highlight: false, alert: false },
    { label: 'On Hold', value: '--', highlight: false, alert: false },
    { label: 'Unassigned', value: '--', highlight: false, alert: false },
    { label: 'In Progress', value: 0, highlight: false, alert: false },
    { label: 'Pending Reply', value: 0, highlight: false, alert: true },
    { label: 'Resolved/Closed', value: 0, highlight: false, alert: false },
  ]);

  performanceMetrics = signal<any[]>([
    { label: 'Resolved Tickets', value: '--' },
    { label: 'Received Tickets', value: '--' },
    { label: 'Resolution Rate', value: '--' },
  ]);

  ticketGroups = signal<any[]>([{ name: 'Loading...', count: 0 }]);
  todos = signal<any[]>([{ id: '1', title: 'No pending tasks.', due: 'None' }]);
  isReportModalOpen = signal<boolean>(false);
  reportData = signal<any[]>([]);

  private dashboardSub = new Subscription();

  ngOnInit() {
    this.loadDashboardData();
  }

  ngAfterViewInit() {
    this.renderChart();
  }

  ngOnDestroy() {
    this.dashboardSub.unsubscribe();
    if (this.chartInstance) {
      this.chartInstance.destroy();
    }
  }

  loadDashboardData() {
    this.dashboardSub.unsubscribe();
    this.dashboardSub = new Subscription();

    // Safely handle NotificationService if it's missing/optional
    const notifications$ = this.notificationService
      ? this.notificationService.getUnreadNotifications()
      : of([]);

    this.dashboardSub.add(
      forkJoin({
        dashboard: this.dashboardService.getDashboard(),
        notifications: notifications$.pipe(catchError(() => of([]))),
      }).subscribe({
        next: ({ dashboard, notifications }) => {
          const activeLogs = notifications || [];

          // Notifications Logic
          const latestInAppLog = activeLogs.find((log: any) => log.recipientEmail || '');
          if (latestInAppLog) {
            this.activeToastTicketId.set(latestInAppLog.ticketId);
            const isSlaBreach = ['teamlead', 'manager', 'agent'].some((role) =>
              (latestInAppLog.recipientEmail || '').includes(role),
            );
            const alertTitle = isSlaBreach ? '⚠️ SLA Violation Alert' : '🎫 Helpdesk Update';
            const alertBody = isSlaBreach
              ? `Active SLA breach tracked on Ticket #${latestInAppLog.ticketNumber || 'Unknown'}.`
              : `New log activity recorded for recipient: ${latestInAppLog.recipientEmail}`;

            this.toastMessage.set(`${alertTitle}: ${alertBody}`);
            setTimeout(() => {
              this.toastMessage.set(null);
              this.activeToastTicketId.set(null);
            }, 7000);
          }

          // KPI Metrics Mapping
          this.kpiMetrics.set([
            {
              label: 'Total Tickets',
              value: dashboard.totalTickets,
              highlight: true,
              alert: false,
            },
            {
              label: 'Unresolved',
              value: dashboard.totalActive ?? 0,
              highlight: true,
              alert: false,
            },
            {
              label: 'Overdue',
              value: activeLogs.filter((log: any) =>
                (log.recipientEmail || '').includes('inapp_user_'),
              ).length,
              highlight: false,
              alert: true,
            },
            { label: 'Due Today', value: 0, highlight: false, alert: false },
            { label: 'Open', value: dashboard.totalActive ?? 0, highlight: false, alert: false },
            { label: 'On Hold', value: 0, highlight: false, alert: false },
            {
              label: 'Unassigned',
              value: dashboard.counts?.unassigned ?? 0,
              highlight: false,
              alert: false,
            },
            {
              label: 'In Progress',
              value: dashboard.inProgress ?? 0,
              highlight: false,
              alert: false,
            },
            {
              label: 'Pending Reply',
              value: dashboard.pendingReply ?? 0,
              highlight: false,
              alert: true,
            },
            {
              label: 'Resolved/Closed',
              value: dashboard.resolvedClosed ?? 0,
              highlight: false,
              alert: false,
            },
          ]);

          // Performance Metrics Mapping
          if (dashboard.performance) {
            this.performanceMetrics.set([
              {
                label: 'Resolved Tickets',
                value: dashboard.performance.resolvedToday?.toString() || '0',
              },
              {
                label: 'Received Tickets',
                value: dashboard.performance.receivedToday?.toString() || '0',
              },
              {
                label: 'Resolution Rate',
                value:
                  dashboard.performance.resolutionRate !== null
                    ? `${dashboard.performance.resolutionRate}%`
                    : '--',
              },
            ]);
          }

          // Lists Mapping
          this.ticketGroups.set(
            dashboard.groupMetrics?.length
              ? dashboard.groupMetrics
              : [{ name: 'No Tickets', count: 0 }],
          );
          this.todos.set(
            dashboard.todos?.length
              ? dashboard.todos
              : [{ id: '1', title: 'All clear! No pending high priority tickets.', due: 'None' }],
          );

          if (dashboard.trends?.length) {
            this.reportData.set(
              dashboard.trends.map((t: any) => ({
                time: t.timeLabel,
                today: t.todayCount,
                yesterday: t.yesterdayCount,
              })),
            );
          }

          this.renderChart();
        },
        error: (err) => console.error('Failed to resolve dashboard metrics', err),
      }),
    );
  }

  navigateToTicket(): void {
    const ticketId = this.activeToastTicketId();
    if (ticketId) {
      this.toastMessage.set(null);
      this.activeToastTicketId.set(null);
      // FIXED ROUTING URL!
      this.router.navigate(['/workspace/tickets', ticketId]);
    }
  }

  private renderChart() {
    const ctx = this.trendChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    if (this.chartInstance) {
      this.chartInstance.destroy();
    }

    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(32, 102, 209, 0.3)');
    gradient.addColorStop(1, 'rgba(32, 102, 209, 0.0)');

    this.chartInstance = new Chart(ctx, {
      type: 'line',
      data: {
        labels: this.reportData().map((r) => r.time) || [
          '8am',
          '10am',
          '12pm',
          '2pm',
          '4pm',
          '6pm',
          '8pm',
        ],
        datasets: [
          {
            label: 'Today',
            data: this.reportData().map((r) => r.today),
            borderColor: '#2066D1',
            backgroundColor: gradient,
            borderWidth: 3,
            tension: 0.4,
            fill: true,
            pointBackgroundColor: '#ffffff',
            pointBorderColor: '#2066D1',
            pointBorderWidth: 2,
            pointRadius: 4,
            pointHoverRadius: 6,
          },
          {
            label: 'Yesterday',
            data: this.reportData().map((r) => r.yesterday),
            borderColor: '#747480',
            borderWidth: 2,
            borderDash: [5, 5],
            tension: 0.4,
            fill: false,
            pointRadius: 0,
            pointHoverRadius: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom',
            labels: {
              usePointStyle: true,
              boxWidth: 8,
              font: { family: "'Open Sans', sans-serif" },
            },
          },
          tooltip: {
            mode: 'index',
            intersect: false,
            backgroundColor: '#383850',
            titleFont: { family: "'Montserrat', sans-serif" },
            bodyFont: { family: "'Open Sans', sans-serif" },
            padding: 12,
            cornerRadius: 8,
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(116, 116, 128, 0.1)' },
            border: { display: false },
            ticks: { font: { family: "'Open Sans', sans-serif" } },
          },
          x: {
            grid: { display: false },
            border: { display: false },
            ticks: { font: { family: "'Open Sans', sans-serif" } },
          },
        },
        interaction: { mode: 'nearest', axis: 'x', intersect: false },
      },
    });
  }
}
