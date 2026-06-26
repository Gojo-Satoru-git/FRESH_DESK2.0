import { Component, computed, signal, OnInit, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RaiseTicketComponent } from './raise-ticket.component';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';

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
  type: string;
  companyId: string;
  createdAt: string;
  updatedAt?: string;
  tags: string[];
  attachments?: {
    id: string;
    fileName: string;
    fileUrl: string;
    mimeType: string;
    fileSizeBytes: number;
  }[];
  statusHistory: {
    fromStatus: string | null;
    toStatus: string;
    createdAt: string;
    reason?: string;
  }[];
  comments: Comment[];
}

interface ConfirmDialog {
  show: boolean;
  type: 'info' | 'success' | 'alert' | 'critical';
  title: string;
  message: string;
  onConfirm: () => void;
}

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule, RaiseTicketComponent],
  template: `
    <div class="min-h-screen bg-[#F8FAFC] text-slate-800 font-sans flex flex-col">
      <header
        class="bg-gray-200 border-b border-slate-200 sticky top-0 z-20 rounded-xl shadow-lg w-full px-2 py-2"
      >
        <div
          class="max-w-7xl mx-auto w-full flex flex-col md:flex-row items-center justify-between gap-4"
        >
          <div class="relative w-full md:w-80 flex-shrink-0">
            <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-lg">🔍</span>
            <input
              type="text"
              placeholder="Search by ID or Subject..."
              class="w-full pl-9 pr-4 py-2 text-lg bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all placeholder:text-slate-400 text-slate-700"
              [ngModel]="searchTerm()"
              (ngModelChange)="searchTerm.set($event)"
            />
          </div>

          <nav
            class="flex gap-1 bg-gray-200 p-1 rounded-xl border border-slate-200/50 flex-1 overflow-x-auto justify-start [&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]"
          >
            @for (tab of statusTabs; track tab) {
              <button
                (click)="selectedStatus.set(tab)"
                class="px-6 md:px-8 py-2 rounded-xl text-xl font-bold transition-all duration-200 flex-shrink-0 whitespace-nowrap text-center"
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

          <button
            (click)="openRaiseTicket()"
            class="flex-shrink-0 ml-0 md:ml-4 w-full md:w-auto justify-center px-6 py-2.5 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 transition-colors shadow-sm flex items-center gap-2"
          >
            <span class="text-2xl leading-none font-normal">+</span>
            New Ticket
          </button>
        </div>
      </header>

      <main class="max-w-7xl mx-auto p-6 w-full flex-1">
        @if (isLoadingList()) {
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @for (i of [1, 2, 3]; track i) {
              <div
                class="p-5 bg-white rounded-2xl border border-slate-200/60 animate-pulse space-y-3 shadow-sm"
              >
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
          <div
            class="p-12 text-center text-slate-400 flex flex-col items-center justify-center bg-white border border-slate-200/60 rounded-3xl max-w-md mx-auto mt-12 shadow-sm"
          >
            <span class="text-4xl mb-3">📬</span>
            <p class="text-xl font-bold text-slate-700">No matching workspace tickets</p>
            <p class="text-lg text-slate-400 mt-1">
              Adjust your upper filters or query string to load missing records.
            </p>
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
                      <span
                        class="px-2.5 py-0.5 rounded-full text-sm font-extrabold uppercase border"
                        [class]="getPriorityClasses(ticket.priority)"
                      >
                        {{ ticket.priority }}
                      </span>
                      <span
                        class="px-2.5 py-0.5 rounded-full text-sm font-extrabold uppercase tracking-wider border"
                        [class]="getStatusClasses(ticket.status)"
                      >
                        {{ getCustomerStatusLabel(ticket.status) }}
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

                <div
                  class="flex items-center justify-between text-lg text-slate-400 border-t border-slate-100 pt-3 mt-1 font-medium"
                >
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

      <!-- ===================== TICKET DETAIL MODAL ===================== -->
      @if (showDetailModal()) {
        <div
          class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-50 flex items-center justify-center p-4"
          (click)="closeDetailModal()"
        >
          <div
            class="bg-white rounded-2xl w-full flex flex-col shadow-2xl border border-slate-100 overflow-hidden relative"
            style="max-width: 860px; height: 90vh;"
            (click)="$event.stopPropagation()"
          >
            @if (isLoadingDetails()) {
              <!-- SKELETON OVERLAY -->
              <div class="absolute inset-0 bg-white z-20 flex flex-col p-6 animate-pulse">
                <div class="flex justify-between items-center border-b border-slate-100 pb-4 mb-4">
                  <div class="flex gap-2">
                    <div class="h-8 w-24 bg-slate-200 rounded-lg"></div>
                    <div class="h-8 w-20 bg-slate-200 rounded-full"></div>
                    <div class="h-8 w-20 bg-slate-200 rounded-full"></div>
                  </div>
                  <div class="flex gap-2">
                    <div class="h-10 w-10 bg-slate-200 rounded-full"></div>
                    <div class="h-10 w-10 bg-slate-200 rounded-full"></div>
                  </div>
                </div>
                <div class="flex-1 space-y-6 mt-4">
                  <div class="h-8 w-3/4 bg-slate-200 rounded-md mb-6"></div>
                  <div class="h-4 w-full bg-slate-200 rounded-md"></div>
                  <div class="h-4 w-5/6 bg-slate-200 rounded-md"></div>
                  <div class="h-4 w-4/6 bg-slate-200 rounded-md"></div>
                  <div class="mt-12 flex gap-4">
                    <div class="h-12 w-12 bg-slate-200 rounded-full flex-shrink-0"></div>
                    <div class="flex-1 space-y-4">
                      <div class="h-24 w-full bg-slate-200 rounded-2xl rounded-tl-none"></div>
                      <div class="h-24 w-3/4 bg-slate-200 rounded-2xl rounded-tl-none"></div>
                    </div>
                  </div>
                </div>
              </div>
            }

            @if (ticketDetail(); as t) {
              <!-- Modal Header -->
              <div
                class="px-6 py-4 border-b border-slate-100 flex justify-between items-center bg-slate-50 flex-shrink-0"
              >
                <div class="flex flex-wrap items-center gap-2">
                  <span
                    class="text-base font-bold px-3 py-1 bg-slate-200/70 text-slate-700 rounded-lg"
                  >
                    TICKET #{{ t.ticketNumber || t.id.toUpperCase() }}
                  </span>
                  <span
                    class="px-2.5 py-0.5 rounded-full text-xs font-extrabold uppercase tracking-wider border"
                    [class]="getPriorityClasses(t.priority)"
                  >
                    {{ t.priority }} Priority
                  </span>
                  <span
                    class="px-2.5 py-0.5 rounded-full text-xs font-extrabold uppercase tracking-wider border"
                    [class]="getStatusClasses(t.status)"
                  >
                    {{ getCustomerStatusLabel(t.status) }}
                  </span>
                </div>
                <div class="flex items-center gap-3">
                  @if (t.status !== 'Closed' && t.status !== 'Resolved') {
                    <button
                      (click)="isEditingTicket.set(!isEditingTicket()); initEditForm(t)"
                      class="px-3 py-1.5 bg-blue-50 text-blue-600 rounded-lg hover:bg-blue-100 text-sm font-bold transition cursor-pointer"
                    >
                      {{ isEditingTicket() ? 'Cancel' : 'Edit' }}
                    </button>
                  }

                  <!-- BEFORE -->
                  @if (t.status === 'Resolved') {
                    <!-- Close Ticket button -->
                    <!-- Reopen Ticket button -->
                  }

                  <!-- AFTER -->
                  @if (t.status.toLowerCase() === 'resolved') {
                    <button
                      (click)="closeTicket(t.id)"
                      class="px-4 py-1.5 bg-red-600 text-white rounded-lg hover:bg-red-700 text-sm font-bold transition cursor-pointer shadow-sm flex items-center gap-1.5"
                    >
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        class="h-3.5 w-3.5"
                        viewBox="0 0 20 20"
                        fill="currentColor"
                      >
                        <path
                          fill-rule="evenodd"
                          d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                          clip-rule="evenodd"
                        />
                      </svg>
                      Close Ticket
                    </button>
                  }

                  @if (
                    t.status.toLowerCase() === 'resolved' || t.status.toLowerCase() === 'closed'
                  ) {
                    <button
                      (click)="reopenTicket(t.id)"
                      class="px-4 py-1.5 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 text-sm font-bold transition cursor-pointer shadow-sm flex items-center gap-1.5"
                    >
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        class="h-3.5 w-3.5"
                        viewBox="0 0 20 20"
                        fill="currentColor"
                      >
                        <path
                          fill-rule="evenodd"
                          d="M4 2a1 1 0 011 1v2.101a7.002 7.002 0 0111.601 2.566 1 1 0 11-1.885.666A5.002 5.002 0 005.999 7H9a1 1 0 010 2H4a1 1 0 01-1-1V3a1 1 0 011-1zm.008 9.057a1 1 0 011.276.61A5.002 5.002 0 0014.001 13H11a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0v-2.101a7.002 7.002 0 01-11.601-2.566 1 1 0 01.61-1.276z"
                          clip-rule="evenodd"
                        />
                      </svg>
                      Reopen Ticket
                    </button>
                  }

                  <button
                    (click)="closeDetailModal()"
                    class="w-9 h-9 flex items-center justify-center rounded-full text-slate-400 hover:bg-slate-200 hover:text-slate-700 font-bold transition text-xl cursor-pointer ml-2"
                  >
                    &times;
                  </button>
                </div>
              </div>

              <!-- SINGLE scrollable body -->
              <div class="flex-1 overflow-y-auto flex flex-col">
                <!-- Ticket Details -->
                <div class="px-6 pt-5 pb-4 border-b border-slate-100 flex-shrink-0">
                  @if (isEditingTicket()) {
                    <div
                      class="flex flex-col gap-4 bg-slate-50 border border-slate-200 p-5 rounded-xl shadow-inner"
                    >
                      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-2">
                        <div class="flex flex-col gap-1">
                          <label class="text-xs font-bold text-slate-500 uppercase tracking-wider"
                            >Ticket Type</label
                          >
                          <select
                            [(ngModel)]="editForm.type"
                            class="w-full px-3 py-2 border border-slate-200 rounded-lg text-slate-700 focus:ring-2 focus:ring-blue-500 outline-none text-sm"
                          >
                            <option value="Bug">Bug</option>
                            <option value="Incident">Incident</option>
                            <option value="Service Request">Service Request</option>
                            <option value="Change Request">Change Request</option>
                            <option value="Feature Request">Feature Request</option>
                            <option value="Access Request">Access Request</option>
                          </select>
                        </div>

                        <div class="flex flex-col gap-1">
                          <label class="text-xs font-bold text-slate-500 uppercase tracking-wider"
                            >Priority</label
                          >
                          <select
                            [(ngModel)]="editForm.priority"
                            class="w-full px-3 py-2 border border-slate-200 rounded-lg text-slate-700 focus:ring-2 focus:ring-blue-500 outline-none text-sm"
                          >
                            <option value="Low">Low</option>
                            <option value="Medium">Medium</option>
                            <option value="High">High</option>
                            <option value="Critical">Critical</option>
                          </select>
                        </div>
                      </div>

                      <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold text-slate-500 uppercase tracking-wider"
                          >Subject</label
                        >
                        <input
                          type="text"
                          [(ngModel)]="editForm.title"
                          placeholder="Ticket Title"
                          class="w-full px-4 py-2 border border-slate-200 rounded-lg text-slate-900 font-bold text-lg focus:ring-2 focus:ring-blue-500 outline-none"
                        />
                      </div>

                      <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold text-slate-500 uppercase tracking-wider"
                          >Description</label
                        >
                        <textarea
                          [(ngModel)]="editForm.description"
                          rows="5"
                          placeholder="Provide a detailed explanation of the issue."
                          class="w-full px-4 py-3 border border-slate-200 rounded-lg text-slate-700 focus:ring-2 focus:ring-blue-500 outline-none resize-y text-sm"
                        ></textarea>
                      </div>

                      <!-- Editable Attachments -->
                      <div class="flex flex-col gap-3 mt-2">
                        <h4 class="text-sm font-bold text-slate-700">Existing Attachments</h4>
                        @if (t.attachments && t.attachments.length > 0) {
                          <div class="flex flex-wrap gap-2">
                            @for (file of t.attachments; track file.id) {
                              <div
                                class="flex items-center gap-2 bg-white border border-slate-200 px-3 py-1.5 rounded-lg shadow-sm"
                              >
                                <span class="text-xs text-slate-600 truncate max-w-[150px]">{{
                                  file.fileName
                                }}</span>
                                <button
                                  (click)="deleteAttachment(t.id, file.id)"
                                  class="text-red-500 hover:text-red-700 font-bold ml-1 cursor-pointer"
                                  title="Delete Attachment"
                                >
                                  &times;
                                </button>
                              </div>
                            }
                          </div>
                        } @else {
                          <span class="text-xs text-slate-500 italic">No attachments found.</span>
                        }

                        <!-- Add new attachments in edit mode -->
                        <h4 class="text-sm font-bold text-slate-700 mt-2">Add New Attachments</h4>
                        @if (editSelectedFiles().length > 0) {
                          <div class="flex flex-wrap gap-2 mb-2">
                            @for (file of editSelectedFiles(); track file.name) {
                              <div
                                class="flex items-center gap-1 bg-blue-50 border border-blue-200 px-2 py-1 rounded text-xs text-slate-700"
                              >
                                <span class="truncate max-w-[150px]">{{ file.name }}</span>
                                <button
                                  (click)="removeEditFile(file)"
                                  class="text-red-500 hover:text-red-700 font-bold ml-1 cursor-pointer"
                                >
                                  &times;
                                </button>
                              </div>
                            }
                          </div>
                        }
                        <div class="flex items-center gap-2">
                          <input
                            type="file"
                            multiple
                            #editFileInput
                            style="display: none;"
                            (change)="onEditFilesSelected($event)"
                          />
                          <button
                            (click)="editFileInput.click()"
                            class="px-4 py-2 bg-slate-200 text-slate-700 rounded-lg hover:bg-slate-300 text-sm font-semibold transition cursor-pointer"
                          >
                            Select Files
                          </button>
                        </div>
                      </div>

                      <div class="flex justify-end gap-2 mt-4 pt-4 border-t border-slate-200">
                        <button
                          (click)="saveTicketUpdate(t.id)"
                          [disabled]="isSavingUpdate()"
                          class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-bold transition cursor-pointer shadow-sm disabled:opacity-50"
                        >
                          {{ isSavingUpdate() ? 'Saving...' : 'Save Changes' }}
                        </button>
                      </div>
                    </div>
                  } @else {
                    <div class="flex flex-col gap-6">
                      <!-- Status Flow Stepper -->
                      <div
                        class="bg-slate-50 border border-slate-200/60 rounded-xl px-4 py-4 shadow-sm overflow-x-auto"
                      >
                        <h3 class="text-xs font-bold text-slate-500 uppercase tracking-wider mb-4">
                          Lifecycle Progress
                        </h3>
                        <div class="flex items-center gap-0 min-w-max">
                          @for (step of statusFlow; track step; let i = $index; let last = $last) {
                            <div class="flex-1 flex items-center">
                              <div class="flex flex-col items-center flex-shrink-0">
                                <div
                                  class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold border-2 transition-all"
                                  [ngClass]="{
                                    'bg-blue-600 border-blue-600 text-white': isCompletedStep(
                                      t,
                                      step
                                    ),
                                    'border-blue-600 text-blue-600 bg-blue-50 ring-2 ring-blue-100':
                                      isCurrentStep(t, step),
                                    'border-slate-300 text-slate-400 bg-white':
                                      !isCompletedStep(t, step) && !isCurrentStep(t, step),
                                  }"
                                >
                                  @if (isCompletedStep(t, step)) {
                                    <svg
                                      class="w-4 h-4"
                                      fill="none"
                                      stroke="currentColor"
                                      viewBox="0 0 24 24"
                                    >
                                      <path
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                        stroke-width="2.5"
                                        d="M5 13l4 4L19 7"
                                      />
                                    </svg>
                                  } @else {
                                    {{ i + 1 }}
                                  }
                                </div>
                                <span
                                  class="text-[9px] font-medium mt-1.5 text-center w-14 leading-tight"
                                  [ngClass]="{
                                    'text-blue-700 font-bold': isCurrentStep(t, step),
                                    'text-blue-500':
                                      isCompletedStep(t, step) && !isCurrentStep(t, step),
                                    'text-slate-400':
                                      !isCompletedStep(t, step) && !isCurrentStep(t, step),
                                  }"
                                >
                                  {{ step }}
                                </span>
                              </div>
                              @if (!last) {
                                <div
                                  class="flex-1 h-0.5 mx-1 mb-4 min-w-[24px] rounded"
                                  [ngClass]="
                                    isCompletedStep(t, step) ? 'bg-blue-500' : 'bg-slate-200'
                                  "
                                ></div>
                              }
                            </div>
                          }
                        </div>
                      </div>

                      <!-- Top Grid: Metadata -->
                      <div
                        class="grid grid-cols-2 md:grid-cols-4 gap-6 bg-slate-50 border border-slate-200/60 rounded-xl p-4 shadow-sm"
                      >
                        <div class="flex flex-col gap-1">
                          <span
                            class="text-xs font-extrabold text-slate-500 uppercase tracking-widest"
                            >Ticket ID</span
                          >
                          <span class="text-sm font-bold text-slate-800">{{
                            t.ticketNumber || t.id.split('-')[0].toUpperCase()
                          }}</span>
                        </div>
                        <div class="flex flex-col gap-1">
                          <span
                            class="text-xs font-extrabold text-slate-500 uppercase tracking-widest"
                            >Ticket Type</span
                          >
                          <span class="text-sm font-bold text-slate-800">{{
                            t.type || 'Incident'
                          }}</span>
                        </div>

                        <div class="flex flex-col gap-1">
                          <span
                            class="text-xs font-extrabold text-slate-500 uppercase tracking-widest"
                            >Priority</span
                          >
                          <span
                            class="text-sm font-bold"
                            [class]="getPriorityTextClasses(t.priority)"
                            >{{ t.priority }}</span
                          >
                        </div>
                      </div>

                      <!-- Subject -->
                      <div class="flex flex-col gap-2">
                        <span
                          class="text-xs font-extrabold text-slate-500 uppercase tracking-widest"
                          >Subject</span
                        >
                        <h2
                          class="text-2xl font-bold text-slate-900 border-b border-slate-100 pb-3"
                        >
                          {{ t.title }}
                        </h2>
                      </div>

                      <!-- Description -->
                      <div class="flex flex-col gap-2">
                        <span
                          class="text-xs font-extrabold text-slate-500 uppercase tracking-widest"
                          >Description</span
                        >
                        <div
                          class="text-sm text-slate-700 leading-relaxed bg-white border border-slate-200 rounded-xl p-5 whitespace-pre-wrap text-justify shadow-sm"
                        >
                          {{ t.description }}
                        </div>
                      </div>
                    </div>
                  }
                </div>

                <!-- Attachments -->
                @if (!isEditingTicket() && t.attachments && t.attachments.length > 0) {
                  <div class="px-6 py-4 border-b border-slate-100 flex-shrink-0 bg-slate-50">
                    <h4 class="text-base font-bold text-slate-900 mb-4">
                      Uploaded Media & Attachments
                    </h4>
                    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                      @for (file of t.attachments; track file.id) {
                        <div
                          class="flex flex-col bg-white border border-slate-200 shadow-sm rounded-xl overflow-hidden hover:border-blue-300 hover:shadow-md transition-all"
                        >
                          <!-- Media Preview Area -->
                          <div
                            class="h-40 bg-slate-100 flex items-center justify-center overflow-hidden border-b border-slate-100"
                          >
                            @if (file.mimeType.startsWith('image/')) {
                              <img
                                [src]="getFileUrl(file.fileUrl)"
                                [alt]="file.fileName"
                                class="w-full h-full object-cover cursor-pointer"
                                (click)="viewMedia(getFileUrl(file.fileUrl), 'image')"
                              />
                            } @else if (file.mimeType.startsWith('video/')) {
                              <video
                                [src]="getFileUrl(file.fileUrl)"
                                class="w-full h-full object-cover cursor-pointer"
                                (click)="viewMedia(getFileUrl(file.fileUrl), 'video')"
                              ></video>
                              <div
                                class="absolute inset-0 flex items-center justify-center pointer-events-none"
                              >
                                <div
                                  class="w-10 h-10 bg-black/50 rounded-full flex items-center justify-center backdrop-blur-sm"
                                >
                                  <span class="text-white text-xl ml-1">▶</span>
                                </div>
                              </div>
                            } @else if (
                              file.mimeType === 'message/rfc822' ||
                              file.mimeType === 'application/vnd.ms-outlook'
                            ) {
                              <a
                                [href]="getFileUrl(file.fileUrl)"
                                target="_blank"
                                class="flex flex-col items-center justify-center h-full w-full text-slate-400 hover:text-blue-500 transition-colors"
                              >
                                <span class="text-4xl mb-2">✉️</span>
                                <span class="text-xs font-semibold uppercase tracking-wider"
                                  >Email</span
                                >
                              </a>
                            } @else {
                              <a
                                [href]="getFileUrl(file.fileUrl)"
                                target="_blank"
                                class="flex flex-col items-center justify-center h-full w-full text-slate-400 hover:text-blue-500 transition-colors"
                              >
                                <span class="text-4xl mb-2">📎</span>
                                <span class="text-xs font-semibold uppercase tracking-wider"
                                  >Document</span
                                >
                              </a>
                            }
                          </div>

                          <!-- File Details Area -->
                          <div class="p-3 flex items-center justify-between gap-2">
                            <div class="flex flex-col overflow-hidden">
                              <span
                                class="text-sm font-semibold text-slate-700 truncate"
                                [title]="file.fileName"
                                >{{ file.fileName }}</span
                              >
                              <span
                                class="text-[10px] text-slate-400 uppercase tracking-wider font-medium"
                                >{{ formatBytes(file.fileSizeBytes) }}</span
                              >
                            </div>
                            <a
                              [href]="getFileUrl(file.fileUrl)"
                              target="_blank"
                              class="w-8 h-8 rounded-full bg-slate-50 flex items-center justify-center text-slate-500 hover:bg-blue-50 hover:text-blue-600 transition-colors"
                              title="Download"
                            >
                              <svg
                                xmlns="http://www.w3.org/2000/svg"
                                class="h-4 w-4"
                                fill="none"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                              >
                                <path
                                  stroke-linecap="round"
                                  stroke-linejoin="round"
                                  stroke-width="2"
                                  d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
                                />
                              </svg>
                            </a>
                          </div>
                        </div>
                      }
                    </div>
                  </div>
                }

                <!-- Conversation section label -->
                <div class="px-6 pt-4 pb-2 flex-shrink-0">
                  <span class="text-xs font-bold uppercase tracking-widest text-slate-400">
                    Conversation · {{ t.comments.length }} message{{
                      t.comments.length !== 1 ? 's' : ''
                    }}
                  </span>
                </div>

                <!-- Chat messages -->
                <div class="px-6 pb-4 space-y-4">
                  @if (t.comments.length === 0) {
                    <div class="py-10 text-center text-slate-400 text-sm italic">
                      No messages yet. Start the conversation below.
                    </div>
                  }
                  @for (comment of t.comments; track comment.id) {
                    @if (comment.contactId) {
                      <!-- Customer message — right aligned -->
                      <div class="flex flex-col items-end gap-1">
                        <span class="text-xs font-semibold text-slate-400 pr-1">
                          {{ comment.contactName || 'You' }}
                        </span>
                        <div
                          class="max-w-[68%] bg-blue-600 text-white rounded-2xl rounded-tr-sm px-4 py-2.5 shadow-sm"
                        >
                          <p class="text-sm leading-relaxed whitespace-pre-wrap">
                            {{ comment.body }}
                          </p>
                          <p class="text-right text-[11px] text-blue-200 mt-1">
                            {{ comment.createdAt | date: 'shortTime' }}
                          </p>
                        </div>
                      </div>
                    } @else {
                      <!-- Agent message — left aligned -->
                      <div class="flex flex-col items-start gap-1">
                        <span class="text-xs font-semibold text-slate-500 pl-1">
                          {{ comment.authorName || 'Support Agent' }}
                        </span>
                        <div
                          class="max-w-[68%] bg-slate-100 text-slate-800 rounded-2xl rounded-tl-sm px-4 py-2.5 shadow-sm border border-slate-200/60"
                        >
                          <p class="text-sm leading-relaxed whitespace-pre-wrap">
                            {{ comment.body }}
                          </p>
                          <p class="text-right text-[11px] text-slate-400 mt-1">
                            {{ comment.createdAt | date: 'shortTime' }}
                          </p>
                        </div>
                      </div>
                    }
                  }
                </div>
              </div>

              <!-- Message input — sticky at bottom -->
              <div
                class="flex flex-col gap-2 px-4 py-3 bg-white border-t border-slate-200 flex-shrink-0"
              >
                @if (selectedFiles().length > 0) {
                  <div class="flex flex-wrap gap-2 px-1">
                    @for (file of selectedFiles(); track file.name) {
                      <div
                        class="flex items-center gap-1 bg-slate-100 px-2 py-1 rounded text-xs text-slate-700"
                      >
                        <span class="truncate max-w-[150px]">{{ file.name }}</span>
                        <button
                          (click)="removeFile(file)"
                          class="text-red-500 hover:text-red-700 font-bold ml-1 cursor-pointer"
                        >
                          &times;
                        </button>
                      </div>
                    }
                  </div>
                }
                <div class="flex items-end gap-2">
                  <input
                    type="file"
                    multiple
                    #fileInput
                    style="display: none;"
                    (change)="onFilesSelected($event)"
                  />
                  <button
                    (click)="fileInput.click()"
                    class="h-10 w-10 rounded-xl bg-slate-100 text-slate-600 flex items-center justify-center hover:bg-slate-200 transition cursor-pointer flex-shrink-0"
                    title="Attach files"
                  >
                    📎
                  </button>
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
                      <span
                        class="animate-spin inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full"
                      ></span>
                    } @else {
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        class="w-5 h-5"
                        viewBox="0 0 24 24"
                        fill="currentColor"
                      >
                        <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
                      </svg>
                    }
                  </button>
                </div>
              </div>
            }
          </div>
        </div>
      }
    </div>

    <!-- ===================== RAISE TICKET MODAL ===================== -->
    @if (showRaiseTicket()) {
      <div
        class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-50 flex items-center justify-center pointer-events-auto p-4"
        (click)="closeRaiseTicket()"
      >
        <div
          class="bg-white w-full max-w-4xl max-h-[92vh] overflow-hidden rounded-2xl shadow-2xl border border-slate-200 p-8 relative pointer-events-auto flex flex-col"
          (click)="$event.stopPropagation()"
        >
          <button
            class="absolute top-4 right-4 text-2xl font-bold text-gray-500 hover:text-black z-10"
            (click)="closeRaiseTicket()"
          >
            ✕
          </button>
          <div
            class="overflow-y-auto flex-1 [&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]"
          >
            <app-raise-ticket (ticketCreated)="closeRaiseTicket()"></app-raise-ticket>
          </div>
        </div>
      </div>
    }

    <!-- ===================== MEDIA VIEWER MODAL ===================== -->
    @if (mediaViewer.url) {
      <div
        class="fixed inset-0 bg-black/90 z-[60] flex items-center justify-center p-4 backdrop-blur-md"
        (click)="closeMediaViewer()"
      >
        <button
          class="absolute top-6 right-6 text-white/70 hover:text-white text-4xl font-light transition-colors"
          (click)="closeMediaViewer()"
        >
          &times;
        </button>
        <div
          class="max-w-5xl max-h-[85vh] flex items-center justify-center relative"
          (click)="$event.stopPropagation()"
        >
          @if (mediaViewer.type === 'image') {
            <img
              [src]="mediaViewer.url"
              class="max-w-full max-h-[85vh] object-contain rounded-lg shadow-2xl"
            />
          } @else if (mediaViewer.type === 'video') {
            <video
              [src]="mediaViewer.url"
              controls
              autoplay
              class="max-w-full max-h-[85vh] rounded-lg shadow-2xl bg-black"
            ></video>
          }
        </div>
      </div>
    }

    <!-- ===================== CONFIRM DIALOG MODAL ===================== -->
    @if (confirmDialog().show) {
      <div
        class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[70] flex items-center justify-center p-4"
      >
        <div
          class="bg-white rounded-2xl w-full max-w-sm shadow-2xl border border-slate-100 overflow-hidden flex flex-col transform transition-all"
        >
          <div class="p-6">
            <div
              class="w-16 h-16 mx-auto rounded-full flex items-center justify-center mb-4"
              [ngClass]="{
                'bg-blue-100 text-blue-600': confirmDialog().type === 'info',
                'bg-emerald-100 text-emerald-600': confirmDialog().type === 'success',
                'bg-amber-100 text-amber-600': confirmDialog().type === 'alert',
                'bg-red-100 text-red-600': confirmDialog().type === 'critical',
              }"
            >
              <span class="text-3xl font-bold">
                {{
                  confirmDialog().type === 'info'
                    ? 'ℹ️'
                    : confirmDialog().type === 'success'
                      ? '✅'
                      : confirmDialog().type === 'alert'
                        ? '⚠️'
                        : '🚨'
                }}
              </span>
            </div>
            <h3 class="text-xl font-bold text-center text-slate-800 mb-2">
              {{ confirmDialog().title }}
            </h3>
            <p class="text-slate-500 text-center text-sm">{{ confirmDialog().message }}</p>
          </div>
          <div class="px-6 py-4 bg-slate-50 border-t border-slate-100 flex gap-3 justify-end">
            <button
              (click)="closeConfirm()"
              class="flex-1 px-4 py-2 bg-white border border-slate-200 text-slate-700 rounded-xl font-bold hover:bg-slate-50 transition cursor-pointer"
            >
              Cancel
            </button>
            <button
              (click)="executeConfirm()"
              class="flex-1 px-4 py-2 text-white rounded-xl font-bold shadow-sm transition cursor-pointer"
              [ngClass]="{
                'bg-blue-600 hover:bg-blue-700': confirmDialog().type === 'info',
                'bg-emerald-600 hover:bg-emerald-700': confirmDialog().type === 'success',
                'bg-amber-500 hover:bg-amber-600': confirmDialog().type === 'alert',
                'bg-red-600 hover:bg-red-700': confirmDialog().type === 'critical',
              }"
            >
              Confirm
            </button>
          </div>
        </div>
      </div>
    }
  `,
})
export class MyTicketsComponent implements OnInit {
  private http = inject(HttpClient);

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
  selectedFiles = signal<File[]>([]);

  isEditingTicket = signal<boolean>(false);
  isSavingUpdate = signal<boolean>(false);
  editForm = { title: '', description: '', priority: '', type: '' };
  editSelectedFiles = signal<File[]>([]);

  mediaViewer = { url: null as string | null, type: null as 'image' | 'video' | null };

  confirmDialog = signal<ConfirmDialog>({
    show: false,
    type: 'info',
    title: '',
    message: '',
    onConfirm: () => {},
  });

  openConfirm(
    type: 'info' | 'success' | 'alert' | 'critical',
    title: string,
    message: string,
    onConfirm: () => void,
  ) {
    this.confirmDialog.set({ show: true, type, title, message, onConfirm });
  }

  closeConfirm() {
    this.confirmDialog.update((d) => ({ ...d, show: false }));
  }

  executeConfirm() {
    const dialog = this.confirmDialog();
    if (dialog.onConfirm) {
      dialog.onConfirm();
    }
    this.closeConfirm();
  }

  constructor() {
    effect(
      () => {
        const status = this.selectedStatus();
        const term = this.searchTerm();
        this.fetchTicketsList(status, term);
      },
      { allowSignalWrites: true },
    );
  }

  private setBodyScroll(locked: boolean) {
    document.body.style.overflow = locked ? 'hidden' : '';
  }

  filteredTickets = computed(() => this.tickets());

  ngOnInit() {}

  fetchTicketsList(status: string = this.selectedStatus(), term: string = this.searchTerm()) {
    this.isLoadingList.set(true);
    let params = new HttpParams().set('page', '1').set('pageSize', '50');

    if (status !== 'All') {
      params = params.set('status', status);
    }
    if (term) {
      params = params.set('term', term);
    }

    this.http
      .get<{ items: TicketListItem[] }>(`${environment.apiBaseUrl}/api/tickets/my`, { params })
      .subscribe({
        next: (res) => {
          console.log(
            'LIST statuses:',
            res.items.map((i) => i.status),
          );
          this.tickets.set(res.items || []);
          this.isLoadingList.set(false);
        },
        error: (err) => {
          console.error('Error fetching tickets', err);
          this.tickets.set([]);
          this.isLoadingList.set(false);
        },
      });
  }

  openTicketDetails(id: string) {
    const listTicket = this.tickets().find((t) => t.id === id);
    const listStatus = listTicket?.status;
    this.selectedTicketId.set(id);
    this.showDetailModal.set(true);
    this.selectTicket(id, listStatus);
    this.setBodyScroll(true);
  }

  closeDetailModal() {
    this.showDetailModal.set(false);
    this.selectedTicketId.set(null);
    this.ticketDetail.set(null);
    this.selectedFiles.set([]);
    this.newCommentText = '';
    this.isEditingTicket.set(false);
    this.editSelectedFiles.set([]);
    this.setBodyScroll(false);
    this.mediaViewer = { url: null, type: null };
  }

  statusFlow = ['open', 'assigned', 'pending', 'resolved', 'closed'];

  private customerStatusMap: Record<string, string> = {
    new: 'open',
    open: 'assigned',
    in_progress: 'assigned',
    inprogress: 'assigned',
    pending_customer: 'pending',
    pendingcustomer: 'pending',
    pending_internal: 'pending',
    pendinginternal: 'pending',
    on_hold: 'pending',
    onhold: 'pending',
    resolved: 'resolved',
    closed: 'closed',
    reopened: 'open',
  };

  private statusLabels: Record<string, string> = {
    open: 'Open',
    assigned: 'Assigned',
    pending: 'Pending',
    resolved: 'Resolved',
    closed: 'Closed',
  };

  getStepLabel(step: string): string {
    return this.statusLabels[step] ?? step;
  }

  getCustomerStatusLabel(status: string): string {
    const s = this.normalizeStatus(status);
    if (['new'].includes(s)) return 'Open';
    if (['open', 'in_progress', 'inprogress'].includes(s)) return 'Assigned';
    if (
      [
        'pending_customer',
        'pendingcustomer',
        'pending_internal',
        'pendinginternal',
        'on_hold',
        'onhold',
      ].includes(s)
    )
      return 'Pending';
    if (s === 'resolved') return 'Resolved';
    if (s === 'closed') return 'Closed';
    return 'Open';
  }

  private normalizeStatus(s: string): string {
    // converts PascalCase → snake_case and lowercases
    // e.g. "InProgress" → "in_progress", "on_hold" → "on_hold"
    return s
      .replace(/([A-Z])/g, '_$1')
      .toLowerCase()
      .replace(/^_/, '');
  }

  isCurrentStep(ticket: TicketDetail, step: string): boolean {
    const mapped = this.customerStatusMap[this.normalizeStatus(ticket.status)] ?? 'open';
    return mapped === step;
  }

  isCompletedStep(ticket: TicketDetail, step: string): boolean {
    const mapped = this.customerStatusMap[this.normalizeStatus(ticket.status)] ?? 'open';
    const currentIdx = this.statusFlow.indexOf(mapped);
    const stepIdx = this.statusFlow.indexOf(step);
    return stepIdx < currentIdx;
  }

  viewMedia(url: string, type: 'image' | 'video') {
    this.mediaViewer = { url, type };
  }

  closeMediaViewer() {
    this.mediaViewer = { url: null, type: null };
  }

  openRaiseTicket() {
    this.showRaiseTicket.set(true);
    this.setBodyScroll(true);
  }

  closeRaiseTicket() {
    this.showRaiseTicket.set(false);
    this.setBodyScroll(false);
  }

  selectTicket(id: string, overrideStatus?: string) {
    this.isLoadingDetails.set(true);
    this.http.get<TicketDetail>(`${environment.apiBaseUrl}/api/tickets/${id}`).subscribe({
      next: (res) => {
        // Trust the list status if provided — detail endpoint may return stale/wrong status
        if (overrideStatus) {
          res.status = overrideStatus;
        }
        this.ticketDetail.set(res);
        this.isLoadingDetails.set(false);
        if (this.isEditingTicket()) {
          this.initEditForm(res);
        }
      },
      error: (err) => {
        console.error('Error fetching ticket details', err);
        this.isLoadingDetails.set(false);
      },
    });
  }

  initEditForm(ticket: TicketDetail) {
    this.editForm = {
      title: ticket.title,
      description: ticket.description,
      priority: ticket.priority,
      type: ticket.type || 'Incident',
    };
  }

  saveTicketUpdate(ticketId: string) {
    this.isSavingUpdate.set(true);
    this.http.put(`${environment.apiBaseUrl}/api/tickets/${ticketId}`, this.editForm).subscribe({
      next: () => {
        const files = this.editSelectedFiles();
        if (files.length > 0) {
          let uploadedCount = 0;
          let hasError = false;
          files.forEach((file) => {
            const formData = new FormData();
            formData.append('File', file);
            this.http
              .post(`${environment.apiBaseUrl}/api/tickets/${ticketId}/attachments`, formData)
              .subscribe({
                next: () => {
                  uploadedCount++;
                  if (uploadedCount === files.length && !hasError) {
                    this.finishTicketUpdate(ticketId);
                  }
                },
                error: (err) => {
                  console.error('Error uploading edit file', file.name, err);
                  hasError = true;
                  this.finishTicketUpdate(ticketId);
                },
              });
          });
        } else {
          this.finishTicketUpdate(ticketId);
        }
      },
      error: (err) => {
        console.error('Error updating ticket', err);
        this.isSavingUpdate.set(false);
      },
    });
  }

  private finishTicketUpdate(ticketId: string) {
    this.isSavingUpdate.set(false);
    this.isEditingTicket.set(false);
    this.editSelectedFiles.set([]);
    this.selectTicket(ticketId);
    this.fetchTicketsList();
  }

  deleteAttachment(ticketId: string, attachmentId: string) {
    this.openConfirm(
      'alert',
      'Delete Attachment',
      'Are you sure you want to delete this attachment?',
      () => {
        this.http
          .delete(`${environment.apiBaseUrl}/api/tickets/${ticketId}/attachments/${attachmentId}`)
          .subscribe({
            next: () => {
              this.selectTicket(ticketId);
            },
            error: (err) => console.error('Error deleting attachment', err),
          });
      },
    );
  }

  onEditFilesSelected(event: any) {
    const files: FileList = event.target.files;
    if (files && files.length > 0) {
      const current = this.editSelectedFiles();
      this.editSelectedFiles.set([...current, ...Array.from(files)]);
    }
    event.target.value = '';
  }

  removeEditFile(fileToRemove: File) {
    this.editSelectedFiles.set(this.editSelectedFiles().filter((f) => f !== fileToRemove));
  }

  closeTicket(ticketId: string) {
    this.openConfirm(
      'critical',
      'Close Ticket',
      'Are you sure you want to close this ticket?',
      () => {
        const currentUserId = '00000000-0000-0000-0000-000000000000';
        this.http
          .post(`${environment.apiBaseUrl}/api/tickets/${ticketId}/close`, {
            closedBy: currentUserId,
            notes: 'Closed by customer',
          })
          .subscribe({
            next: () => {
              this.selectTicket(ticketId);
              this.fetchTicketsList();
            },
            error: (err) => console.error('Error closing ticket', err),
          });
      },
    );
  }

  reopenTicket(ticketId: string) {
    this.openConfirm(
      'info',
      'Reopen Ticket',
      'Are you sure you want to reopen this ticket?',
      () => {
        this.http
          .post(`${environment.apiBaseUrl}/api/tickets/${ticketId}/reopen`, {
            reason: 'Reopened by customer',
          })
          .subscribe({
            next: () => {
              this.selectTicket(ticketId);
              this.fetchTicketsList();
            },
            error: (err) => console.error('Error reopening ticket', err),
          });
      },
    );
  }

  onFilesSelected(event: any) {
    const files: FileList = event.target.files;
    if (files && files.length > 0) {
      const current = this.selectedFiles();
      this.selectedFiles.set([...current, ...Array.from(files)]);
    }
    event.target.value = '';
  }

  removeFile(fileToRemove: File) {
    this.selectedFiles.set(this.selectedFiles().filter((f) => f !== fileToRemove));
  }

  postComment(ticketId: string) {
    if (!this.newCommentText.trim() && this.selectedFiles().length === 0) return;
    this.isPostingComment.set(true);

    const finishPosting = () => {
      this.newCommentText = '';
      this.selectedFiles.set([]);
      this.isPostingComment.set(false);
      this.selectTicket(ticketId);
    };

    if (this.newCommentText.trim()) {
      this.http
        .post<string>(`${environment.apiBaseUrl}/api/tickets/${ticketId}/comments`, {
          body: this.newCommentText,
        })
        .subscribe({
          next: (commentId) => {
            this.uploadFiles(ticketId, commentId, finishPosting);
          },
          error: (err) => {
            console.error('Error posting comment', err);
            this.isPostingComment.set(false);
          },
        });
    } else {
      this.uploadFiles(ticketId, null, finishPosting);
    }
  }

  private uploadFiles(ticketId: string, commentId: string | null, onComplete: () => void) {
    const files = this.selectedFiles();
    if (files.length === 0) {
      onComplete();
      return;
    }

    let uploadedCount = 0;
    let hasError = false;

    files.forEach((file) => {
      const formData = new FormData();
      formData.append('File', file);
      if (commentId) {
        formData.append('CommentId', commentId);
      }

      this.http
        .post(`${environment.apiBaseUrl}/api/tickets/${ticketId}/attachments`, formData)
        .subscribe({
          next: () => {
            uploadedCount++;
            if (uploadedCount === files.length && !hasError) {
              onComplete();
            }
          },
          error: (err) => {
            console.error('Error uploading file', file.name, err);
            hasError = true;
            this.isPostingComment.set(false);
          },
        });
    });
  }

  formatDate(dateStr: string): string {
    const diff = Math.floor((new Date().getTime() - new Date(dateStr).getTime()) / 1000);
    if (diff < 60) return 'just now';
    if (diff < 3600) return Math.floor(diff / 60) + 'm ago';
    if (diff < 86400) return Math.floor(diff / 3600) + 'h ago';
    return Math.floor(diff / 86400) + 'd ago';
  }

  getFileUrl(url: string): string {
    if (!url) return '#';
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return `${environment.apiBaseUrl}${url.startsWith('/') ? '' : '/'}${url}`;
  }

  formatBytes(bytes: number, decimals = 2): string {
    if (!+bytes) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
  }

  getStatusClasses(status: string): string {
    const label = this.getCustomerStatusLabel(status);
    if (label === 'Open') return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    if (label === 'Assigned') return 'bg-blue-50 text-blue-700 border-blue-200';
    if (label === 'Pending') return 'bg-yellow-50 text-yellow-700 border-yellow-200';
    if (label === 'Resolved') return 'bg-green-50 text-green-700 border-green-200';
    return 'bg-gray-100 text-slate-600 border-slate-200';
  }

  getPriorityClasses(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'high':
      case 'critical':
        return 'border-red-200 bg-red-50 text-red-700';
      case 'medium':
        return 'border-amber-200 bg-amber-50 text-amber-700';
      case 'low':
        return 'border-emerald-200 bg-emerald-50 text-emerald-700';
      default:
        return 'border-slate-200 bg-slate-50 text-slate-700';
    }
  }

  getPriorityTextClasses(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'high':
      case 'critical':
        return 'text-red-600';
      case 'medium':
        return 'text-amber-600';
      case 'low':
        return 'text-emerald-600';
      default:
        return 'text-slate-700';
    }
  }
}
