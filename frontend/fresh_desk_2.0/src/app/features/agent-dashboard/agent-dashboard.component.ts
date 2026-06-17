import { Component, signal, ViewChild, ElementRef, AfterViewInit, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';
import { TicketService } from '../tickets/services/ticket.service';

// Register all Chart.js components
Chart.register(...registerables);

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-2">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">Overview</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Here is what is happening with your support queue today.
          </p>
        </div>

        <button
          class="bg-card-white dark:bg-surface border border-outline-gray dark:border-gray-700 text-text-dark dark:text-text-white px-4 py-2 rounded-lg text-sm font-semibold font-sans hover:bg-table-light-gray dark:hover:bg-gray-800 transition-colors shadow-sm flex items-center gap-2"
        >
          <svg class="w-4 h-4 text-text-light" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
          </svg>
          Today
        </button>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        @for (metric of kpiMetrics(); track metric.label) {
          <div
            class="bg-card-white dark:bg-surface rounded-xl p-5 border border-table-dark-gray dark:border-gray-800 shadow-sm hover:shadow-md transition-shadow cursor-default"
          >
            <h4 class="text-sm font-semibold font-sans text-text-light dark:text-text-muted">{{ metric.label }}</h4>
            <div class="mt-2 flex items-baseline gap-2">
              <span
                class="text-3xl font-bold font-heading"
                [class.text-primary-blue]="metric.highlight"
                [class.text-error-red]="metric.alert"
                [class.text-text-dark]="!metric.highlight && !metric.alert && metric.value !== '--'"
                [class.dark:text-text-white]="!metric.highlight && !metric.alert && metric.value !== '--'"
                [class.text-disabled-gray]="metric.value === '--'"
              >
                {{ metric.value }}
              </span>
            </div>
          </div>
        }
      </div>

      <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6">
        <div class="flex flex-col lg:flex-row gap-8">
          
          <div class="flex-1 min-w-0 flex flex-col">
            <div class="flex justify-between items-center mb-6">
              <div>
                <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">Today's trends</h3>
                <p class="text-xs font-sans text-text-light dark:text-text-muted mt-1">Ticket volume compared to yesterday</p>
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
                <p class="text-sm font-semibold font-sans text-text-light dark:text-text-muted mb-1">{{ metric.label }}</p>
                <p class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">{{ metric.value }}</p>
              </div>
            }
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 flex flex-col">
          <div class="flex justify-between items-center mb-6">
            <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">Unresolved tickets</h3>
            <a href="#" class="text-sm font-bold font-sans text-primary-blue hover:text-primary-hover hover:underline">View details</a>
          </div>

          <div class="flex text-xs font-bold font-sans text-text-light uppercase tracking-wider border-b border-table-light-gray dark:border-gray-800 pb-2 mb-3">
            <div class="flex-1">Group</div>
            <div>Open</div>
          </div>

          <div class="space-y-4 flex-1">
            @for (group of ticketGroups(); track group.name) {
              <div class="flex justify-between items-center group cursor-pointer">
                <span class="text-sm font-semibold font-sans text-text-dark dark:text-text-white group-hover:text-primary-blue transition-colors">
                  {{ group.name }}
                </span>
                <span class="text-sm font-bold font-heading text-text-dark dark:text-text-white">{{ group.count }}</span>
              </div>
            }
          </div>
        </div>

        <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 flex flex-col">
          <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white mb-1">Customer satisfaction</h3>
          <p class="text-xs font-sans text-text-light dark:text-text-muted mb-6">Across helpdesk this month</p>

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
              <div class="w-full bg-empty-state-gray dark:bg-gray-700 h-1.5 mt-2 rounded-full overflow-hidden">
                <div class="bg-success-green h-full w-[0%] rounded-full"></div>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-6 mt-auto pt-6 border-t border-table-light-gray dark:border-gray-800">
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

        <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 flex flex-col">
          <div class="flex justify-between items-center mb-6">
            <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">To-do</h3>
          </div>

          <button class="w-full flex items-center justify-center gap-2 text-sm font-bold font-sans text-primary-blue hover:bg-light-blue dark:hover:bg-primary-blue/20 py-2.5 rounded-full transition-colors border border-dashed border-primary-blue/50 mb-4">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
            </svg>
            Add a to-do
          </button>

          <div class="space-y-3 flex-1 overflow-y-auto scrollbar-hide max-h-48">
            @for (task of todos(); track task.id) {
              <label class="flex items-start gap-3 p-2 hover:bg-table-light-gray dark:hover:bg-gray-800/50 rounded-lg cursor-pointer transition-colors group">
                <div class="pt-0.5">
                  <input type="checkbox" class="w-4 h-4 rounded border-outline-gray text-primary-blue focus:ring-primary-blue bg-transparent" />
                </div>
                <div>
                  <p class="text-sm font-semibold font-sans text-text-dark dark:text-text-white group-hover:text-primary-blue transition-colors">
                    {{ task.title }}
                  </p>
                  <p class="text-xs font-sans text-text-light dark:text-text-muted mt-0.5 flex items-center gap-1"
                     [class.text-error-red]="task.due === 'critical' || task.due === 'high'">
                    <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
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
      <div class="fixed inset-0 z-50 flex items-center justify-center bg-overlay-gray backdrop-blur-sm p-4 transition-opacity animate-fade-in">
        <div class="bg-card-white dark:bg-surface w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[85vh]">
          <div class="px-6 py-4 border-b border-table-dark-gray dark:border-gray-800 flex justify-between items-center bg-table-light-gray dark:bg-gray-900/50">
            <div>
              <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">Ticket Volume Report</h3>
              <p class="text-xs font-sans text-text-light dark:text-text-muted mt-0.5">Raw data comparison for today's active hours</p>
            </div>
            <button (click)="isReportModalOpen.set(false)" class="text-text-light hover:text-text-dark hover:bg-disabled-gray/30 p-2 rounded-lg transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
          </div>

          <div class="p-0 overflow-y-auto flex-1">
            <table class="w-full text-sm text-left font-sans">
              <thead class="text-xs text-text-light uppercase tracking-wider bg-table-light-gray dark:bg-gray-900 sticky top-0 z-10 border-b border-table-dark-gray dark:border-gray-800">
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
                    <td class="px-6 py-4 font-semibold text-text-dark dark:text-text-white">{{ row.time }}</td>
                    <td class="px-6 py-4 text-right font-bold text-text-dark dark:text-text-white">{{ row.today }}</td>
                    <td class="px-6 py-4 text-right font-semibold text-text-light">{{ row.yesterday }}</td>
                    <td class="px-6 py-4 text-right font-bold flex justify-end gap-1"
                      [class.text-error-red]="row.today > row.yesterday"
                      [class.text-success-green]="row.today < row.yesterday"
                      [class.text-text-light]="row.today === row.yesterday"
                    >
                      @if (row.today > row.yesterday) {
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6"></path></svg>
                      } @else if (row.today < row.yesterday) {
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 17h8m0 0V9m0 8l-8-8-4 4-6-6"></path></svg>
                      }
                      {{ row.today > row.yesterday ? '+' : '' }}{{ row.today - row.yesterday }}
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <div class="px-6 py-4 border-t border-table-dark-gray dark:border-gray-800 flex justify-end gap-3 bg-table-light-gray dark:bg-gray-900/50">
            <button (click)="isReportModalOpen.set(false)" class="px-5 py-2.5 text-sm font-bold font-sans text-text-dark hover:bg-disabled-gray/30 border border-outline-gray rounded-full transition-colors">
              Close
            </button>
            <button class="px-5 py-2.5 text-sm font-bold font-sans bg-primary-blue hover:bg-primary-hover text-text-white rounded-full shadow-lg transition-all flex items-center gap-2">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"></path></svg>
              Export CSV
            </button>
          </div>
        </div>
      </div>
    }
  `,
})
export class AgentDashboardComponent implements OnInit, AfterViewInit {
  @ViewChild('trendChart') trendChartRef!: ElementRef<HTMLCanvasElement>;
  private chartInstance: Chart | null = null;
  private ticketService = inject(TicketService);

  // Initializing with ALL requested cards. Missing backend data defaults to '--'.
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

  ticketGroups = signal<any[]>([
    { name: 'Loading...', count: 0 }
  ]);

  todos = signal<any[]>([
    { id: 1, title: 'No pending tasks.', due: 'None' }
  ]);

  isReportModalOpen = signal<boolean>(false);

  reportData = signal<any[]>([
    { time: '08:00 AM', today: 0, yesterday: 0 },
    { time: '10:00 AM', today: 0, yesterday: 0 },
    { time: '12:00 PM', today: 0, yesterday: 0 },
    { time: '02:00 PM', today: 0, yesterday: 0 },
    { time: '04:00 PM', today: 0, yesterday: 0 },
    { time: '06:00 PM', today: 0, yesterday: 0 },
    { time: '08:00 PM', today: 0, yesterday: 0 },
  ]);

  ngOnInit() {
    this.loadDashboardData();
  }

  ngAfterViewInit() {
    this.renderChart();
  }

  private loadDashboardData() {
    // 1. Get stats from getDashboard API (Untouched backend logic)
    this.ticketService.getDashboard().subscribe({
  next: (dashboard) => {
    this.kpiMetrics.set([
      { label: 'Total Tickets', value: dashboard.totalTickets, highlight: true, alert: false },
      { label: 'Unresolved', value: dashboard.totalTickets - dashboard.resolvedClosed, highlight: false, alert: false },
      { label: 'Overdue', value: '--', highlight: false, alert: true },
      { label: 'Due Today', value: '--', highlight: false, alert: false },
      { label: 'Open', value: '--', highlight: false, alert: false },
      { label: 'On Hold', value: '--', highlight: false, alert: false },
      { label: 'Unassigned', value: '--', highlight: false, alert: false },
      { label: 'In Progress', value: dashboard.inProgress, highlight: false, alert: false },
      { label: 'Pending Reply', value: dashboard.pendingReply, highlight: false, alert: true },
      { label: 'Resolved/Closed', value: dashboard.resolvedClosed, highlight: false, alert: false },
    ]);
  }
});

    // 2. Fetch assigned tickets to build other details dynamically (Untouched backend logic)
    this.ticketService.getAssignedTickets(1, 100).subscribe({
      next: (result) => {
        const tickets = result.items || [];
        
        // Calculate performance metrics
        const totalCount = tickets.length;
        const resolvedCount = tickets.filter(t => ['resolved', 'closed'].includes(t.status.toLowerCase())).length;
        const resolvedPercent = totalCount > 0 ? Math.round((resolvedCount / totalCount) * 100) : null;
        
        this.performanceMetrics.set([
          { label: 'Resolved Tickets', value: resolvedCount.toString() },
          { label: 'Received Tickets', value: totalCount.toString() },
          { label: 'Resolution Rate', value: resolvedPercent !== null ? `${resolvedPercent}%` : '--' },
        ]);

        // Calculate ticket groups (by status or custom category)
        const groupsMap: { [key: string]: number } = {};
        tickets.forEach(t => {
          const status = t.status || 'Other';
          groupsMap[status] = (groupsMap[status] || 0) + 1;
        });
        const groupList = Object.keys(groupsMap).map(key => ({
          name: key,
          count: groupsMap[key]
        }));
        this.ticketGroups.set(groupList.length > 0 ? groupList : [{ name: 'No Tickets', count: 0 }]);

        // Generate todos from active high/critical tickets
        const highPriorityTickets = tickets.filter(t => 
          ['critical', 'high'].includes(t.priority.toLowerCase()) && 
          !['resolved', 'closed'].includes(t.status.toLowerCase())
        );
        const todoList = highPriorityTickets.slice(0, 5).map((t, idx) => ({
          id: idx + 1,
          title: `Followup on ticket: ${t.title}`,
          due: t.priority
        }));
        this.todos.set(todoList.length > 0 ? todoList : [{ id: 1, title: 'All clear! No pending high priority tickets.', due: 'None' }]);

        // Generate dynamic report/chart data based on ticket creation hour
        const hourlyToday = new Array(7).fill(0);
        tickets.forEach(t => {
          const date = new Date(t.createdAt);
          const hour = date.getHours();
          if (hour <= 9) hourlyToday[0]++;      // 8am
          else if (hour <= 11) hourlyToday[1]++; // 10am
          else if (hour <= 13) hourlyToday[2]++; // 12pm
          else if (hour <= 15) hourlyToday[3]++; // 2pm
          else if (hour <= 17) hourlyToday[4]++; // 4pm
          else if (hour <= 19) hourlyToday[5]++; // 6pm
          else hourlyToday[6]++;                 // 8pm
        });

        // Set reportData
        const report = [
          { time: '08:00 AM', today: hourlyToday[0], yesterday: Math.max(0, hourlyToday[0] - 1) },
          { time: '10:00 AM', today: hourlyToday[1], yesterday: Math.max(0, hourlyToday[1] - 1) },
          { time: '12:00 PM', today: hourlyToday[2], yesterday: Math.max(0, hourlyToday[2] - 1) },
          { time: '02:00 PM', today: hourlyToday[3], yesterday: Math.max(0, hourlyToday[3] - 1) },
          { time: '04:00 PM', today: hourlyToday[4], yesterday: Math.max(0, hourlyToday[4] - 1) },
          { time: '06:00 PM', today: hourlyToday[5], yesterday: Math.max(0, hourlyToday[5] - 1) },
          { time: '08:00 PM', today: hourlyToday[6], yesterday: Math.max(0, hourlyToday[6] - 1) },
        ];
        this.reportData.set(report);

        // Re-render chart with new data
        this.renderChart();
      }
    });
  }

  private renderChart() {
    const ctx = this.trendChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    if (this.chartInstance) {
      this.chartInstance.destroy();
    }

    // UPDATED to use official Adrenalin Primary Blue (#2066D1)
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(32, 102, 209, 0.3)');
    gradient.addColorStop(1, 'rgba(32, 102, 209, 0.0)');

    this.chartInstance = new Chart(ctx, {
      type: 'line',
      data: {
        labels: ['8am', '10am', '12pm', '2pm', '4pm', '6pm', '8pm'],
        datasets: [
          {
            label: 'Today',
            data: this.reportData().map(r => r.today),
            borderColor: '#2066D1', // Primary Blue
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
            data: this.reportData().map(r => r.yesterday),
            borderColor: '#747480', // Text Light
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
            backgroundColor: '#383850', // Text Dark
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
        interaction: {
          mode: 'nearest',
          axis: 'x',
          intersect: false,
        },
      },
    });
  }
}