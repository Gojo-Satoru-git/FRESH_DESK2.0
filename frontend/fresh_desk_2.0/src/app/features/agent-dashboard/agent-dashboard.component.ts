import { Component, signal, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';

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
          <h2 class="text-2xl font-bold text-text-main">Overview</h2>
          <p class="text-sm text-text-muted mt-1">
            Here is what is happening with your support queue today.
          </p>
        </div>

        <button
          class="bg-surface border border-gray-300 dark:border-gray-700 text-text-main px-4 py-2 rounded-lg text-sm font-medium hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors shadow-sm flex items-center gap-2"
        >
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
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
            ></path>
          </svg>
          Today
        </button>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        @for (metric of kpiMetrics(); track metric.label) {
          <div
            class="bg-surface rounded-xl p-5 border border-gray-200 dark:border-gray-800 shadow-sm hover:shadow-md transition-shadow cursor-default"
          >
            <h4 class="text-sm font-medium text-text-muted">{{ metric.label }}</h4>
            <div class="mt-2 flex items-baseline gap-2">
              <span
                class="text-3xl font-bold"
                [class.text-primary]="metric.highlight"
                [class.text-red-500]="metric.alert"
                [class.text-text-main]="!metric.highlight && !metric.alert"
              >
                {{ metric.value }}
              </span>
            </div>
          </div>
        }
      </div>

      <div class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-6">
        <div class="flex flex-col lg:flex-row gap-8">
          <div class="flex-1 min-w-0 flex flex-col">
            <div class="flex justify-between items-center mb-6">
              <div>
                <h3 class="text-lg font-bold text-text-main">Today's trends</h3>
                <p class="text-xs text-text-muted mt-1">Ticket volume compared to yesterday</p>
              </div>
              <button
                (click)="isReportModalOpen.set(true)"
                class="text-sm font-medium text-primary hover:underline"
              >
                View report
              </button>
            </div>

            <div class="h-80 w-full relative">
              <canvas #trendChart></canvas>
            </div>
          </div>

          <div
            class="w-full lg:w-64 flex-shrink-0 flex flex-col justify-between gap-6 lg:border-l border-gray-200 dark:border-gray-800 lg:pl-8 py-2"
          >
            @for (metric of performanceMetrics(); track metric.label) {
              <div>
                <p class="text-sm text-text-muted mb-1">{{ metric.label }}</p>
                <p class="text-2xl font-bold text-text-main">{{ metric.value }}</p>
              </div>
            }
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div
          class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-6 flex flex-col"
        >
          <div class="flex justify-between items-center mb-6">
            <h3 class="text-lg font-bold text-text-main">Unresolved tickets</h3>
            <a href="#" class="text-sm font-medium text-primary hover:underline">View details</a>
          </div>

          <div
            class="flex text-xs font-semibold text-text-muted border-b border-gray-100 dark:border-gray-800 pb-2 mb-3"
          >
            <div class="flex-1">Group</div>
            <div>Open</div>
          </div>

          <div class="space-y-4 flex-1">
            @for (group of ticketGroups(); track group.name) {
              <div class="flex justify-between items-center group cursor-pointer">
                <span
                  class="text-sm font-medium text-text-main group-hover:text-primary transition-colors"
                  >{{ group.name }}</span
                >
                <span class="text-sm font-bold text-text-main">{{ group.count }}</span>
              </div>
            }
          </div>
        </div>

        <div
          class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-6 flex flex-col"
        >
          <h3 class="text-lg font-bold text-text-main mb-1">Customer satisfaction</h3>
          <p class="text-xs text-text-muted mb-6">Across helpdesk this month</p>

          <div class="grid grid-cols-2 gap-6 mb-6">
            <div>
              <p class="text-sm text-text-muted mb-1">Responses</p>
              <p class="text-3xl font-bold text-text-main">320</p>
            </div>
            <div>
              <p class="text-sm text-text-muted mb-1">Positive</p>
              <p class="text-3xl font-bold text-green-500 flex items-center gap-2">
                90%
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  ></path>
                </svg>
              </p>
              <div
                class="w-full bg-gray-200 dark:bg-gray-700 h-1 mt-2 rounded-full overflow-hidden"
              >
                <div class="bg-green-500 h-full w-[90%] rounded-full"></div>
              </div>
            </div>
          </div>

          <div
            class="grid grid-cols-2 gap-6 mt-auto pt-6 border-t border-gray-100 dark:border-gray-800"
          >
            <div>
              <p class="text-sm text-text-muted mb-1">Neutral</p>
              <p class="text-xl font-semibold text-yellow-500">6%</p>
            </div>
            <div>
              <p class="text-sm text-text-muted mb-1">Negative</p>
              <p class="text-xl font-semibold text-red-500">4%</p>
            </div>
          </div>
        </div>

        <div
          class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-6 flex flex-col"
        >
          <div class="flex justify-between items-center mb-6">
            <h3 class="text-lg font-bold text-text-main">To-do (2)</h3>
          </div>

          <button
            class="w-full flex items-center justify-center gap-2 text-sm font-medium text-primary hover:bg-primary/10 py-2 rounded-lg transition-colors border border-dashed border-primary/30 mb-4"
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

          <div class="space-y-3 flex-1">
            @for (task of todos(); track task.id) {
              <label
                class="flex items-start gap-3 p-2 hover:bg-gray-50 dark:hover:bg-gray-800/50 rounded-lg cursor-pointer transition-colors group"
              >
                <div class="pt-0.5">
                  <input
                    type="checkbox"
                    class="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary bg-transparent"
                  />
                </div>
                <div>
                  <p
                    class="text-sm font-medium text-text-main group-hover:text-primary transition-colors"
                  >
                    {{ task.title }}
                  </p>
                  <p class="text-xs text-text-muted mt-0.5 flex items-center gap-1">
                    <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                      ></path>
                    </svg>
                    {{ task.due }}
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
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4 transition-opacity animate-fade-in"
      >
        <div
          class="bg-surface w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden border border-gray-200 dark:border-gray-800 flex flex-col max-h-[85vh]"
        >
          <div
            class="px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center bg-gray-50/50 dark:bg-gray-900/50"
          >
            <div>
              <h3 class="text-lg font-bold text-text-main">Ticket Volume Report</h3>
              <p class="text-xs text-text-muted mt-0.5">
                Raw data comparison for today's active hours
              </p>
            </div>
            <button
              (click)="isReportModalOpen.set(false)"
              class="text-gray-400 hover:text-text-main hover:bg-gray-200 dark:hover:bg-gray-800 p-2 rounded-lg transition-colors"
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
            <table class="w-full text-sm text-left">
              <thead
                class="text-xs text-text-muted uppercase bg-gray-50 dark:bg-gray-900 sticky top-0 z-10 border-b border-gray-200 dark:border-gray-800"
              >
                <tr>
                  <th class="px-6 py-4 font-semibold">Time Interval</th>
                  <th class="px-6 py-4 font-semibold text-right">Today</th>
                  <th class="px-6 py-4 font-semibold text-right">Yesterday</th>
                  <th class="px-6 py-4 font-semibold text-right">Trend</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
                @for (row of reportData(); track row.time) {
                  <tr class="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors group">
                    <td class="px-6 py-4 font-medium text-text-main">{{ row.time }}</td>
                    <td class="px-6 py-4 text-right font-medium text-text-main">{{ row.today }}</td>
                    <td class="px-6 py-4 text-right text-text-muted">{{ row.yesterday }}</td>
                    <td
                      class="px-6 py-4 text-right font-semibold flex justify-end gap-1"
                      [class.text-red-500]="row.today > row.yesterday"
                      [class.text-green-500]="row.today < row.yesterday"
                      [class.text-gray-500]="row.today === row.yesterday"
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
            class="px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex justify-end gap-3 bg-gray-50/50 dark:bg-gray-900/50"
          >
            <button
              (click)="isReportModalOpen.set(false)"
              class="px-5 py-2.5 text-sm font-semibold text-text-main hover:bg-gray-200 dark:hover:bg-gray-700 border border-gray-300 dark:border-gray-700 rounded-xl transition-colors"
            >
              Close
            </button>
            <button
              class="px-5 py-2.5 text-sm font-semibold bg-primary hover:bg-primary-hover text-white rounded-xl shadow-lg shadow-primary/30 transition-all flex items-center gap-2"
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
  `,
})
export class AgentDashboardComponent implements AfterViewInit {
  // Grab the canvas element from the DOM
  @ViewChild('trendChart') trendChartRef!: ElementRef<HTMLCanvasElement>;
  private chartInstance: Chart | null = null;

  // Mock Data Signals
  kpiMetrics = signal([
    { label: 'Unresolved', value: 55, highlight: true, alert: false },
    { label: 'Overdue', value: 4, highlight: false, alert: true },
    { label: 'Due today', value: 11, highlight: false, alert: false },
    { label: 'Open', value: 28, highlight: false, alert: false },
    { label: 'On hold', value: 3, highlight: false, alert: false },
    { label: 'Unassigned', value: 8, highlight: false, alert: false },
  ]);
  // Agent Performance & Motivation Metrics
  performanceMetrics = signal([
    { label: 'Resolved', value: '45' },
    { label: 'Received', value: '100' },
    { label: 'Average first response time', value: '12m' },
    { label: 'Average response time', value: '24m 12s' },
    { label: 'Resolution within SLA', value: '91%' },
  ]);

  ticketGroups = signal([
    { name: 'Customer support', count: 32 },
    { name: 'Loyalty programs', count: 8 },
    { name: 'Vendor management', count: 12 },
    { name: 'Billing', count: 3 },
  ]);

  todos = signal([
    { id: 1, title: 'Followup with customer about Upgrade', due: 'In a day' },
    { id: 2, title: 'Billing reminder for Enterprise Corp', due: 'In 8 days' },
  ]);
  // Add these right below your other signals
  isReportModalOpen = signal<boolean>(false);

  // The raw data that feeds the chart
  reportData = signal([
    { time: '08:00 AM', today: 12, yesterday: 10 },
    { time: '10:00 AM', today: 19, yesterday: 15 },
    { time: '12:00 PM', today: 15, yesterday: 12 },
    { time: '02:00 PM', today: 25, yesterday: 18 },
    { time: '04:00 PM', today: 22, yesterday: 15 },
    { time: '06:00 PM', today: 30, yesterday: 20 },
    { time: '08:00 PM', today: 28, yesterday: 22 },
  ]);

  ngAfterViewInit() {
    this.renderChart();
  }

  private renderChart() {
    const ctx = this.trendChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    // Create a smooth gradient fill under the main line
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    // Using RGBA values of your #012A4A primary color
    gradient.addColorStop(0, 'rgba(1, 42, 74, 0.4)');
    gradient.addColorStop(1, 'rgba(1, 42, 74, 0.0)');

    this.chartInstance = new Chart(ctx, {
      type: 'line',
      data: {
        labels: ['8am', '10am', '12pm', '2pm', '4pm', '6pm', '8pm'],
        datasets: [
          {
            label: 'Today',
            data: [12, 19, 15, 25, 22, 30, 28],
            borderColor: '#012A4A', // Your primary Navy Blue
            backgroundColor: gradient,
            borderWidth: 3,
            tension: 0.4, // This creates the smooth curve
            fill: true,
            pointBackgroundColor: '#ffffff',
            pointBorderColor: '#012A4A',
            pointBorderWidth: 2,
            pointRadius: 4,
            pointHoverRadius: 6,
          },
          {
            label: 'Yesterday',
            data: [10, 15, 12, 18, 15, 20, 22],
            borderColor: '#9ca3af', // Tailwind gray-400
            borderWidth: 2,
            borderDash: [5, 5], // Makes the line dashed
            tension: 0.4,
            fill: false,
            pointRadius: 0, // Hide points for the secondary line
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
              font: { family: "'Inter', sans-serif" },
            },
          },
          tooltip: {
            mode: 'index',
            intersect: false,
            backgroundColor: '#111827',
            titleFont: { family: "'Inter', sans-serif" },
            bodyFont: { family: "'Inter', sans-serif" },
            padding: 12,
            cornerRadius: 8,
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: {
              color: 'rgba(156, 163, 175, 0.2)', // Subtle horizontal lines
            },
            border: { display: false },
            ticks: { font: { family: "'Inter', sans-serif" } },
          },
          x: {
            grid: {
              display: false, // Hide vertical lines for a cleaner look
            },
            border: { display: false },
            ticks: { font: { family: "'Inter', sans-serif" } },
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
