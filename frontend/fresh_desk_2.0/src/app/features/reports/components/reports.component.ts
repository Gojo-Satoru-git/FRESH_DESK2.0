import { Component, signal, OnInit, inject } from '@angular/core';
import { InsightCardComponent } from './insight-card.component';
import { ReportCategoryComponent, ReportItem } from './report-category.component';
import { TicketService } from '../../tickets/services/ticket.service';
import { TicketDashboard, PagedResult, TicketListItem } from '../../tickets/models/ticket.model';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [InsightCardComponent, ReportCategoryComponent],
  template: `
    <div class="h-full flex flex-col animate-fade-in bg-background">
      <div class="bg-primary/10 border-b border-primary/20 p-3 px-6 flex items-center gap-3">
        <svg class="w-5 h-5 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          ></path>
        </svg>
        <span class="text-sm font-medium text-primary">
          Reports will continue until further notice. Do explore our Analytics as well and give your
          feedback.
        </span>
      </div>

      <div class="flex flex-col lg:flex-row flex-1 overflow-hidden">
        <div
          class="flex-1 overflow-y-auto scrollbar-hide p-6 lg:p-10 border-r border-gray-200 dark:border-gray-800"
        >
          <div class="relative mb-12 max-w-3xl">
            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <svg
                class="w-5 h-5 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                ></path>
              </svg>
            </div>
            <input
              type="text"
              placeholder="Ask me a question about your helpdesk"
              class="w-full pl-12 pr-4 py-3 bg-surface border border-gray-300 dark:border-gray-700 rounded-lg text-base focus:outline-none focus:ring-2 focus:ring-primary text-text-main placeholder:text-text-muted/60 transition-all shadow-sm"
            />
          </div>

          @for (category of categories(); track category.title) {
            <app-report-category
              [title]="category.title"
              [reports]="category.reports"
            ></app-report-category>
          }
        </div>

        <div
          class="w-full lg:w-96 flex-shrink-0 bg-gray-50/50 dark:bg-gray-900/30 overflow-y-auto scrollbar-hide p-6"
        >
          <div class="flex justify-between items-end mb-6">
            <div>
              <h3 class="text-base font-bold text-text-main flex items-center gap-2">
                Today's Insights
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
                    d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  ></path>
                </svg>
              </h3>
              <p class="text-xs text-text-muted mt-1">Last updated live</p>
            </div>
            <button class="text-sm font-medium text-primary hover:underline">Customize</button>
          </div>

          <div class="space-y-4">
            @for (insight of insights(); track insight.title) {
              <app-insight-card
                [trendDirection]="insight.trendDirection"
                [trendColor]="insight.trendColor"
                [trendValue]="insight.trendValue"
                [metric]="insight.metric"
                [title]="insight.title"
                [description]="insight.description"
              ></app-insight-card>
            }
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ReportsComponent implements OnInit {
  private ticketService = inject(TicketService);

  categories = signal([
    {
      title: 'Helpdesk Analysis',
      reports: [
        {
          title: 'Helpdesk In-depth',
          colorClass: 'text-green-600 bg-green-100 dark:bg-green-900/30 dark:text-green-400',
          iconSvg:
            '<svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>',
        },
        {
          title: 'Ticket Volume Trends',
          colorClass: 'text-green-600 bg-green-100 dark:bg-green-900/30 dark:text-green-400',
          iconSvg:
            '<svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z"></path></svg>',
        },
      ],
    },
    {
      title: 'Productivity',
      reports: [
        {
          title: 'Agent Performance',
          colorClass: 'text-blue-600 bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400',
          iconSvg:
            '<svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>',
        },
        {
          title: 'Group Performance',
          colorClass: 'text-blue-600 bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400',
          iconSvg:
            '<svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>',
        },
        {
          title: 'Time Sheet Summary',
          colorClass: 'text-blue-600 bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400',
          iconSvg:
            '<svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>',
        },
      ],
    },
    {
      title: 'Customer Happiness',
      reports: [
        {
          title: 'Top Customer Analysis',
          colorClass: 'text-purple-600 bg-purple-100 dark:bg-purple-900/30 dark:text-purple-400',
          iconSvg:
            '<svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z"></path></svg>',
        },
        {
          title: 'Satisfaction Survey',
          colorClass: 'text-purple-600 bg-purple-100 dark:bg-purple-900/30 dark:text-purple-400',
          iconSvg:
            '<svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>',
        },
      ],
    },
  ]);

  insights = signal<
    {
      trendDirection: 'up' | 'down';
      trendColor: 'red' | 'green';
      trendValue: string;
      metric: string;
      title: string;
      description: string;
    }[]
  >([]);

  ngOnInit(): void {
    this.loadInsights();
  }

  private loadInsights(): void {
    this.ticketService.getDashboard().subscribe({
      next: (dashboard) => {
        // Fetch tickets to compute response time metrics
        this.ticketService.searchTickets({ page: 1, pageSize: 100 }).subscribe({
          next: (res: PagedResult<TicketListItem>) => {
            const tickets: TicketListItem[] = res.items || [];

            let totalFirstResponseTimeMs = 0;
            let firstResponseCount = 0;

            tickets.forEach((ticket: TicketListItem) => {
              if (ticket.updatedAt) {
                const created = new Date(ticket.createdAt).getTime();
                const updated = new Date(ticket.updatedAt).getTime();
                totalFirstResponseTimeMs += updated - created;
                firstResponseCount++;
              }
            });

            const avgTimeMs =
              firstResponseCount > 0 ? totalFirstResponseTimeMs / firstResponseCount : 0;
            const avgMins = Math.floor(avgTimeMs / 60000);
            const avgSecs = Math.round((avgTimeMs % 60000) / 1000);
            const timeStr = avgMins > 0 ? `${avgMins}m ${avgSecs}s` : `${avgSecs}s`;

            // SLA: resolved or updated within 15 minutes
            const slaMetCount = tickets.filter((t) => {
              if (!t.updatedAt) return true;
              const created = new Date(t.createdAt).getTime();
              const updated = new Date(t.updatedAt).getTime();
              return updated - created <= 15 * 60 * 1000;
            }).length;
            const slaPercent =
              tickets.length > 0 ? Math.round((slaMetCount / tickets.length) * 100) : 100;

            this.insights.set([
              {
                trendDirection: 'up',
                trendColor: 'red',
                trendValue: 'Live',
                metric: (dashboard.totalActive + dashboard.resolvedClosed).toString(),
                title: 'Received tickets',
                description: 'Total ticket volume recorded in helpdesk.',
              },
              {
                trendDirection: 'up',
                trendColor: 'red',
                trendValue: 'Live',
                metric: dashboard.totalActive.toString(),
                title: 'Unresolved tickets',
                description: 'Active tickets currently awaiting resolution.',
              },
              {
                trendDirection: 'down',
                trendColor: 'green',
                trendValue: 'Live',
                metric: timeStr === '0s' ? '0m 0s' : timeStr,
                title: 'Average first response time',
                description: 'High fives, your team is keeping first responses fast!',
              },
              {
                trendDirection: 'down',
                trendColor: 'green',
                trendValue: 'Live',
                metric: timeStr === '0s' ? '0m 0s' : timeStr,
                title: 'Average response time',
                description: 'Customers love a highly responsive support team!',
              },
              {
                trendDirection: 'up',
                trendColor: 'green',
                trendValue: 'Live',
                metric: `${slaPercent}%`,
                title: 'First Response SLA',
                description: 'Nice work meeting response SLA thresholds!',
              },
            ]);
          },
          error: () => {
            this.setDefaultInsights(dashboard);
          },
        });
      },
      error: () => {
        this.setDefaultInsights({
          totalActive: 0,
          inProgress: 0,
          pendingReply: 0,
          resolvedClosed: 0,
        });
      },
    });
  }

  private setDefaultInsights(dashboard: TicketDashboard): void {
    this.insights.set([
      {
        trendDirection: 'up',
        trendColor: 'red',
        trendValue: 'Live',
        metric: (dashboard.totalActive + dashboard.resolvedClosed).toString(),
        title: 'Received tickets',
        description: 'Total ticket volume recorded in helpdesk.',
      },
      {
        trendDirection: 'up',
        trendColor: 'red',
        trendValue: 'Live',
        metric: dashboard.totalActive.toString(),
        title: 'Unresolved tickets',
        description: 'Active tickets currently awaiting resolution.',
      },
      {
        trendDirection: 'down',
        trendColor: 'green',
        trendValue: 'Live',
        metric: '0m 0s',
        title: 'Average first response time',
        description: 'High fives, your team is keeping first responses fast!',
      },
      {
        trendDirection: 'down',
        trendColor: 'green',
        trendValue: 'Live',
        metric: '0m 0s',
        title: 'Average response time',
        description: 'Customers love a highly responsive support team!',
      },
      {
        trendDirection: 'up',
        trendColor: 'green',
        trendValue: 'Live',
        metric: '100%',
        title: 'First Response SLA',
        description: 'Nice work meeting response SLA thresholds!',
      },
    ]);
  }
}
