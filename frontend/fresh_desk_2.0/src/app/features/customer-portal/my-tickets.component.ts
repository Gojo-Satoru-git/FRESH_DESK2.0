import { Component, computed, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RaiseTicketComponent } from '../customer-portal/raise-ticket.component';

// ================= TYPE INTERFACES =================
interface TicketListItem {
  id: string;
  ticketNumber: string;
  title: string;
  status: string;
  priority: string;
  descriptionPreview: string;
  companyId: string;
  createdAt: string;
  updatedAt?: string;
}

interface Comment {
  id: string;
  authorId?: string;
  contactId?: string;
  body: string;
  visibility: string;
  createdAt: string;
  authorName?: string;
  contactName?: string;
}

interface TicketDetail {
  id: string;
  ticketNumber?: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  category: string;
  companyId: string;
  createdAt: string;
  updatedAt?: string;
  tags: string[];
  attachments?: string[];
  comments: Comment[];
}

// ================= BACKEND MOCK DATA SCHEMA =================
const SAMPLE_TICKETS: TicketDetail[] = [
  {
    id: 'tkt-001',
    ticketNumber: 'TCK-2026-001',
    title: 'Production Authentication Portal Loop Issue',
    description: 'Users are experiencing an infinite redirect loop when attempting to log in via OAuth2 providers on Chrome 124. Clearing cookies temporarily solves the problem, but it recurs on subsequent sessions.',
    status: 'Open',
    priority: 'Critical',
    category: 'Authentication',
    companyId: 'comp-alpha',
    createdAt: new Date(Date.now() - 3600000 * 4).toISOString(),
    updatedAt: new Date(Date.now() - 600000).toISOString(),
    tags: ['Auth', 'Bug', 'Production'],
    attachments: ['oauth_error_logs.txt', 'network_trace.har'],
    comments: [
      {
        id: 'c-101',
        contactId: 'user-01',
        contactName: 'Alex Rivers',
        body: 'This is completely blocking our external QA team from completing sanity tests.',
        visibility: 'public',
        createdAt: new Date(Date.now() - 3600000 * 3).toISOString()
      },
      {
        id: 'c-102',
        authorId: 'agent-99',
        authorName: 'Sarah Jenkins (Support)',
        body: 'Hi Alex, our engineering team has isolated the problem to a cross-site cookie attribute change. We are prepping a patch.',
        visibility: 'public',
        createdAt: new Date(Date.now() - 600000).toISOString()
      }
    ]
  },
  {
    id: 'tkt-002',
    ticketNumber: 'TCK-2026-002',
    title: 'Request for Sandbox API Rate Limit Extension',
    description: 'We are ramping up load testing for our new microservice architecture integration and need to provisionally increase our rate limit ceiling from 100 req/min to 1500 req/min in the staging environment.',
    status: 'Pending',
    priority: 'Medium',
    category: 'Infrastructure',
    companyId: 'comp-alpha',
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 3600000 * 12).toISOString(),
    tags: ['Config', 'Limits'],
    attachments: ['load_test_requirements.pdf'],
    comments: [
      {
        id: 'c-201',
        contactId: 'user-01',
        contactName: 'Alex Rivers',
        body: 'Hoping to execute this testing window by Thursday morning. Let me know if you need our source subnets.',
        visibility: 'public',
        createdAt: new Date(Date.now() - 86400000).toISOString()
      }
    ]
  },
  {
    id: 'tkt-003',
    ticketNumber: 'TCK-2026-003',
    title: 'Billing Invoice Discrepancy - May 2026',
    description: 'Our automatic corporate card renewal charged $1,450 instead of our contractually negotiated tiered rate of $1,200. Please issue a statement credit adjustment.',
    status: 'Resolved',
    priority: 'High',
    category: 'Billing',
    companyId: 'comp-alpha',
    createdAt: new Date(Date.now() - 86400000 * 5).toISOString(),
    updatedAt: new Date(Date.now() - 86400000 * 2).toISOString(),
    tags: ['Finance', 'Invoice'],
    attachments: [],
    comments: []
  }
];

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule, RaiseTicketComponent],
  template: `
    <div class="min-h-screen bg-[#F8FAFC] text-slate-800 font-sans flex flex-col">
      
      <header class="bg-gray-200 border-b border-slate-200 sticky top-0 z-20 rounded-xl shadow-lg w-full px-2 py-2">
        <div class="max-w-7xl ml-4 flex flex-col md:flex-row items-center justify-between gap-4">
          
          <div class="relative w-full mr-8 md:w-80 flex-shrink-0">
            <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-lg">🔍</span>
            <input
              type="text"
              placeholder="Search by ID or Subject..."
              class="w-full pl-9 pr-4 py-2 text-lg bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all placeholder:text-slate-400 text-slate-700"
              [ngModel]="searchTerm()"
              (ngModelChange)="searchTerm.set($event)"
            />
          </div>

          <nav class="flex gap-1 bg-gray-200 p-1 ml-20 rounded-xl border border-slate-200/50 w-full justify-center">
            @for (tab of statusTabs; track tab) {
              <button
                (click)="selectedStatus.set(tab)"
                class="px-8 py-2 rounded-xl text-xl font-bold transition-all duration-200 flex-1 md:flex-none text-center"
                [class.bg-white]="selectedStatus() === tab"
                [class.text-[#0F172A]]="selectedStatus() === tab"
                [class.shadow-sm]="selectedStatus() === tab"
                [class.text-slate-500]="selectedStatus() !== tab"
                [class.hover:text-slate-800]="selectedStatus() !== tab"
              >
                {{ tab }}
              </button>
            }
          </nav>

          
          
        </div>
      </header>

      <main class="max-w-7xl mx-auto p-6 w-full flex-1">
        @if (isLoadingList()) {
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @for (i of [1, 2, 3]; track i) {
              <div class="p-5 bg-white rounded-2xl border border-slate-200/60 animate-pulse space-y-3 shadow-sm">
                <div class="flex justify-between items-center">
                  <div class="h-4 w-20 bg-slate-200 rounded-md"></div>
                  <div class="h-5 w-14 bg-slate-200 rounded-full"></div>
                </div>
                <div class="h-5 w-3/4 bg-slate-200 rounded-md"></div>
                <div class="h-4 w-1/3 bg-slate-200 rounded-md"></div>
              </div>
            }
          </div>
        } @else if (filteredTickets().length === 0) {
          <div class="p-12 text-center text-slate-400 flex flex-col items-center justify-center bg-white border border-slate-200/60 rounded-3xl max-w-md mx-auto mt-12 shadow-sm">
            <span class="text-4xl mb-3">📬</span>
            <p class="text-xl font-bold text-slate-700">No matching workspace tickets</p>
            <p class="text-lg text-slate-400 mt-1">Adjust your upper filters or query string to load missing records.</p>
          </div>
        } @else {
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @for (ticket of filteredTickets(); track ticket.id) {
              <div
                (click)="openTicketDetails(ticket.id)"
                class="bg-white border border-gray-200 rounded-2xl p-5 cursor-pointer transition-all duration-200 shadow-xl hover:shadow-md hover:border-slate-300 relative overflow-hidden flex flex-col justify-between"
              >
                <div>
                  <div class="flex items-center justify-between mb-3">
                    <span class="text-lg font-bold text-slate-400 tracking-wider">
                      #{{ ticket.ticketNumber }}
                    </span>
                    <div class="flex items-center gap-1.5">
                      <span class="px-2.5 py-0.5 rounded-full text-sm font-extrabold uppercase border" [class]="getPriorityClasses(ticket.priority)">
                        {{ ticket.priority }}
                      </span>
                      <span class="px-2.5 py-0.5 rounded-full text-sm font-extrabold uppercase tracking-wider border" [class]="getStatusClasses(ticket.status)">
                        {{ ticket.status }}
                      </span>
                    </div>
                  </div>

                  <h4 class="text-xl font-bold text-slate-800 mb-2 line-clamp-2 transition-colors">
                    {{ ticket.title }}
                  </h4>
                  <p class="text-lg text-slate-400 line-clamp-2 mb-4 font-normal leading-relaxed">
                    {{ ticket.descriptionPreview }}
                  </p>
                </div>

                <div class="flex items-center justify-between text-lg text-slate-400 border-t border-slate-100 pt-3 mt-1 font-medium">
                  <span>Opened {{ ticket.createdAt | date: 'shortDate' }}</span>
                  @if (ticket.updatedAt) {
                    <span class="text-blue-500 bg-blue-50 px-2 py-0.5 rounded-md text-lg italic">
                      Active {{ formatDate(ticket.updatedAt) }}
                    </span>
                  }
                </div>
              </div>
            }
          </div>
        }
      </main>

      <!-- ===================== TICKET DETAIL MODAL (LARGE, WHATSAPP CHAT) ===================== -->
      @if (showDetailModal() && ticketDetail(); as t) {
  <div
    class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-50 flex items-center justify-center p-4"
    (click)="closeDetailModal()"
  >
    <div
      class="bg-white rounded-2xl w-full flex flex-col shadow-2xl border border-slate-100 overflow-hidden"
      style="max-width: 860px; height: 90vh;"
      (click)="$event.stopPropagation()"
    >
      
      <!-- Modal Header -->
    <div class="px-6 py-4 border-b border-slate-100 flex justify-between items-center bg-slate-50 flex-shrink-0">
      <div class="flex flex-wrap items-center gap-2">
        <span class="text-base font-bold px-3 py-1 bg-slate-200/70 text-slate-700 rounded-lg">
          TICKET #{{ t.ticketNumber || t.id.toUpperCase() }}
        </span>
        <span class="px-2.5 py-0.5 rounded-full text-xs font-extrabold uppercase tracking-wider border" [class]="getPriorityClasses(t.priority)">
          {{ t.priority }} Priority
        </span>
        <span class="px-2.5 py-0.5 rounded-full text-xs font-extrabold uppercase tracking-wider border" [class]="getStatusClasses(t.status)">
          {{ t.status }}
        </span>
      </div>
      <button
        (click)="closeDetailModal()"
        class="w-9 h-9 flex items-center justify-center rounded-full text-slate-400 hover:bg-slate-200 hover:text-slate-700 font-bold transition text-xl cursor-pointer"
      >
        &times;
      </button>
    </div>

      <!-- SINGLE scrollable body -->
      <div class="flex-1 overflow-y-auto flex flex-col">

        <!-- Ticket title + description -->
        <div class="px-6 pt-5 pb-4 border-b border-slate-100 flex-shrink-0">
          <h2 class="text-xl font-bold text-slate-900 mb-3">{{ t.title }}</h2>
          <div class="text-base text-slate-600 leading-relaxed bg-slate-50 border border-slate-200/50 rounded-xl p-4 whitespace-pre-wrap">
            {{ t.description }}
          </div>
        </div>

        <!-- Attachments -->
        @if (t.attachments && t.attachments.length > 0) {
          <div class="px-6 py-4 border-b border-slate-100 flex-shrink-0">
            <h4 class="text-base font-bold text-slate-900 mb-2">Uploaded Attachments</h4>
            <div class="flex flex-wrap gap-2">
              @for (file of t.attachments; track file) {
                <div class="flex items-center gap-2 px-3 py-1.5 bg-slate-50 border border-slate-200 rounded-xl text-sm font-medium text-slate-600 hover:text-blue-600 cursor-pointer transition-colors">
                  <span class="text-slate-400">📎</span>
                  <span>{{ file }}</span>
                </div>
              }
            </div>
          </div>
        }

        <!-- Conversation section label -->
        <div class="px-6 pt-4 pb-2 flex-shrink-0">
          <span class="text-xs font-bold uppercase tracking-widest text-slate-400">
            Conversation · {{ t.comments.length }} message{{ t.comments.length !== 1 ? 's' : '' }}
          </span>
        </div>

        <!-- Chat messages — grows to fill -->
        <div class="px-6 pb-4 space-y-4">
          @if (t.comments.length === 0) {
            <div class="py-10 text-center text-slate-400 text-sm italic">No messages yet. Start the conversation below.</div>
          }
          @for (comment of t.comments; track comment.id) {

            <!-- Customer message — right aligned -->
            @if (comment.contactId) {
              <div class="flex flex-col items-end gap-1">
                <span class="text-xs font-semibold text-slate-400 pr-1">
                  {{ comment.contactName || 'You' }}
                </span>
                <div class="max-w-[68%] bg-blue-600 text-white rounded-2xl rounded-tr-sm px-4 py-2.5 shadow-sm">
                  <p class="text-sm leading-relaxed whitespace-pre-wrap">{{ comment.body }}</p>
                  <p class="text-right text-[11px] text-blue-200 mt-1">
                    {{ comment.createdAt | date: 'shortTime' }}
                  </p>
                </div>
              </div>

            <!-- Agent message — left aligned -->
            } @else {
              <div class="flex flex-col items-start gap-1">
                <span class="text-xs font-semibold text-slate-500 pl-1">
                  {{ comment.authorName || 'Support Agent' }}
                </span>
                <div class="max-w-[68%] bg-slate-100 text-slate-800 rounded-2xl rounded-tl-sm px-4 py-2.5 shadow-sm border border-slate-200/60">
                  <p class="text-sm leading-relaxed whitespace-pre-wrap">{{ comment.body }}</p>
                  <p class="text-right text-[11px] text-slate-400 mt-1">
                    {{ comment.createdAt | date: 'shortTime' }}
                  </p>
                </div>
              </div>
            }

          }
        </div>
      </div>

      <!-- Message input — sticky at bottom, outside scroll area -->
      <div class="flex items-end gap-2 px-4 py-3 bg-white border-t border-slate-200 flex-shrink-0">
        <textarea
          rows="1"
          [(ngModel)]="newCommentText"
          placeholder="Type a reply..."
          (keydown.enter)="$event.preventDefault(); postComment(t.id)"
          class="flex-1 bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 resize-none text-slate-700 transition"
          style="max-height: 120px; overflow-y: auto;"
        ></textarea>
        <button
          (click)="postComment(t.id)"
          [disabled]="isPostingComment() || !newCommentText.trim()"
          class="h-10 w-10 rounded-xl bg-blue-600 text-white flex items-center justify-center disabled:opacity-40 transition hover:bg-blue-700 shadow-sm flex-shrink-0 cursor-pointer"
        >
          @if (isPostingComment()) {
            <span class="animate-spin inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full"></span>
          } @else {
            <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
              <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
            </svg>
          }
        </button>
      </div>

    </div>
  </div>
}

    </div>
  `,
})
export class MyTicketsComponent implements OnInit {
  private mockDatabase: TicketDetail[] = [...SAMPLE_TICKETS];

  statusTabs = ['All', 'Open', 'Pending', 'Resolved', 'Closed'];
  selectedStatus = signal<string>('All');
  searchTerm = signal<string>('');
  
  tickets = signal<TicketListItem[]>([]);
  selectedTicketId = signal<string | null>(null);
  ticketDetail = signal<TicketDetail | null>(null);

  showDetailModal = signal<boolean>(false);
  showRaiseTicket = signal<boolean>(false);

  isLoadingList = signal<boolean>(false);
  isLoadingDetails = signal<boolean>(false);
  isPostingComment = signal<boolean>(false);
  newCommentText = '';

  private setBodyScroll(locked: boolean) {
  document.body.style.overflow = locked ? 'hidden' : '';
}

  filteredTickets = computed(() => {
    return this.tickets().filter((ticket) => {
      const statusMatch =
        this.selectedStatus() === 'All' ||
        (this.selectedStatus() === 'Open' && ['New', 'Open', 'Assigned', 'InProgress', 'Reopened'].includes(ticket.status)) ||
        (this.selectedStatus() === 'Pending' && ticket.status === 'Pending') ||
        (this.selectedStatus() === 'Resolved' && ticket.status === 'Resolved') ||
        (this.selectedStatus() === 'Closed' && ticket.status === 'Closed');

      const search = this.searchTerm().toLowerCase();
      return statusMatch && (
        ticket.id.toLowerCase().includes(search) ||
        ticket.title.toLowerCase().includes(search) ||
        ticket.ticketNumber?.toLowerCase().includes(search)
      );
    });
  });

  ngOnInit() {
    this.fetchTicketsList();
  }

  fetchTicketsList() {
    this.isLoadingList.set(true);
    setTimeout(() => {
      const listItems: TicketListItem[] = this.mockDatabase.map(t => ({
        id: t.id,
        ticketNumber: t.ticketNumber || '',
        title: t.title,
        status: t.status,
        priority: t.priority,
        descriptionPreview: t.description.substring(0, 110) + '...',
        companyId: t.companyId,
        createdAt: t.createdAt,
        updatedAt: t.updatedAt
      }));

      this.tickets.set(listItems);
      this.isLoadingList.set(false);
    }, 400);
  }

  openTicketDetails(id: string) {
    this.selectedTicketId.set(id);
    this.showDetailModal.set(true);
    this.selectTicket(id);
    this.setBodyScroll(true);
  }

  closeDetailModal() {
    this.showDetailModal.set(false);
    this.selectedTicketId.set(null);
    this.ticketDetail.set(null);
    this.setBodyScroll(false);
  }

  openRaiseTicket() {
    this.showRaiseTicket.set(true);
    this.setBodyScroll(true);
  }

  closeRaiseTicket() {
    this.showRaiseTicket.set(false);
    this.setBodyScroll(false);
  }

  selectTicket(id: string) {
    this.isLoadingDetails.set(true);
    setTimeout(() => {
      const found = this.mockDatabase.find(t => t.id === id);
      if (found) {
        this.ticketDetail.set(JSON.parse(JSON.stringify(found)));
      }
      this.isLoadingDetails.set(false);
    }, 200);
  }

  postComment(ticketId: string) {
    if (!this.newCommentText.trim()) return;
    this.isPostingComment.set(true);

    setTimeout(() => {
      const dbRecord = this.mockDatabase.find(t => t.id === ticketId);
      if (dbRecord) {
        const newComment: Comment = {
          id: 'c-' + Math.random().toString(36).substring(2, 6),
          contactId: 'user-01',
          contactName: 'You (Contact)',
          body: this.newCommentText,
          visibility: 'public',
          createdAt: new Date().toISOString()
        };

        dbRecord.comments.push(newComment);
        dbRecord.updatedAt = new Date().toISOString();
        
        this.newCommentText = '';
        this.isPostingComment.set(false);
        
        this.fetchTicketsList();
        this.selectTicket(ticketId);
      }
    }, 300);
  }

  formatDate(dateStr: string): string {
    const diff = Math.floor((new Date().getTime() - new Date(dateStr).getTime()) / 1000);
    if (diff < 60) return 'just now';
    if (diff < 3600) return Math.floor(diff / 60) + 'm ago';
    if (diff < 86400) return Math.floor(diff / 3600) + 'h ago';
    return Math.floor(diff / 86400) + 'd ago';
  }

  getStatusClasses(status: string): string {
    const s = status.toLowerCase();
    if (s === 'new') return 'bg-indigo-50 text-indigo-700 border-indigo-200';
    if (['open', 'assigned', 'inprogress', 'reopened'].includes(s)) return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    if (s === 'pending') return 'bg-yellow-50 text-yellow-700 border-yellow-200';
    if (s === 'resolved') return 'bg-green-50 text-green-700 border-green-200';
    return 'bg-gray-100 text-slate-600 border-slate-200';
  }

  getPriorityClasses(priority: string): string {
    const p = priority.toLowerCase();
    if (p === 'critical') return 'bg-red-50 text-red-700 border-red-200';
    if (p === 'high') return 'bg-orange-50 text-orange-700 border-orange-200';
    if (p === 'medium') return 'bg-yellow-50 text-yellow-700 border-yellow-200';
    return 'bg-blue-50 text-blue-700 border-blue-200';
  }
}