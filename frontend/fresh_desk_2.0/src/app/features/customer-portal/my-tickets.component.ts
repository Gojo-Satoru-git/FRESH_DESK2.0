import { Component, computed, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { environment } from '../../../environments/environment.development';

interface TicketListItem {
  id: string;
  ticketNumber: string;
  title: string;
  status: string;
  priority: string;
  descriptionPreview: string;
  assignedAgentId?: string;
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

interface Activity {
  id: string;
  ticketId: string;
  activityType: string;
  oldValue?: string;
  newValue?: string;
  performedBy: string;
  performedAt: string;
  performedByName?: string;
}

interface TicketDetail {
  id: string;
  ticketNumber?: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  category: string;
  assignedAgentId?: string;
  reporterId?: string;
  companyId: string;
  createdAt: string;
  updatedAt?: string;
  tags: string[];
  comments: Comment[];
  activities?: Activity[];
  reporterName?: string;
  assignedAgentName?: string;
}

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex h-[calc(100vh-64px)] bg-[#F8FAFC] text-slate-800 font-sans overflow-hidden">
      <!-- ================= LEFT PANEL: TICKET LIST ================= -->
      <aside
        class="w-80 md:w-96 bg-white border-r border-slate-200 flex flex-col flex-shrink-0 shadow-sm z-10"
      >
        <!-- Header -->
        <div class="p-5 border-b border-slate-100 bg-white">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-2xl font-bold text-[#0F172A]">My Tickets</h2>
            <span class="text-xs font-semibold px-2 py-1 bg-slate-100 text-slate-600 rounded-full">
              {{ filteredTickets().length }} total
            </span>
          </div>

          <!-- Search Box -->
          <div class="relative w-full mb-3">
            <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm">🔍</span>
            <input
              type="text"
              placeholder="Search by ID or Subject..."
              class="w-full pl-9 pr-4 py-2 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all placeholder:text-slate-400 text-slate-700"
              [ngModel]="searchTerm()"
              (ngModelChange)="searchTerm.set($event)"
            />
          </div>

          <!-- Status Filters Tabs -->
          <div class="flex gap-1 bg-slate-50 p-1 rounded-xl border border-slate-200/50">
            @for (tab of statusTabs; track tab) {
              <button
                (click)="selectedStatus.set(tab)"
                class="flex-1 text-center py-1.5 rounded-lg text-xs font-semibold transition-all duration-200"
                [class.bg-white]="selectedStatus() === tab"
                [class.text-[#0F172A]]="selectedStatus() === tab"
                [class.shadow-sm]="selectedStatus() === tab"
                [class.text-slate-500]="selectedStatus() !== tab"
                [class.hover:text-slate-800]="selectedStatus() !== tab"
              >
                {{ tab }}
              </button>
            }
          </div>
        </div>

        <!-- Scrollable Ticket List -->
        <div class="flex-1 overflow-y-auto divide-y divide-slate-100">
          @if (isLoadingList()) {
            @for (i of [1, 2, 3, 4]; track i) {
              <div class="p-5 animate-pulse space-y-3">
                <div class="flex justify-between items-center">
                  <div class="h-4 w-24 bg-slate-200 rounded-md"></div>
                  <div class="h-5 w-16 bg-slate-200 rounded-full"></div>
                </div>
                <div class="h-5 w-3/4 bg-slate-200 rounded-md"></div>
                <div class="h-4 w-1/2 bg-slate-200 rounded-md"></div>
              </div>
            }
          } @else if (filteredTickets().length === 0) {
            <div
              class="p-8 text-center text-slate-400 flex flex-col items-center justify-center h-48"
            >
              <span class="text-3xl mb-2">📬</span>
              <p class="text-sm font-medium">No tickets found</p>
              <p class="text-xs text-slate-400 mt-1">Raise a ticket or adjust your filters</p>
            </div>
          } @else {
            @for (ticket of filteredTickets(); track ticket.id) {
              <div
                (click)="selectTicket(ticket.id)"
                class="p-5 hover:bg-slate-50/80 cursor-pointer transition-all duration-150 border-l-4"
                [class.border-blue-600]="selectedTicketId() === ticket.id"
                [class.bg-blue-50/20]="selectedTicketId() === ticket.id"
                [class.border-transparent]="selectedTicketId() !== ticket.id"
              >
                <div class="flex justify-between items-start mb-1.5">
                  <div class="flex items-center gap-1.5">
                    <span class="text-xs font-bold text-slate-400 tracking-wider">
                      #{{ ticket.ticketNumber }}
                    </span>
                    <span
                      class="px-2.5 py-0.5 rounded-full text-[9px] font-extrabold uppercase border"
                      [class]="getPriorityClasses(ticket.priority)"
                    >
                      {{ ticket.priority }}
                    </span>
                  </div>
                  <span
                    class="px-2.5 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider border"
                    [class]="getStatusClasses(ticket.status)"
                  >
                    {{ ticket.status }}
                  </span>
                </div>
                <h4
                  class="text-sm font-semibold text-slate-800 mb-1 line-clamp-1 group-hover:text-blue-600 transition-colors"
                >
                  {{ ticket.title }}
                </h4>
                <p class="text-xs text-slate-500 line-clamp-2 mb-2 leading-relaxed">
                  {{ ticket.descriptionPreview }}
                </p>
                <div class="flex items-center justify-between text-[11px] text-slate-400 font-medium">
                  <div class="flex items-center">
                    <span>{{ ticket.createdAt | date: 'shortDate' }}</span>
                    <span class="mx-1">•</span>
                    <span>Created</span>
                  </div>
                  @if (ticket.updatedAt) {
                    <span class="italic">Updated {{ formatDate(ticket.updatedAt) }}</span>
                  }
                </div>
              </div>
            }
          }
        </div>
      </aside>

      <!-- ================= CENTER PANEL: TICKET DETAILS ================= -->
      <main class="flex-1 bg-[#F8FAFC] flex flex-col min-w-0 overflow-y-auto">
        @if (selectedTicketId()) {
          @if (isLoadingDetails()) {
            <!-- Details Skeleton -->
            <div class="p-8 animate-pulse space-y-6">
              <div class="space-y-3">
                <div class="h-6 w-1/3 bg-slate-200 rounded-md"></div>
                <div class="h-8 w-2/3 bg-slate-200 rounded-md"></div>
              </div>
              <div class="h-32 w-full bg-slate-200 rounded-xl"></div>
              <div class="space-y-4">
                <div class="h-5 w-24 bg-slate-200 rounded-md"></div>
                <div class="h-16 w-full bg-slate-200 rounded-xl"></div>
                <div class="h-16 w-full bg-slate-200 rounded-xl"></div>
              </div>
            </div>
          } @else if (ticketDetail(); as t) {
            <!-- Content Wrapper -->
            <div class="p-6 md:p-8 space-y-6 max-w-4xl mx-auto w-full">
              <!-- Back Button on Mobile -->
              <div class="lg:hidden mb-2">
                <button
                  (click)="selectedTicketId.set(null)"
                  class="flex items-center text-sm font-semibold text-blue-600 hover:text-blue-700"
                >
                  ← Back to List
                </button>
              </div>

              <!-- Ticket Info Box -->
              <div
                class="bg-white rounded-2xl p-6 border border-slate-200/60 shadow-sm relative overflow-hidden"
              >
                <div
                  class="absolute top-0 left-0 right-0 h-1.5 bg-gradient-to-r from-blue-500 via-indigo-500 to-purple-500"
                ></div>

                <div class="flex flex-wrap items-center justify-between gap-3 mb-4 mt-1">
                  <div class="flex items-center gap-2">
                    <span
                      class="text-xs font-bold px-2.5 py-1 bg-slate-100 text-slate-500 rounded-lg"
                    >
                      TICKET #{{ t.ticketNumber || t.id.substring(0, 8).toUpperCase() }}
                    </span>
                    <span
                      class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider border"
                      [class]="getStatusClasses(t.status)"
                    >
                      {{ t.status }}
                    </span>
                    <span
                      class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider border"
                      [class]="getPriorityClasses(t.priority)"
                    >
                      {{ t.priority }}
                    </span>
                  </div>
                  <span class="text-xs text-slate-400 font-medium">
                    Raised {{ t.createdAt | date: 'medium' }}
                  </span>
                </div>

                <h1 class="text-2xl font-bold text-slate-900 mb-4">{{ t.title }}</h1>

                <div
                  class="text-sm text-slate-600 leading-relaxed bg-slate-50 rounded-xl p-4 border border-slate-100 whitespace-pre-wrap"
                >
                  {{ t.description }}
                </div>

                <!-- Tags -->
                @if (t.tags && t.tags.length > 0) {
                  <div class="flex flex-wrap gap-1.5 mt-4">
                    @for (tag of t.tags; track tag) {
                      <span
                        class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-medium border"
                        [class]="tag.toLowerCase() === 'sla breached' ? 'bg-red-50 text-red-700 border-red-200' : 'bg-blue-50 text-blue-700 border-blue-100'"
                      >
                        #{{ tag }}
                      </span>
                    }
                  </div>
                }
              </div>

              <!-- Comments Section -->
              <div class="space-y-4">
                <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 px-1">
                  <span>💬</span>
                  <span>Conversation History</span>
                  <span
                    class="text-xs bg-slate-100 text-slate-500 px-2 py-0.5 rounded-full font-semibold"
                  >
                    {{ t.comments.length }}
                  </span>
                </h3>

                <!-- Add Comment Form -->
                <div
                  class="bg-white rounded-2xl p-4 border border-slate-200/60 shadow-sm space-y-3"
                >
                  <textarea
                    rows="3"
                    [(ngModel)]="newCommentText"
                    placeholder="Write a message or reply to support..."
                    class="w-full border border-slate-200 rounded-xl p-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all placeholder:text-slate-400 resize-none text-slate-700"
                  ></textarea>
                  <div class="flex justify-between items-center">
                    <span class="text-xs text-slate-400 font-medium">
                      Pressing send alerts support team
                    </span>
                    <button
                      (click)="postComment(t.id)"
                      [disabled]="isPostingComment() || !newCommentText.trim()"
                      class="px-5 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-md shadow-blue-500/10 hover:shadow-blue-500/20 disabled:opacity-50 disabled:shadow-none transition-all flex items-center gap-1.5"
                    >
                      @if (isPostingComment()) {
                        <span
                          class="animate-spin inline-block w-3.5 h-3.5 border-2 border-white/30 border-t-white rounded-full"
                        ></span>
                      }
                      <span>Send Message</span>
                    </button>
                  </div>
                </div>

                <!-- Comments Timeline -->
                <div class="space-y-4 mt-4">
                  @if (t.comments.length === 0) {
                    <div class="text-center py-6 text-slate-400 text-sm">
                      No updates yet. Feel free to leave a note above.
                    </div>
                  } @else {
                    @for (comment of t.comments; track comment.id) {
                      <div
                        class="flex gap-4 p-4 rounded-2xl border transition-all duration-150"
                        [class.bg-blue-50/30]="comment.contactId"
                        [class.border-blue-100]="comment.contactId"
                        [class.bg-white]="comment.authorId"
                        [class.border-slate-200/60]="comment.authorId"
                      >
                        <!-- Avatar -->
                        <div class="flex-shrink-0">
                          <div
                            class="w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm text-white"
                            [class.bg-blue-500]="comment.contactId"
                            [class.bg-indigo-500]="comment.authorId"
                          >
                            {{ comment.contactId ? 'ME' : 'SP' }}
                          </div>
                        </div>

                        <!-- Body -->
                        <div class="flex-1 space-y-1 min-w-0">
                          <div class="flex items-center justify-between">
                            <span class="text-xs font-bold text-slate-800">
                              {{ comment.contactId ? (comment.contactName || 'You (Contact)') : (comment.authorName || 'Support Agent') }}
                            </span>
                            <span class="text-[10px] text-slate-400 font-medium">
                              {{ comment.createdAt | date: 'medium' }}
                            </span>
                          </div>
                          <p class="text-sm text-slate-600 whitespace-pre-wrap leading-relaxed">
                            {{ comment.body }}
                          </p>
                        </div>
                      </div>
                    }
                  }
                </div>
              </div>

              <!-- Activities Section -->
              @if (t.activities && t.activities.length > 0) {
                <div class="space-y-3 pt-4">
                  <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 px-1">
                    <span>⏳</span>
                    <span>Audit & Activity Timeline</span>
                  </h3>
                  <div
                    class="bg-white rounded-2xl p-4 border border-slate-200/60 shadow-sm space-y-4"
                  >
                    @for (act of t.activities; track act.id) {
                      <div
                        class="flex items-start gap-3 text-xs text-slate-500 border-l-2 border-slate-200 pl-4 py-1 relative"
                      >
                        <span
                          class="absolute -left-1.5 top-2.5 w-2.5 h-2.5 bg-slate-300 rounded-full border-2 border-white"
                        ></span>
                        <div class="flex-1">
                          <span class="font-semibold text-slate-800">{{ act.performedByName || act.performedBy }}</span>
                          <span class="mx-1">performed</span>
                          <span class="font-medium text-blue-600">{{ act.activityType }}</span>
                          @if (act.oldValue || act.newValue) {
                            <span class="mx-1">from</span>
                            <span
                              class="px-1.5 py-0.5 bg-slate-100 rounded text-slate-600 font-mono"
                              >{{ act.oldValue || 'None' }}</span
                            >
                            <span class="mx-1">to</span>
                            <span
                              class="px-1.5 py-0.5 bg-blue-50 text-blue-700 rounded font-mono"
                              >{{ act.newValue || 'None' }}</span
                            >
                          }
                        </div>
                        <span class="text-slate-400 text-[10px]">{{
                          act.performedAt | date: 'shortTime'
                        }}</span>
                      </div>
                    }
                  </div>
                </div>
              }
            </div>
          }
        } @else {
          <!-- Empty State -->
          <div
            class="flex-1 flex flex-col items-center justify-center p-8 text-center text-slate-400"
          >
            <div
              class="w-24 h-24 bg-blue-50 text-blue-500 rounded-full flex items-center justify-center text-4xl mb-4 shadow-inner"
            >
              📬
            </div>
            <h3 class="text-xl font-bold text-slate-800 mb-1">Select a Ticket</h3>
            <p class="text-sm max-w-sm text-slate-500">
              Choose a ticket from the left panel to read comments, view history, or send messages
              to support agents.
            </p>
          </div>
        }
      </main>

      <!-- ================= RIGHT PANEL: META & STATUS FLOW ================= -->
      @if (selectedTicketId() && ticketDetail(); as t) {
        <aside
          class="w-80 bg-white border-l border-slate-200 flex flex-col flex-shrink-0 shadow-sm z-10 hidden xl:flex"
        >
          <!-- Summary Header -->
          <div class="p-6 border-b border-slate-100">
            <h3 class="text-base font-bold text-slate-900 mb-4">Ticket Management</h3>

            <!-- Quick Actions -->
            <div class="space-y-2">
              @if (t.status === 'Resolved') {
                <button
                  (click)="changeStatusAction(t.id, 'Close')"
                  [disabled]="isActionLoading()"
                  class="w-full py-2.5 px-4 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-bold transition-all shadow-md shadow-blue-500/10 hover:shadow-blue-500/20 disabled:opacity-50 flex items-center justify-center gap-1.5"
                >
                  <span>Close Ticket</span>
                </button>
                <button
                  (click)="changeStatusAction(t.id, 'Reopen')"
                  [disabled]="isActionLoading()"
                  class="w-full py-2.5 px-4 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-xs font-bold transition-all shadow-md shadow-emerald-500/10 hover:shadow-emerald-500/20 disabled:opacity-50 mt-1 flex items-center justify-center gap-1.5"
                >
                  <span>Reopen Ticket</span>
                </button>
              } @else if (t.status === 'Closed') {
                <div
                  class="p-3 bg-slate-50 border border-slate-200 text-center rounded-xl text-slate-500 text-xs font-semibold"
                >
                  Closed & Archived
                </div>
              }
              <button
                (click)="openEditModal()"
                class="w-full py-2 px-4 border border-slate-200 hover:bg-slate-50 text-slate-700 rounded-xl text-xs font-semibold transition-all mt-1 flex items-center justify-center gap-1.5"
              >
                <span>✏️ Edit Details</span>
              </button>
            </div>
          </div>

          <!-- Ticket Info Details -->
          <div class="p-6 space-y-6 flex-1 overflow-y-auto">
            <div class="space-y-4">
              <div>
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider"
                  >Priority</span
                >
                <div class="mt-1">
                  <span
                    class="px-2.5 py-1 rounded-full text-xs font-semibold border"
                    [class]="getPriorityClasses(t.priority)"
                  >
                    {{ t.priority }}
                  </span>
                </div>
              </div>

              <div>
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider"
                  >Category</span
                >
                <div class="mt-1 font-semibold text-slate-700 text-sm">
                  {{ t.category || 'Support' }}
                </div>
              </div>

              <div>
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider"
                  >Assigned Agent</span
                >
                <div class="mt-1.5 flex items-center gap-2">
                  <div
                    class="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-xs text-slate-600 font-bold"
                  >
                    {{ t.assignedAgentId ? 'A' : '?' }}
                  </div>
                  <span class="text-sm font-medium text-slate-700">
                    {{ t.assignedAgentId ? (t.assignedAgentName || 'Support Agent') : 'Unassigned (In Queue)' }}
                  </span>
                </div>
              </div>

              <div>
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider block mb-1.5"
                  >Reporter</span
                >
                <div class="flex items-center gap-2">
                  <div
                    class="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-xs text-slate-600 font-bold"
                  >
                    R
                  </div>
                  <span class="text-xs font-semibold text-slate-700 truncate max-w-[180px]" [title]="t.reporterName || t.reporterId || ''">
                    {{ t.reporterName || t.reporterId || 'Customer' }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Dates -->
            <div class="pt-6 border-t border-slate-100 space-y-3">
              <div>
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider block mb-0.5">Created At</span>
                <span class="text-xs font-semibold text-slate-700">{{ t.createdAt | date: 'medium' }}</span>
              </div>
              @if (t.updatedAt) {
                <div>
                  <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider block mb-0.5">Last Updated</span>
                  <span class="text-xs font-semibold text-slate-700">{{ t.updatedAt | date: 'medium' }}</span>
                </div>
              }
            </div>

            <!-- Visual Status Flow Timeline -->
            <div class="pt-6 border-t border-slate-100">
              <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider block mb-4"
                >Status Progress Flow</span
              >
              <div class="space-y-4 relative pl-6">
                <!-- Vertical Progress Line -->
                <div class="absolute left-2 top-2 bottom-2 w-0.5 bg-slate-100"></div>

                <!-- Steps -->
                @for (step of statusFlow; track step; let i = $index) {
                  <div class="relative flex gap-3 text-xs items-center">
                    <div
                      class="absolute -left-[22px] w-5 h-5 rounded-full flex items-center justify-center border-2 transition-all"
                      [class.bg-blue-600]="isCompletedStep(step) || isCurrentStep(step)"
                      [class.border-blue-600]="isCompletedStep(step) || isCurrentStep(step)"
                      [class.text-white]="isCompletedStep(step) || isCurrentStep(step)"
                      [class.bg-white]="!isCompletedStep(step) && !isCurrentStep(step)"
                      [class.border-slate-200]="!isCompletedStep(step) && !isCurrentStep(step)"
                    >
                      @if (isCompletedStep(step)) {
                        <span class="text-[9px]">✓</span>
                      } @else if (isCurrentStep(step)) {
                        <span class="w-1.5 h-1.5 bg-white rounded-full"></span>
                      } @else {
                        <span class="text-[8px] text-slate-400">{{ i + 1 }}</span>
                      }
                    </div>
                    <div>
                      <p
                        class="font-bold"
                        [class.text-slate-800]="isCompletedStep(step) || isCurrentStep(step)"
                        [class.text-slate-400]="!isCompletedStep(step) && !isCurrentStep(step)"
                      >
                        {{ step }}
                      </p>
                    </div>
                  </div>
                }
              </div>
            </div>
          </div>
        </aside>
      }
    </div>

    <!-- Edit Ticket Modal -->
    @if (isEditingTicket()) {
      <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white w-full max-w-lg rounded-2xl shadow-2xl border border-slate-200 flex flex-col max-h-[90vh]">
          <div class="px-6 py-4 border-b border-slate-100 flex justify-between items-center bg-slate-50/50 rounded-t-2xl">
            <h3 class="text-lg font-bold text-slate-900">Edit Ticket Details</h3>
            <button (click)="isEditingTicket.set(false)" class="text-slate-400 hover:text-slate-600">
              ✕
            </button>
          </div>
          <div class="p-6 overflow-y-auto space-y-4">
            <div>
              <label class="block text-sm font-semibold text-slate-700 mb-1">Title *</label>
              <input
                type="text"
                [(ngModel)]="editFormTitle"
                class="w-full px-3 py-2 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 text-sm text-slate-700"
              />
            </div>
            <div>
              <label class="block text-sm font-semibold text-slate-700 mb-1">Description *</label>
              <textarea
                rows="4"
                [(ngModel)]="editFormDescription"
                class="w-full px-3 py-2 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 text-sm text-slate-700"
              ></textarea>
            </div>
            <div>
              <label class="block text-sm font-semibold text-slate-700 mb-1">Tags (comma-separated)</label>
              <input
                type="text"
                [(ngModel)]="editFormTags"
                class="w-full px-3 py-2 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 text-sm text-slate-700"
              />
            </div>
          </div>
          <div class="px-6 py-4 border-t border-slate-100 bg-slate-50/50 flex justify-end gap-3 rounded-b-2xl">
            <button
              (click)="isEditingTicket.set(false)"
              class="px-4 py-2 text-xs font-semibold text-slate-500 hover:bg-slate-100 rounded-lg"
            >
              Cancel
            </button>
            <button
              (click)="saveTicketDetails()"
              [disabled]="isSavingDetails() || !editFormTitle.trim() || !editFormDescription.trim()"
              class="px-4 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-lg shadow-md disabled:opacity-50"
            >
              {{ isSavingDetails() ? 'Saving...' : 'Save Changes' }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Toast notification -->
    @if (toastMessage()) {
      <div
        class="fixed bottom-6 right-6 bg-slate-900 text-white text-sm font-semibold px-5 py-3 rounded-xl shadow-2xl animate-fade-in z-50 flex items-center gap-2"
      >
        <div class="w-2.5 h-2.5 rounded-full" [class.bg-emerald-400]="!toastMessage()!.toLowerCase().includes('failed')" [class.bg-red-400]="toastMessage()!.toLowerCase().includes('failed')"></div>
        {{ toastMessage() }}
      </div>
    }
  `,
})
export class MyTicketsComponent implements OnInit {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);

  statusTabs = ['All', 'Open', 'Pending', 'Resolved', 'Closed'];
  selectedStatus = signal<string>('All');
  searchTerm = signal<string>('');

  tickets = signal<TicketListItem[]>([]);
  selectedTicketId = signal<string | null>(null);
  ticketDetail = signal<TicketDetail | null>(null);

  isLoadingList = signal<boolean>(false);
  isLoadingDetails = signal<boolean>(false);
  isPostingComment = signal<boolean>(false);
  isActionLoading = signal<boolean>(false);
  isSavingDetails = signal<boolean>(false);
  toastMessage = signal<string | null>(null);

  showToast(msg: string) {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3000);
  }

  newCommentText = '';

  statusFlow = ['New', 'Open', 'Assigned', 'InProgress', 'Pending', 'Resolved', 'Closed'];

  // Edit form state
  isEditingTicket = signal<boolean>(false);
  editFormTitle = '';
  editFormDescription = '';
  editFormTags = '';

  filteredTickets = computed(() => {
    return this.tickets().filter((ticket) => {
      const statusMatch =
        this.selectedStatus() === 'All' ||
        (this.selectedStatus() === 'Open' &&
          ['New', 'Open', 'Assigned', 'InProgress', 'Reopened'].includes(ticket.status)) ||
        (this.selectedStatus() === 'Pending' && ticket.status === 'Pending') ||
        (this.selectedStatus() === 'Resolved' && ticket.status === 'Resolved') ||
        (this.selectedStatus() === 'Closed' && ticket.status === 'Closed');

      const search = this.searchTerm().toLowerCase();
      const searchMatch =
        ticket.id.toLowerCase().includes(search) ||
        ticket.title.toLowerCase().includes(search) ||
        ticket.ticketNumber?.toLowerCase().includes(search);

      return statusMatch && searchMatch;
    });
  });

  ngOnInit() {
    const id = this.route.snapshot.queryParams['id'];
    if (id) {
      this.selectedTicketId.set(id);
      this.selectTicket(id);
    }
    this.fetchTicketsList();
  }

  fetchTicketsList() {
    this.isLoadingList.set(true);
    this.http.get<{ items: TicketListItem[] }>(`${environment.apiUrl}/api/tickets/my`).subscribe({
      next: (response) => {
        this.tickets.set(response.items || []);
        this.isLoadingList.set(false);
        // Auto select first ticket if available and none selected
        if (response.items && response.items.length > 0 && !this.selectedTicketId()) {
          this.selectTicket(response.items[0].id);
        }
      },
      error: () => {
        this.isLoadingList.set(false);
      },
    });
  }

  selectTicket(id: string) {
    this.selectedTicketId.set(id);
    this.isLoadingDetails.set(true);
    this.newCommentText = '';

    this.http.get<TicketDetail>(`${environment.apiUrl}/api/tickets/${id}`).subscribe({
      next: (detail) => {
        // Fetch activities for audit timeline
        this.http.get<Activity[]>(`${environment.apiUrl}/api/tickets/${id}/activities`).subscribe({
          next: (activities) => {
            detail.activities = activities || [];
            this.ticketDetail.set(detail);
            this.isLoadingDetails.set(false);
          },
          error: () => {
            detail.activities = [];
            this.ticketDetail.set(detail);
            this.isLoadingDetails.set(false);
          },
        });
      },
      error: () => {
        this.isLoadingDetails.set(false);
      },
    });
  }

  postComment(ticketId: string) {
    if (!this.newCommentText.trim()) return;
    this.isPostingComment.set(true);

    const body = {
      body: this.newCommentText,
      visibility: 0, // Public
    };

    this.http.post(`${environment.apiUrl}/api/tickets/${ticketId}/comments`, body).subscribe({
      next: () => {
        this.isPostingComment.set(false);
        this.newCommentText = '';
        this.showToast('Message sent!');
        // Refresh details
        this.selectTicket(ticketId);
      },
      error: () => {
        this.isPostingComment.set(false);
        this.showToast('Failed to send message.');
      },
    });
  }

  changeStatusAction(ticketId: string, action: 'Resolve' | 'Reopen' | 'Close') {
    this.isActionLoading.set(true);
    const actorId = this.authService.currentUser()?.id;

    let url = '';
    let payload = {};

    if (action === 'Resolve') {
      url = `${environment.apiUrl}/api/tickets/${ticketId}/resolve`;
      payload = { resolvedBy: actorId, resolutionSummary: 'Resolved by customer' };
    } else if (action === 'Reopen') {
      url = `${environment.apiUrl}/api/tickets/${ticketId}/reopen`;
      payload = { reopenedBy: actorId, reason: 'Reopened by customer' };
    } else if (action === 'Close') {
      url = `${environment.apiUrl}/api/tickets/${ticketId}/close`;
      payload = { closedBy: actorId, notes: 'Closed by customer' };
    }

    this.http.post(url, payload).subscribe({
      next: () => {
        this.isActionLoading.set(false);
        const actionPast = action === 'Resolve' ? 'resolved' : action === 'Reopen' ? 'reopened' : 'closed';
        this.showToast(`Ticket successfully ${actionPast}!`);
        // Refresh ticket details & list
        this.selectTicket(ticketId);
        this.fetchTicketsList();
      },
      error: () => {
        this.isActionLoading.set(false);
        this.showToast(`Failed to ${action.toLowerCase()} ticket.`);
      },
    });
  }

  openEditModal() {
    const t = this.ticketDetail();
    if (!t) return;
    this.editFormTitle = t.title;
    this.editFormDescription = t.description;
    this.editFormTags = (t.tags || []).join(', ');
    this.isEditingTicket.set(true);
  }

  saveTicketDetails() {
    const t = this.ticketDetail();
    if (!t) return;
    this.isSavingDetails.set(true);
    const tags = this.editFormTags
      .split(',')
      .map((tg) => tg.trim())
      .filter((tg) => tg.length > 0);

    const payload = {
      title: this.editFormTitle.trim(),
      description: this.editFormDescription.trim(),
      priority: t.priority,
      category: t.category,
      tags: tags,
    };

    this.http.put(`${environment.apiUrl}/api/tickets/${t.id}`, payload).subscribe({
      next: () => {
        this.isSavingDetails.set(false);
        this.isEditingTicket.set(false);
        this.showToast('Ticket details saved!');
        // Refresh list & detail
        this.selectTicket(t.id);
        this.fetchTicketsList();
      },
      error: (err) => {
        this.isSavingDetails.set(false);
        this.showToast('Failed to save ticket details.');
        console.error('Failed to update ticket', err);
      }
    });
  }

  isCompletedStep(step: string): boolean {
    const t = this.ticketDetail();
    if (!t) return false;
    const currentIdx = this.statusFlow.findIndex((s) => s.toLowerCase() === t.status.toLowerCase());
    const stepIdx = this.statusFlow.findIndex((s) => s.toLowerCase() === step.toLowerCase());
    return stepIdx < currentIdx;
  }

  isCurrentStep(step: string): boolean {
    const t = this.ticketDetail();
    if (!t) return false;
    return t.status.toLowerCase() === step.toLowerCase();
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    const now = new Date();
    const diff = Math.floor((now.getTime() - d.getTime()) / 1000);
    if (diff < 60) return 'just now';
    if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
    return `${Math.floor(diff / 86400)}d ago`;
  }

  getStatusClasses(status: string): string {
    const s = status.toLowerCase();
    if (s === 'new') return 'bg-indigo-50 text-indigo-700 border-indigo-200';
    if (['open', 'assigned', 'inprogress', 'reopened'].includes(s))
      return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    if (s === 'pending') return 'bg-yellow-50 text-yellow-700 border-yellow-200';
    if (s === 'resolved') return 'bg-green-50 text-green-700 border-green-200';
    if (s === 'closed') return 'bg-gray-100 text-slate-600 border-slate-200';
    return 'bg-slate-50 text-slate-500 border-slate-200';
  }

  getPriorityClasses(priority: string): string {
    const p = priority.toLowerCase();
    if (p === 'critical') return 'bg-red-50 text-red-700 border-red-200';
    if (p === 'high') return 'bg-orange-50 text-orange-700 border-orange-200';
    if (p === 'medium') return 'bg-yellow-50 text-yellow-700 border-yellow-200';
    if (p === 'low') return 'bg-blue-50 text-blue-700 border-blue-200';
    return 'bg-slate-50 text-slate-500 border-slate-200';
  }
}
