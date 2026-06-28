import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TicketService } from '../services/ticket.service';
import { TicketDetails, TicketComment, TicketActivity } from '../models/ticket.model';
import { AuthService } from '../../../core/auth/auth.service';
import { environment } from '../../../../environments/environment';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

interface ConfirmDialog {
  type: 'assign' | 'status';
  title: string;
  message: string;
  detail: string;
  confirmLabel: string;
  confirmClass: string;
  payload: string;
}

const STATUS_FLOW = [
  'New',
  'Open',
  'InProgress',
  'PendingCustomer',
  'PendingInternal',
  'OnHold',
  'Resolved',
  'Closed',
];

/** Human-readable labels for TicketStatus enum values */
const STATUS_LABELS: Record<string, string> = {
  New: 'Open',
  Open: 'Assigned',
  InProgress: 'In Progress',
  PendingCustomer: 'Pending Customer',
  PendingInternal: 'Pending Internal',
  OnHold: 'On Hold',
  ProductRoadmap: 'Product Roadmap',
  PendingApproval: 'Pending Approval',
  ComplianceReview: 'Compliance Review',
  DualAgentConfirm: 'Dual Agent Confirm',
  Resolved: 'Resolved',
  Reopened: 'Reopened',
  Closed: 'Closed',
};

/**
 * Valid status transitions — must match backend TicketStatus enum values exactly.
 */
const VALID_TRANSITIONS: Record<string, string[]> = {
  new: ['Open'],
  open: ['InProgress', 'PendingCustomer', 'OnHold'],
  inprogress: ['PendingCustomer', 'PendingInternal', 'OnHold', 'Resolved'],
  pendingcustomer: ['InProgress', 'Resolved'],
  pendinginternal: ['InProgress'],
  onhold: ['InProgress', 'Resolved'],
  resolved: ['Reopened'],
  closed: ['Reopened'],
  reopened: ['InProgress'],
};

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col animate-fade-in">
      <!-- Back Button -->
      <div class="flex items-center gap-3 mb-5">
        <button
          (click)="goBack()"
          class="flex items-center gap-2 text-sm text-text-muted hover:text-primary transition-colors font-medium"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 19l-7-7 7-7"
            />
          </svg>
          Back to tickets
        </button>
        @if (ticket()) {
          <span class="text-text-muted">/</span>
          <span class="text-sm font-mono text-primary/80 font-semibold">{{
            ticket()!.ticketNumber
          }}</span>
        }
      </div>

      @if (loading()) {
        <div class="flex-1 flex items-center justify-center">
          <div class="text-center">
            <div
              class="w-10 h-10 border-2 border-primary border-t-transparent rounded-full animate-spin mx-auto mb-4"
            ></div>
            <p class="text-text-muted text-sm">Loading ticket…</p>
          </div>
        </div>
      } @else if (!ticket()) {
        <div class="flex-1 flex items-center justify-center text-center">
          <div>
            <div
              class="w-16 h-16 bg-red-50 dark:bg-red-900/20 rounded-2xl flex items-center justify-center mx-auto mb-4"
            >
              <svg
                class="w-8 h-8 text-red-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="1.5"
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
            </div>
            <p class="font-semibold text-text-main">Ticket not found</p>
            <button (click)="goBack()" class="mt-3 text-sm text-primary hover:underline">
              Go back
            </button>
          </div>
        </div>
      } @else {
        <!-- 3-Column Layout -->
        <div class="flex-1 flex gap-5 overflow-hidden min-h-0">
          <!-- ===== CENTER PANEL ===== -->
          <div class="flex-1 overflow-y-auto scrollbar-hide space-y-5 pr-1">
            <!-- Title & Actions Row -->
            <div
              class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-6"
            >
              <div class="flex items-start justify-between gap-4 mb-4">
                <div class="flex-1">
                  <div class="flex flex-wrap items-center gap-2 mb-2">
                    <span
                      [class]="
                        'inline-flex px-2.5 py-1 rounded-lg text-xs font-bold ' +
                        getPriorityBadge(ticket()!.priority)
                      "
                    >
                      {{ ticket()!.priority }}
                    </span>
                    <span
                      [class]="
                        'inline-flex px-2.5 py-1 rounded-full text-xs font-semibold ' +
                        getStatusBadge(ticket()!.status)
                      "
                    >
                      {{ statusLabel(ticket()!.status) }}
                    </span>
                    <span class="text-xs text-text-muted font-mono">{{
                      ticket()!.ticketNumber
                    }}</span>
                  </div>
                  @if (editingTitle()) {
                    <input
                      [(ngModel)]="editTitle"
                      (blur)="saveTitle()"
                      (keydown.enter)="saveTitle()"
                      class="w-full text-xl font-bold bg-background border border-primary rounded-lg px-3 py-1 outline-none text-text-main"
                    />
                  } @else {
                    <h1
                      class="text-xl font-bold text-text-main leading-tight cursor-pointer hover:text-primary transition-colors"
                      (click)="startEditTitle()"
                    >
                      {{ ticket()!.title }}
                    </h1>
                  }
                </div>

                <!-- Action Buttons -->
                <div class="flex items-center gap-2 flex-shrink-0">
                  @if (canTransitionTo('resolved')) {
                    <button
                      (click)="resolveTicket()"
                      [disabled]="isActionLoading()"
                      class="px-3 py-1.5 text-xs font-semibold bg-green-600 hover:bg-green-700 text-white rounded-lg transition-colors shadow-sm disabled:opacity-50"
                    >
                      ✓ Resolve
                    </button>
                  }
                  @if (canClose()) {
                    <button
                      (click)="closeTicket()"
                      [disabled]="isActionLoading()"
                      class="px-3 py-1.5 text-xs font-semibold bg-gray-600 hover:bg-gray-700 text-white rounded-lg transition-colors shadow-sm disabled:opacity-50"
                    >
                      Close
                    </button>
                  }
                  @if (canReopen()) {
                    <button
                      (click)="reopenTicket()"
                      [disabled]="isActionLoading()"
                      class="px-3 py-1.5 text-xs font-semibold bg-orange-600 hover:bg-orange-700 text-white rounded-lg transition-colors shadow-sm disabled:opacity-50"
                    >
                      Reopen
                    </button>
                  }
                </div>
              </div>

              <!-- Description -->
              <div class="text-sm text-text-muted leading-relaxed whitespace-pre-wrap">
                {{ ticket()!.description }}
              </div>

              <!-- Tags -->
              @if ((ticket()!.tags ?? []).length > 0) {
                <div class="flex flex-wrap gap-2 mt-4">
                  @for (tag of ticket()!.tags ?? []; track tag) {
                    <span
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border"
                      [class]="
                        tag.toLowerCase() === 'sla breached'
                          ? 'bg-red-50 text-red-700 border-red-200'
                          : 'bg-primary/10 text-primary border-primary/20'
                      "
                    >
                      # {{ tag }}
                    </span>
                  }
                </div>
              }
            </div>

            <!-- Status Flow Stepper -->
            <div
              class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm px-6 py-4"
            >
              <h3 class="text-xs font-bold text-text-muted uppercase tracking-wider mb-4">
                Lifecycle Progress
              </h3>
              <div class="flex items-center gap-0">
                @for (step of statusFlow; track step; let i = $index; let last = $last) {
                  <div class="flex-1 flex items-center">
                    <div class="flex flex-col items-center flex-shrink-0">
                      <div
                        [class]="
                          'w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold border-2 transition-all ' +
                          getStepClass(step)
                        "
                      >
                        @if (isCompletedStep(step)) {
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
                        [class]="isCurrentStep(step) ? 'text-primary font-bold' : 'text-text-muted'"
                      >
                        {{ statusLabel(step) }}
                      </span>
                    </div>
                    @if (!last) {
                      <div
                        class="flex-1 h-0.5 mx-1 mb-4"
                        [class]="
                          isCompletedStep(step) ? 'bg-primary' : 'bg-gray-200 dark:bg-gray-700'
                        "
                      ></div>
                    }
                  </div>
                }
              </div>
            </div>

            <!-- ===== ATTACHMENTS ===== -->
            @if ((ticket()!.attachments ?? []).length > 0) {
              <div
                class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-5"
              >
                <h3 class="text-xs font-bold text-text-muted uppercase tracking-wider mb-4">
                  Attachments ({{ (ticket()!.attachments ?? []).length }})
                </h3>
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  @for (file of ticket()!.attachments ?? []; track file.id) {
                    <div
                      class="flex flex-col bg-background border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden hover:border-primary/40 hover:shadow-md transition-all"
                    >
                      <!-- Preview Area -->
                      <div
                        class="h-36 bg-gray-100 dark:bg-gray-800 flex items-center justify-center overflow-hidden border-b border-gray-100 dark:border-gray-700 relative"
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
                            class="w-full h-full object-cover"
                          ></video>
                          <div
                            class="absolute inset-0 flex items-center justify-center cursor-pointer"
                            (click)="viewMedia(getFileUrl(file.fileUrl), 'video')"
                          >
                            <div
                              class="w-10 h-10 bg-black/50 rounded-full flex items-center justify-center backdrop-blur-sm"
                            >
                              <span class="text-white text-xl ml-0.5">▶</span>
                            </div>
                          </div>
                        } @else if (
                          file.mimeType === 'message/rfc822' ||
                          file.mimeType === 'application/vnd.ms-outlook'
                        ) {
                          <a
                            [href]="getFileUrl(file.fileUrl)"
                            target="_blank"
                            class="flex flex-col items-center justify-center h-full w-full text-text-muted hover:text-primary transition-colors"
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
                            class="flex flex-col items-center justify-center h-full w-full text-text-muted hover:text-primary transition-colors"
                          >
                            <span class="text-4xl mb-2">📎</span>
                            <span class="text-xs font-semibold uppercase tracking-wider"
                              >Document</span
                            >
                          </a>
                        }
                      </div>

                      <!-- File Details -->
                      <div class="p-3 flex items-center justify-between gap-2">
                        <div class="flex flex-col overflow-hidden">
                          <span
                            class="text-sm font-semibold text-text-main truncate"
                            [title]="file.fileName"
                            >{{ file.fileName }}</span
                          >
                          <span
                            class="text-[10px] text-text-muted uppercase tracking-wider font-medium"
                          >
                            {{ formatBytes(file.fileSizeBytes) }}
                          </span>
                        </div>
                        <a
                          [href]="getFileUrl(file.fileUrl)"
                          target="_blank"
                          class="w-8 h-8 rounded-full bg-gray-50 dark:bg-gray-800 flex items-center justify-center text-text-muted hover:bg-primary/10 hover:text-primary transition-colors flex-shrink-0"
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

            <!-- Comments -->
            <div
              class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm"
            >
              <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-800">
                <h3 class="text-sm font-bold text-text-main">Comments ({{ comments().length }})</h3>
              </div>

              <div class="p-4 space-y-4">
                @if (loadingComments()) {
                  <div class="py-4 text-center text-text-muted text-sm animate-pulse">
                    Loading comments…
                  </div>
                } @else if (comments().length === 0) {
                  <div class="py-6 text-center text-text-muted text-sm">
                    No comments yet. Be the first to comment!
                  </div>
                } @else {
                  @for (comment of comments(); track comment.id) {
                    <div class="flex gap-3">
                      <div
                        class="flex-shrink-0 w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center text-primary font-bold text-sm"
                      >
                        {{
                          (comment.authorName || comment.contactName || 'A').charAt(0).toUpperCase()
                        }}
                      </div>
                      <div class="flex-1 bg-gray-50 dark:bg-gray-800/60 rounded-xl px-4 py-3">
                        <div class="flex items-center justify-between mb-1">
                          <span class="text-xs font-semibold text-text-main">
                            {{
                              comment.authorName ||
                                comment.contactName ||
                                comment.authorId ||
                                'Customer'
                            }}
                          </span>
                          <span class="text-[10px] text-text-muted">{{
                            formatDate(comment.createdAt)
                          }}</span>
                        </div>
                        <p class="text-sm text-text-muted leading-relaxed">{{ comment.body }}</p>
                      </div>
                    </div>
                  }
                }

                <!-- Add Comment -->
                <div class="border-t border-gray-100 dark:border-gray-800 pt-4">
                  <div class="flex gap-3">
                    <div
                      class="flex-shrink-0 w-8 h-8 rounded-full bg-primary flex items-center justify-center text-white font-bold text-sm"
                    >
                      A
                    </div>
                    <div class="flex-1">
                      <textarea
                        [(ngModel)]="newComment"
                        rows="3"
                        placeholder="Write a comment… Use @mention to notify someone"
                        class="w-full px-4 py-3 bg-background border border-gray-200 dark:border-gray-700 rounded-xl text-sm text-text-main placeholder:text-text-muted/60 focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all resize-none"
                      ></textarea>
                      <div class="flex justify-between items-center mt-2">
                        <div>
                          @if (!isCustomer()) {
                            <label
                              class="flex items-center gap-2 text-sm text-text-muted cursor-pointer"
                            >
                              <input
                                type="checkbox"
                                [(ngModel)]="isInternalComment"
                                class="rounded border-gray-300 text-primary focus:ring-primary"
                              />
                              Internal Note
                            </label>
                          }
                        </div>
                        <button
                          (click)="submitComment()"
                          [disabled]="!newComment.trim() || submittingComment()"
                          class="px-4 py-2 bg-primary hover:bg-primary-hover disabled:opacity-50 text-white text-sm font-semibold rounded-lg transition-all shadow-lg shadow-primary/20"
                        >
                          @if (submittingComment()) {
                            Sending…
                          } @else {
                            Send Comment
                          }
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Activity Timeline -->
            <div
              class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm"
            >
              <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-800">
                <h3 class="text-sm font-bold text-text-main">Activity Timeline</h3>
              </div>
              <div class="p-4">
                @if (activities().length === 0) {
                  <p class="text-sm text-text-muted text-center py-4">No activity recorded yet.</p>
                } @else {
                  <div class="relative">
                    <div
                      class="absolute left-3.5 top-0 bottom-0 w-px bg-gray-200 dark:bg-gray-700"
                    ></div>
                    <div class="space-y-4">
                      @for (activity of activities(); track activity.id) {
                        <div class="flex gap-3 relative">
                          <div
                            class="flex-shrink-0 w-7 h-7 rounded-full bg-surface border-2 border-gray-200 dark:bg-gray-700 flex items-center justify-center z-10"
                          >
                            <div class="w-2 h-2 rounded-full bg-primary"></div>
                          </div>
                          <div class="flex-1 pt-1 pb-2">
                            <p class="text-xs font-semibold text-text-main">
                              {{ activity.activityType }}
                            </p>
                            @if (activity.performedByName) {
                              <p class="text-[10px] text-text-muted mt-0.5 font-medium">
                                performed by {{ activity.performedByName }}
                              </p>
                            }
                            @if (activity.oldValue || activity.newValue) {
                              <p class="text-xs text-text-muted mt-0.5">
                                @if (activity.oldValue) {
                                  <span class="line-through">{{ activity.oldValue }}</span> →
                                }
                                @if (activity.newValue) {
                                  <span class="text-primary font-medium">{{
                                    activity.newValue
                                  }}</span>
                                }
                              </p>
                            }
                            <p class="text-[10px] text-text-muted mt-0.5">
                              {{ formatDate(activity.performedAt) }}
                            </p>
                          </div>
                        </div>
                      }
                    </div>
                  </div>
                }
              </div>
            </div>
          </div>
          <!-- END CENTER PANEL -->

          <!-- ===== RIGHT PANEL ===== -->
          <div class="w-72 flex-shrink-0 space-y-4 overflow-y-auto scrollbar-hide">
            <!-- Ticket Meta -->
            <div
              class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-5 space-y-4"
            >
              <h3 class="text-xs font-bold text-text-muted uppercase tracking-wider">
                Ticket Details
              </h3>
              <div class="space-y-3 text-sm">
                <div class="flex justify-between items-start">
                  <span class="text-text-muted">Status</span>
                  <span
                    [class]="
                      'inline-flex px-2 py-0.5 rounded-full text-xs font-semibold ' +
                      getStatusBadge(ticket()!.status)
                    "
                  >
                    {{ statusLabel(ticket()!.status) }}
                  </span>
                </div>
                <div class="flex justify-between items-start">
                  <span class="text-text-muted">Priority</span>
                  <span
                    [class]="
                      'inline-flex px-2 py-0.5 rounded-lg text-xs font-bold ' +
                      getPriorityBadge(ticket()!.priority)
                    "
                  >
                    {{ ticket()!.priority }}
                  </span>
                </div>
                <div class="flex justify-between items-start">
                  <span class="text-text-muted">Category</span>
                  <span class="font-medium text-text-main">{{ ticket()!.type }}</span>
                </div>
                @if (ticket()!.department) {
                  <div class="flex justify-between items-start">
                    <span class="text-text-muted">Department</span>
                    <span class="font-medium text-text-main">{{ ticket()!.department }}</span>
                  </div>
                }
                @if (ticket()!.region) {
                  <div class="flex justify-between items-start">
                    <span class="text-text-muted">Region</span>
                    <span class="font-medium text-text-main">{{ ticket()!.region }}</span>
                  </div>
                }
              </div>
            </div>

            <!-- People -->
            <div
              class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-5 space-y-4"
            >
              <h3 class="text-xs font-bold text-text-muted uppercase tracking-wider">People</h3>

              @if (isAssignOptionAllowed()) {
                <div class="flex items-center gap-3">
                  <div
                    class="w-9 h-9 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center text-indigo-600 font-bold text-sm"
                  >
                    A
                  </div>
                  <div class="flex-1">
                    <p class="text-xs text-text-muted mb-1 font-medium">
                      Assignee
                      @if (currentUserRole() === 'agent') {
                        <span
                          class="ml-1 text-[10px] font-semibold text-indigo-500 bg-indigo-50 dark:bg-indigo-900/30 px-1.5 py-0.5 rounded-full"
                          >Team Leads only</span
                        >
                      }
                      @if (currentUserRole() === 'team_lead') {
                        <span
                          class="ml-1 text-[10px] font-semibold text-green-600 bg-green-50 dark:bg-green-900/30 px-1.5 py-0.5 rounded-full"
                          >Group agents</span
                        >
                      }
                    </p>
                    <select
                      [ngModel]="ticket()!.assignedAgentId || ''"
                      (ngModelChange)="onAssignAgent($event)"
                      [disabled]="isActionLoading() || loadingAgents()"
                      class="w-full text-sm font-medium bg-background border border-gray-300 dark:border-gray-700 rounded-lg px-2.5 py-1.5 outline-none text-text-main focus:ring-2 focus:ring-primary focus:border-primary transition-all disabled:opacity-50"
                    >
                      <option value="" disabled selected>
                        {{ loadingAgents() ? 'Loading…' : '— Unassigned —' }}
                      </option>
                      @for (agent of agents(); track agent.id) {
                        <option [value]="agent.id">{{ getAgentName(agent.id) }}</option>
                      }
                    </select>
                  </div>
                </div>
              } @else {
                @if (ticket()!.assignedAgentId) {
                  <div class="flex items-center gap-3">
                    <div
                      class="w-9 h-9 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center text-indigo-600 font-bold text-sm"
                    >
                      A
                    </div>
                    <div>
                      <p class="text-xs text-text-muted">Assignee</p>
                      <p class="text-sm font-medium text-text-main font-semibold">
                        {{ getAgentName(ticket()!.assignedAgentId) }}
                      </p>
                    </div>
                  </div>
                } @else {
                  <div class="flex items-center gap-3">
                    <div
                      class="w-9 h-9 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-gray-400"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                        />
                      </svg>
                    </div>
                    <div>
                      <p class="text-xs text-text-muted">Assignee</p>
                      <p class="text-sm text-text-muted italic">Unassigned</p>
                    </div>
                  </div>
                }
              }

              @if (ticket()!.reporterId) {
                <div class="flex items-center gap-3">
                  <div
                    class="w-9 h-9 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center text-green-600 font-bold text-sm"
                  >
                    R
                  </div>
                  <div>
                    <p class="text-xs text-text-muted">Reporter</p>
                    <p
                      class="text-sm font-semibold text-text-main truncate max-w-[180px]"
                      [title]="ticket()!.reporterName || ticket()!.reporterId || ''"
                    >
                      {{ ticket()!.reporterName || ticket()!.reporterId || 'Customer' }}
                    </p>
                  </div>
                </div>
              }
            </div>

            <!-- Dates -->
            <div
              class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-5 space-y-3"
            >
              <h3 class="text-xs font-bold text-text-muted uppercase tracking-wider">Dates</h3>
              <div class="space-y-2.5 text-sm">
                <div class="flex justify-between">
                  <span class="text-text-muted">Created</span>
                  <span class="font-medium text-text-main text-right">{{
                    formatFullDate(ticket()!.createdAt)
                  }}</span>
                </div>
                @if (ticket()!.updatedAt) {
                  <div class="flex justify-between">
                    <span class="text-text-muted">Updated</span>
                    <span class="font-medium text-text-main text-right">{{
                      formatFullDate(ticket()!.updatedAt!)
                    }}</span>
                  </div>
                }
                @if (ticket()!.resolvedAt) {
                  <div class="flex justify-between">
                    <span class="text-text-muted">Resolved</span>
                    <span class="font-medium text-green-600 dark:text-green-400 text-right">{{
                      formatFullDate(ticket()!.resolvedAt!)
                    }}</span>
                  </div>
                }
                @if (ticket()!.closedAt) {
                  <div class="flex justify-between">
                    <span class="text-text-muted">Closed</span>
                    <span class="font-medium text-gray-500 text-right">{{
                      formatFullDate(ticket()!.closedAt!)
                    }}</span>
                  </div>
                }
              </div>
            </div>

            <!-- Quick Status Change -->
            @if (availableTransitions().length > 0) {
              <div
                class="bg-surface rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm p-5"
              >
                <h3 class="text-xs font-bold text-text-muted uppercase tracking-wider mb-3">
                  Change Status
                </h3>
                <div class="space-y-2">
                  @for (t of availableTransitions(); track t) {
                    <button
                      (click)="changeStatus(t)"
                      class="w-full text-left px-3 py-2 rounded-lg text-sm font-medium text-text-main hover:bg-primary/10 hover:text-primary border border-transparent hover:border-primary/20 transition-all"
                    >
                      → {{ statusLabel(t) }}
                    </button>
                  }
                </div>
              </div>
            }

            <!-- Toast notification -->
            @if (toastMessage()) {
              <div
                class="fixed bottom-6 right-6 bg-gray-900 text-white text-sm font-medium px-5 py-3 rounded-xl shadow-2xl animate-fade-in z-50 flex items-center gap-2"
              >
                <div
                  class="w-2.5 h-2.5 rounded-full"
                  [class.bg-emerald-400]="
                    !toastMessage()!.toLowerCase().includes('failed') &&
                    !toastMessage()!.toLowerCase().includes('error')
                  "
                  [class.bg-red-400]="
                    toastMessage()!.toLowerCase().includes('failed') ||
                    toastMessage()!.toLowerCase().includes('error')
                  "
                ></div>
                {{ toastMessage() }}
              </div>
            }
          </div>
          <!-- END RIGHT PANEL -->
        </div>
      }
    </div>

    <!-- ===== CONFIRMATION MODAL ===== -->
    @if (confirmDialog()) {
      <div
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        (click)="cancelConfirm()"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
        <div
          class="relative bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 w-full max-w-sm p-6 animate-fade-in"
          (click)="$event.stopPropagation()"
        >
          <div class="flex items-start gap-4 mb-5">
            <div
              class="flex-shrink-0 w-11 h-11 rounded-xl flex items-center justify-center"
              [class]="
                confirmDialog()!.type === 'assign'
                  ? 'bg-indigo-100 dark:bg-indigo-900/40'
                  : 'bg-blue-100 dark:bg-blue-900/40'
              "
            >
              @if (confirmDialog()!.type === 'assign') {
                <svg
                  class="w-5 h-5 text-indigo-600 dark:text-indigo-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                  />
                </svg>
              } @else {
                <svg
                  class="w-5 h-5 text-blue-600 dark:text-blue-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                  />
                </svg>
              }
            </div>
            <div class="flex-1 min-w-0">
              <h3 class="text-base font-bold text-gray-900 dark:text-white leading-snug">
                {{ confirmDialog()!.title }}
              </h3>
              <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
                {{ confirmDialog()!.message }}
              </p>
            </div>
          </div>

          <div
            class="mb-5 px-4 py-3 rounded-xl bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 flex items-center justify-between gap-3"
          >
            <span
              class="text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider"
            >
              {{ confirmDialog()!.type === 'assign' ? 'Agent' : 'New Status' }}
            </span>
            <span class="text-sm font-bold text-gray-900 dark:text-white">{{
              confirmDialog()!.detail
            }}</span>
          </div>

          <div class="flex gap-3">
            <button
              (click)="cancelConfirm()"
              class="flex-1 px-4 py-2.5 rounded-xl text-sm font-semibold border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 transition-all"
            >
              Cancel
            </button>
            <button
              (click)="executeConfirm()"
              [disabled]="isActionLoading()"
              [class]="
                'flex-1 px-4 py-2.5 rounded-xl text-sm font-bold text-white transition-all shadow-lg disabled:opacity-60 ' +
                confirmDialog()!.confirmClass
              "
            >
              @if (isActionLoading()) {
                <span class="flex items-center justify-center gap-2">
                  <svg class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                    <circle
                      class="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      stroke-width="4"
                    ></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"></path>
                  </svg>
                  Processing…
                </span>
              } @else {
                {{ confirmDialog()!.confirmLabel }}
              }
            </button>
          </div>
        </div>
      </div>
    }

    <!-- ===== MEDIA VIEWER MODAL ===== -->
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
          class="max-w-5xl max-h-[85vh] flex items-center justify-center"
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
  `,
})
export class TicketDetailComponent implements OnInit {
  ticket = signal<TicketDetails | null>(null);
  comments = signal<TicketComment[]>([]);
  activities = signal<TicketActivity[]>([]);
  loading = signal(true);
  loadingComments = signal(false);
  submittingComment = signal(false);
  toastMessage = signal<string | null>(null);
  editingTitle = signal(false);
  editTitle = '';
  isActionLoading = signal<boolean>(false);
  newComment = '';
  isInternalComment = false;

  statusFlow = STATUS_FLOW;
  agents = signal<any[]>([]);
  loadingAgents = signal<boolean>(false);
  isAssignOptionAllowed = signal<boolean>(false);
  currentUserRole = signal<string>('');
  confirmDialog = signal<ConfirmDialog | null>(null);

  // ── Media viewer ────────────────────────────────────────────────────────────
  mediaViewer: { url: string | null; type: 'image' | 'video' | null } = { url: null, type: null };

  private authService = inject(AuthService);

  availableTransitions = computed(() => {
    const t = this.ticket();
    if (!t) return [];
    const current = t.status.toLowerCase();
    const transitions = VALID_TRANSITIONS[current] ?? [];
    if (this.isCustomer()) {
      return transitions.includes('Closed')
        ? transitions
        : [...transitions, ...(current === 'resolved' ? ['Closed'] : [])];
    }
    return transitions.filter((s) => s !== 'Closed');
  });

  isCustomer = computed(() => this.currentUserRole() === 'customer');

  canClose = computed(() => {
    const t = this.ticket();
    if (!t) return false;
    return this.isCustomer() && t.status.toLowerCase() === 'resolved';
  });

  canReopen = computed(() => {
    const t = this.ticket();
    if (!t) return false;
    const s = t.status.toLowerCase();
    return !this.isCustomer() && (s === 'resolved' || s === 'closed');
  });

  constructor(
    private ticketService: TicketService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) this.loadTicket(id);
    });

    const role = this.authService.currentUser()?.role ?? '';
    const userId = this.authService.currentUser()?.id ?? '';
    this.currentUserRole.set(role);

    if (role === 'admin' || role === 'supervisor' || role === 'agent' || role === 'team_lead') {
      this.isAssignOptionAllowed.set(true);
      this.loadAgentsList(role, userId);
    }
  }

  // ── Media viewer helpers ────────────────────────────────────────────────────

  viewMedia(url: string, type: 'image' | 'video') {
    this.mediaViewer = { url, type };
  }

  closeMediaViewer() {
    this.mediaViewer = { url: null, type: null };
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

  // ── Agents list ─────────────────────────────────────────────────────────────

  private loadAgentsList(role: string, userId: string) {
    this.loadingAgents.set(true);

    if (role === 'agent' || role === 'team_lead') {
      this.ticketService.getUserGroups(userId).subscribe({
        next: (groups: any) => {
          const groupList: any[] = Array.isArray(groups)
            ? groups
            : (groups?.items ?? groups?.value ?? []);

          if (!groupList || groupList.length === 0) {
            this.agents.set([]);
            this.loadingAgents.set(false);
            return;
          }

          const groupId: string = groupList[0].id ?? groupList[0].Id;

          this.ticketService.getGroupMembers(groupId).subscribe({
            next: (res: any) => {
              const members: any[] = res?.members ?? res?.Members ?? [];

              if (role === 'agent') {
                this.agents.set(
                  members
                    .filter((m: any) => m.isLead === true || m.IsLead === true)
                    .map((m: any) => ({
                      id: m.userId ?? m.UserId,
                      firstName: m.firstName ?? m.FirstName,
                      lastName: m.lastName ?? m.LastName,
                      email: m.email ?? m.Email,
                    })),
                );
                this.loadingAgents.set(false);
              } else {
                // Team Lead assigning to agents in their group
                const assignableMembers = members.filter((m: any) => {
                  // Allow assigning to anyone in the group who is not a lead, OR themselves
                  const isLead = m.isLead === true || m.IsLead === true;
                  const memberId = m.userId ?? m.UserId;
                  return !isLead || memberId === userId;
                });

                if (assignableMembers.length === 0) {
                  this.agents.set([]);
                  this.loadingAgents.set(false);
                  return;
                }

                // Map directly to agents without fetching roles (avoids 403 for team leads)
                this.agents.set(
                  assignableMembers.map((m: any) => ({
                    id: m.userId ?? m.UserId,
                    firstName: m.firstName ?? m.FirstName,
                    lastName: m.lastName ?? m.LastName,
                    email: m.email ?? m.Email,
                  })),
                );
                this.loadingAgents.set(false);
              }
            },
            error: () => {
              this.agents.set([]);
              this.loadingAgents.set(false);
            },
          });
        },
        error: () => {
          this.agents.set([]);
          this.loadingAgents.set(false);
        },
      });
    } else {
      this.ticketService.getAgents().subscribe({
        next: (res) => {
          const allUsers: any[] = res?.items ?? res?.Items ?? [];
          const roleCalls = allUsers.map((user) =>
            this.ticketService.getUserWithRoles(user.id).pipe(
              map((details: any) => {
                const roles: string[] = (details?.roles ?? []).map((r: any) =>
                  (r.name ?? '').toLowerCase(),
                );
                const isCustomerOnly =
                  roles.length === 0 || (roles.length === 1 && roles[0] === 'customer');
                return isCustomerOnly ? null : user;
              }),
              catchError(() => of(null)),
            ),
          );
          if (roleCalls.length === 0) {
            this.agents.set([]);
            this.loadingAgents.set(false);
            return;
          }
          forkJoin(roleCalls).subscribe({
            next: (results) => {
              this.agents.set(results.filter(Boolean));
              this.loadingAgents.set(false);
            },
            error: () => {
              this.agents.set(allUsers);
              this.loadingAgents.set(false);
            },
          });
        },
        error: () => {
          this.loadingAgents.set(false);
        },
      });
    }
  }

  // ── Ticket loading ──────────────────────────────────────────────────────────

  private loadTicket(id: string) {
    this.loading.set(true);
    this.ticketService.getTicketById(id).subscribe({
      next: (t) => {
        console.log('Ticket response:', t);
        this.ticket.set(t);
        this.loading.set(false);
        this.comments.set(t.comments ?? []);
        this.activities.set(t.activities ?? []);
      },
      error: () => {
        this.ticket.set(null);
        this.loading.set(false);
      },
    });
  }

  private loadComments(id: string) {
    this.loadingComments.set(true);
    this.ticketService.getComments(id).subscribe({
      next: (c) => {
        this.comments.set(c ?? []);
        this.loadingComments.set(false);
      },
      error: () => this.loadingComments.set(false),
    });
  }

  private loadActivities(id: string) {
    this.ticketService.getActivities(id).subscribe({
      next: (a) => this.activities.set(a ?? []),
      error: () => {},
    });
  }

  // ── Comments ────────────────────────────────────────────────────────────────

  submitComment() {
    const t = this.ticket();
    if (!t || !this.newComment.trim()) return;
    this.submittingComment.set(true);
    this.ticketService
      .addComment(t.id, { body: this.newComment, isPrivate: this.isInternalComment })
      .subscribe({
        next: () => {
          this.newComment = '';
          this.isInternalComment = false;
          this.submittingComment.set(false);
          this.loadComments(t.id);
          this.showToast('Comment added!');
        },
        error: () => this.submittingComment.set(false),
      });
  }

  // ── Confirm-dialog openers ──────────────────────────────────────────────────

  changeStatus(newStatus: string) {
    if (!this.ticket()) return;
    const colors: Record<string, string> = {
      open: 'bg-blue-600 hover:bg-blue-700 shadow-blue-200',
      inprogress: 'bg-indigo-600 hover:bg-indigo-700 shadow-indigo-200',
      pendingcustomer: 'bg-yellow-500 hover:bg-yellow-600 shadow-yellow-200',
      pendinginternal: 'bg-orange-500 hover:bg-orange-600 shadow-orange-200',
      onhold: 'bg-slate-600 hover:bg-slate-700 shadow-slate-200',
      resolved: 'bg-green-600 hover:bg-green-700 shadow-green-200',
      closed: 'bg-gray-600 hover:bg-gray-700 shadow-gray-200',
      reopened: 'bg-orange-600 hover:bg-orange-700 shadow-orange-200',
    };
    this.confirmDialog.set({
      type: 'status',
      title: 'Change ticket status?',
      message: `The status will change from "${this.statusLabel(this.ticket()!.status)}" to the new status below.`,
      detail: newStatus,
      confirmLabel: `Set to ${newStatus}`,
      confirmClass: colors[newStatus.toLowerCase()] ?? 'bg-primary hover:bg-primary-hover',
      payload: newStatus,
    });
  }

  resolveTicket() {
    if (!this.ticket()) return;
    this.confirmDialog.set({
      type: 'status',
      title: 'Resolve this ticket?',
      message: 'The ticket will be marked as resolved. It can still be reopened if needed.',
      detail: 'Resolved',
      confirmLabel: 'Resolve Ticket',
      confirmClass: 'bg-green-600 hover:bg-green-700 shadow-green-200',
      payload: '__resolve__',
    });
  }

  closeTicket() {
    if (!this.ticket()) return;
    this.confirmDialog.set({
      type: 'status',
      title: 'Close this ticket?',
      message: 'The ticket will be closed. It can be reopened later if necessary.',
      detail: 'Closed',
      confirmLabel: 'Close Ticket',
      confirmClass: 'bg-gray-700 hover:bg-gray-800 shadow-gray-300',
      payload: '__close__',
    });
  }

  reopenTicket() {
    if (!this.ticket()) return;
    this.confirmDialog.set({
      type: 'status',
      title: 'Reopen this ticket?',
      message: 'The ticket will be reopened and agents can continue working on it.',
      detail: 'Reopened',
      confirmLabel: 'Reopen Ticket',
      confirmClass: 'bg-orange-600 hover:bg-orange-700 shadow-orange-200',
      payload: '__reopen__',
    });
  }

  cancelConfirm() {
    this.confirmDialog.set(null);
  }

  executeConfirm() {
    const dialog = this.confirmDialog();
    if (!dialog) return;
    if (dialog.type === 'assign') {
      this._doAssign(dialog.payload);
    } else {
      switch (dialog.payload) {
        case '__resolve__':
          this._doResolve();
          break;
        case '__close__':
          this._doClose();
          break;
        case '__reopen__':
          this._doReopen();
          break;
        default:
          this._doChangeStatus(dialog.payload);
      }
    }
  }

  // ── Private API callers ─────────────────────────────────────────────────────

  private _doChangeStatus(newStatus: string) {
    const t = this.ticket();
    if (!t) return;
    this.isActionLoading.set(true);
    this.ticketService.changeStatus(t.id, newStatus).subscribe({
      next: () => {
        this.ticket.update((tk) => (tk ? { ...tk, status: newStatus } : tk));
        this.loadActivities(t.id);
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast(`Status changed to ${newStatus}`);
      },
      error: () => {
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast('Status change failed.');
      },
    });
  }

  private _doResolve() {
    const t = this.ticket();
    if (!t) return;
    this.isActionLoading.set(true);
    this.ticketService.resolveTicket(t.id, 'Resolved via UI').subscribe({
      next: () => {
        this.ticket.update((tk) => (tk ? { ...tk, status: 'Resolved' } : tk));
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast('Ticket resolved!');
      },
      error: () => {
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast('Failed to resolve ticket.');
      },
    });
  }

  private _doClose() {
    const t = this.ticket();
    if (!t) return;
    this.isActionLoading.set(true);
    this.ticketService.closeTicket(t.id).subscribe({
      next: () => {
        this.ticket.update((tk) => (tk ? { ...tk, status: 'Closed' } : tk));
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast('Ticket closed!');
      },
      error: () => {
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast('Failed to close ticket.');
      },
    });
  }

  private _doReopen() {
    const t = this.ticket();
    if (!t) return;
    this.isActionLoading.set(true);
    this.ticketService.reopenTicket(t.id, 'Reopened via UI').subscribe({
      next: () => {
        this.ticket.update((tk) => (tk ? { ...tk, status: 'Reopened' } : tk));
        this.isActionLoading.set(false);
        this.loadActivities(t.id);
        this.confirmDialog.set(null);
        this.showToast('Ticket reopened!');
      },
      error: () => {
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast('Reopen failed — insufficient permissions.');
      },
    });
  }

  // ── Title editing ───────────────────────────────────────────────────────────

  startEditTitle() {
    this.editTitle = this.ticket()?.title ?? '';
    this.editingTitle.set(true);
  }

  saveTitle() {
    const t = this.ticket();
    if (!t || !this.editTitle.trim() || this.editTitle === t.title) {
      this.editingTitle.set(false);
      return;
    }
    this.ticketService
      .updateTicket(t.id, {
        title: this.editTitle,
        description: t.description,
        priority: t.priority,
        type: t.category,
        tags: t.tags ?? [],
      })
      .subscribe({
        next: () => {
          this.ticket.update((tk) => (tk ? { ...tk, title: this.editTitle } : tk));
          this.editingTitle.set(false);
          this.showToast('Title updated!');
        },
        error: () => this.editingTitle.set(false),
      });
  }

  // ── Assignment ──────────────────────────────────────────────────────────────

  onAssignAgent(agentId: string | null) {
    if (!agentId || !this.ticket()) return;
    const agent = this.agents().find((a) => a.id === agentId);
    const agentName = agent ? this.getAgentName(agentId) : agentId;
    this.confirmDialog.set({
      type: 'assign',
      title: 'Assign ticket?',
      message: 'This ticket will be assigned to the selected agent.',
      detail: agentName,
      confirmLabel: 'Assign',
      confirmClass: 'bg-indigo-600 hover:bg-indigo-700 shadow-indigo-200',
      payload: agentId,
    });
  }

  private _doAssign(agentId: string) {
    const t = this.ticket();
    if (!t) return;
    this.isActionLoading.set(true);
    this.ticketService.assignTicket(t.id, agentId).subscribe({
      next: () => {
        this.ticket.update((tk) => (tk ? { ...tk, assignedAgentId: agentId, status: 'Open' } : tk));
        this.loadActivities(t.id);
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast('Ticket assigned successfully!');
      },
      error: (err) => {
        this.isActionLoading.set(false);
        this.confirmDialog.set(null);
        this.showToast(err?.error?.detail ?? 'Assignment failed.');
      },
    });
  }

  // ── Stepper helpers ─────────────────────────────────────────────────────────

  canTransitionTo(status: string): boolean {
    const t = this.ticket();
    if (!t) return false;
    const current = t.status.toLowerCase();
    return (VALID_TRANSITIONS[current] ?? []).some((s) => s.toLowerCase() === status.toLowerCase());
  }

  /** Map special statuses to their visual equivalent in STATUS_FLOW */
  private effectiveStatus(status: string): string {
    const s = status?.toLowerCase();
    if (s === 'reopened') return 'Open';
    return status;
  }

  isCurrentStep(step: string): boolean {
    const effective = this.effectiveStatus(this.ticket()?.status ?? '');
    return effective?.toLowerCase() === step.toLowerCase();
  }

  isCompletedStep(step: string): boolean {
    const t = this.ticket();
    if (!t) return false;
    const effective = this.effectiveStatus(t.status);
    const currentIdx = STATUS_FLOW.findIndex((s) => s.toLowerCase() === effective.toLowerCase());
    const stepIdx = STATUS_FLOW.findIndex((s) => s.toLowerCase() === step.toLowerCase());
    return stepIdx < currentIdx;
  }

  getStepClass(step: string): string {
    if (this.isCompletedStep(step)) return 'bg-primary border-primary text-white';
    if (this.isCurrentStep(step))
      return 'border-primary text-primary bg-primary/5 ring-2 ring-primary/20';
    return 'border-gray-300 dark:border-gray-600 text-text-muted bg-surface';
  }

  // ── Utility ─────────────────────────────────────────────────────────────────

  getAgentName(agentId: string | undefined): string {
    if (!agentId) return 'Unassigned';
    const agent = this.agents().find((a) => a.id === agentId);
    if (!agent) return 'Unknown Agent';
    const name = `${agent.firstName ?? ''} ${agent.lastName ?? ''}`.trim();
    return name || agent.email || 'Unknown Agent';
  }

  private showToast(msg: string) {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3000);
  }

  goBack() {
    this.router.navigate(['/agent/tickets']);
  }

  getPriorityBadge(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'critical':
        return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400';
      case 'high':
        return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400';
      case 'medium':
        return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400';
      case 'low':
        return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400';
      default:
        return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
    }
  }

  getStatusBadge(status: string): string {
    switch (status?.toLowerCase()) {
      case 'new':
        return 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400';
      case 'open':
        return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400';
      case 'inprogress':
        return 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400';
      case 'pendingcustomer':
        return 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400';
      case 'pendinginternal':
        return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400';
      case 'onhold':
        return 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-400';
      case 'productroadmap':
        return 'bg-teal-100 text-teal-700 dark:bg-teal-900/30 dark:text-teal-400';
      case 'pendingapproval':
        return 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400';
      case 'compliancereview':
        return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400';
      case 'dualagentconfirm':
        return 'bg-pink-100 text-pink-700 dark:bg-pink-900/30 dark:text-pink-400';
      case 'resolved':
        return 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400';
      case 'closed':
        return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
      case 'reopened':
        return 'bg-pink-100 text-pink-700 dark:bg-pink-900/30 dark:text-pink-400';
      default:
        return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
    }
  }

  statusLabel(status: string): string {
    return STATUS_LABELS[status] ?? status;
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

  formatFullDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
