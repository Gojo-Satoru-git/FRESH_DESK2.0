import { Component, input, output, signal, inject, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
} from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { UiInputComponent } from '../../../shared/components/ui-input/ui-input.component';
import { TicketService } from '../services/ticket.service';
import { AuthService } from '../../../core/auth/auth.service';
import { environment } from '../../../../environments/environment.development';

@Component({
  selector: 'app-create-ticket-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, UiInputComponent],
  template: `
    @if (isOpen()) {
      <div class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 animate-fade-in">
        <div class="absolute inset-0 bg-black/50 backdrop-blur-sm" (click)="attemptCancel()"></div>

        <div
          class="relative bg-surface w-full max-w-2xl rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-800 flex flex-col max-h-[90vh]"
        >
          <!-- Header -->
          <div
            class="px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center bg-gray-50/50 dark:bg-gray-900/50 rounded-t-2xl"
          >
            <h2 class="text-xl font-bold text-text-main">Create New Ticket</h2>
            <button
              (click)="attemptCancel()"
              class="text-gray-400 hover:text-text-main hover:bg-gray-200 dark:hover:bg-gray-800 p-2 rounded-lg transition-colors"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </button>
          </div>

          <!-- Body -->
          <div class="p-6 overflow-y-auto flex-1 scrollbar-hide" (click)="activeDropdown.set(null)">
            <form [formGroup]="ticketForm" class="space-y-6">

              <!-- Title -->
              <app-ui-input
                id="title"
                label="Title *"
                [control]="getControl('title')"
                placeholder="Briefly describe the issue..."
              ></app-ui-input>

              <!-- Category & Priority -->
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <div class="relative" (click)="$event.stopPropagation(); toggleDropdown('category')">
                  <app-ui-input
                    id="category"
                    label="Category"
                    [control]="getControl('category')"
                    [isDropdown]="true"
                  ></app-ui-input>
                  @if (activeDropdown() === 'category') {
                    <div class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1">
                      @for (opt of categories; track opt) {
                        <div (click)="$event.stopPropagation(); selectOption('category', opt)" class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium">
                          {{ opt }}
                        </div>
                      }
                    </div>
                  }
                </div>

                <div class="relative" (click)="$event.stopPropagation(); toggleDropdown('priority')">
                  <app-ui-input
                    id="priority"
                    label="Priority"
                    [control]="getControl('priority')"
                    [isDropdown]="true"
                  ></app-ui-input>
                  @if (activeDropdown() === 'priority') {
                    <div class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1">
                      @for (opt of priorities; track opt) {
                        <div (click)="$event.stopPropagation(); selectOption('priority', opt)" class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium">
                          {{ opt }}
                        </div>
                      }
                    </div>
                  }
                </div>
              </div>

              <!-- Assignee — Team Lead only -->
              @if (isTeamLead()) {
                <div class="relative" (click)="$event.stopPropagation(); toggleDropdown('assignee')">
                  <app-ui-input
                    id="assigneeName"
                    label="Assignee"
                    [control]="getControl('assigneeName')"
                    [isDropdown]="true"
                    placeholder="{{ loadingAgents() ? 'Loading agents…' : 'Select assignee...' }}"
                  ></app-ui-input>
                  @if (activeDropdown() === 'assignee') {
                    <div class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1 max-h-48 overflow-y-auto">
                      <div (click)="$event.stopPropagation(); selectAssignee(null, '— Unassigned —')" class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-muted font-medium">
                        — Unassigned —
                      </div>
                      @for (agent of agents(); track agent.id) {
                        <div (click)="$event.stopPropagation(); selectAssignee(agent.id, (agent.firstName || '') + ' ' + (agent.lastName || ''))" class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium">
                          {{ agent.firstName }} {{ agent.lastName }}
                        </div>
                      }
                      @if (!loadingAgents() && agents().length === 0) {
                        <div class="px-5 py-3 text-sm text-text-muted italic">No agents in your group</div>
                      }
                    </div>
                  }
                </div>
              }

              <!-- Description with word counter -->
              <div class="space-y-1 w-full">
                <label for="description" class="block text-xl font-medium text-text-main transition-colors">
                  Description *
                </label>
                <div class="relative transition-all">
                  <textarea
                    id="description"
                    formControlName="description"
                    rows="4"
                    placeholder="Steps to reproduce, context, expected vs actual..."
                    class="w-full px-5 py-3 text-lg bg-surface text-text-main shadow-lg border border-gray-300 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all placeholder:text-text-muted/50 resize-y"
                    [class.border-red-500]="ticketForm.get('description')?.touched && ticketForm.get('description')?.invalid"
                    (input)="onDescriptionInput($event)"
                  ></textarea>
                </div>
                <div class="flex justify-between items-center pt-1">
                  @if (ticketForm.get('description')?.touched && ticketForm.get('description')?.invalid) {
                    <span class="text-xs font-medium text-red-500 dark:text-red-400">Description is required</span>
                  } @else {
                    <span></span>
                  }
                  <span class="text-xs text-text-muted">{{ descriptionWordCount() }} / {{ MAX_WORDS }} words</span>
                </div>
              </div>

              <!-- File Attachments -->
              <div class="space-y-2">
                <label class="block text-xl font-medium text-text-main">Attachments</label>
                <div
                  class="w-full p-5 border-2 border-dashed rounded-xl flex flex-col items-center justify-center gap-2 bg-background hover:bg-gray-50 dark:hover:bg-gray-900 cursor-pointer transition-colors"
                  [class.border-primary]="isDragOver"
                  [class.bg-primary/5]="isDragOver"
                  [class.border-gray-300]="!isDragOver"
                  [class.dark:border-gray-700]="!isDragOver"
                  (click)="fileInput.click()"
                  (dragover)="$event.preventDefault(); isDragOver = true"
                  (dragleave)="isDragOver = false"
                  (drop)="onFileDrop($event)"
                >
                  <input
                    type="file"
                    multiple
                    hidden
                    #fileInput
                    accept=".pdf,.png,.jpg,.jpeg,.docx,.doc,.xlsx,.csv,.txt,.mp4,.eml,.msg"
                    (change)="onFileSelect($event)"
                  />
                  <svg class="w-8 h-8 text-text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"></path>
                  </svg>
                  <p class="text-sm text-text-muted">
                    Drag & drop files here or
                    <span class="text-primary font-semibold">browse</span>
                  </p>
                  <p class="text-xs text-text-muted/60">PDF, PNG, JPG, DOCX, XLSX, MP4, EML, MSG · Max 50 MB each</p>
                </div>

                @if (attachedFiles.length > 0) {
                  <ul class="space-y-1.5 mt-2">
                    @for (file of attachedFiles; track file.name; let i = $index) {
                      <li class="flex justify-between items-center bg-gray-100 dark:bg-gray-800 px-4 py-2 rounded-lg">
                        <div class="flex items-center gap-2 min-w-0">
                          <svg class="w-4 h-4 flex-shrink-0 text-text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13"></path>
                          </svg>
                          <span class="text-sm text-text-main truncate">{{ file.name }}</span>
                        </div>
                        <button type="button" (click)="removeFile(i); $event.stopPropagation()" class="ml-3 flex-shrink-0 text-red-500 hover:text-red-700 transition-colors font-bold text-xs">✕</button>
                      </li>
                    }
                  </ul>
                }
              </div>

            </form>
          </div>

          <!-- Footer -->
          <div class="px-6 py-4 border-t border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-900/50 rounded-b-2xl">
            @if (submitError()) {
              <p class="text-xs text-red-600 dark:text-red-400 mb-3">{{ submitError() }}</p>
            }
            @if (toastMessage()) {
              <p class="text-xs text-emerald-600 dark:text-emerald-400 mb-3">{{ toastMessage() }}</p>
            }
            <div class="flex justify-end gap-3">
              <button
                (click)="attemptCancel()"
                [disabled]="submitting()"
                class="px-5 py-2.5 text-sm font-semibold text-text-muted hover:text-text-main hover:bg-gray-200 dark:hover:bg-gray-700 rounded-xl transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                (click)="submitTicket()"
                [disabled]="ticketForm.invalid || submitting()"
                class="px-5 py-2.5 text-sm font-semibold bg-primary hover:bg-primary-hover disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-xl shadow-lg shadow-primary/30 transition-all flex items-center gap-2"
              >
                @if (submitting()) {
                  <svg class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                  </svg>
                  {{ uploadProgress() ? 'Uploading files…' : 'Creating…' }}
                } @else {
                  Create Ticket
                }
              </button>
            </div>
          </div>

          <!-- Cancel Confirm Overlay -->
          @if (showCancelConfirm()) {
            <div class="absolute inset-0 z-60 bg-surface/90 backdrop-blur-sm rounded-2xl flex items-center justify-center p-6">
              <div class="bg-background border border-gray-200 dark:border-gray-700 p-6 rounded-xl shadow-2xl max-w-sm w-full text-center animate-fade-in">
                <div class="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400 flex items-center justify-center mx-auto mb-4">
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
                  </svg>
                </div>
                <h3 class="text-lg font-bold text-text-main mb-2">Discard details?</h3>
                <p class="text-sm text-text-muted mb-6">Are you sure you want to discard the details? All entered information will be lost.</p>
                <div class="flex justify-center gap-3">
                  <button (click)="showCancelConfirm.set(false)" class="px-4 py-2 text-sm font-medium border border-gray-300 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                    No, keep editing
                  </button>
                  <button (click)="confirmCancel()" class="px-4 py-2 text-sm font-medium bg-red-600 hover:bg-red-700 text-white rounded-lg shadow-md shadow-red-500/20 transition-colors">
                    Yes, discard
                  </button>
                </div>
              </div>
            </div>
          }
        </div>
      </div>
    }
  `,
})
export class CreateTicketModalComponent implements OnInit {
  isOpen = input.required<boolean>();
  closeModal = output<void>();
  ticketCreated = output<void>();

  showCancelConfirm = signal<boolean>(false);
  activeDropdown = signal<string | null>(null);
  submitting = signal<boolean>(false);
  submitError = signal<string | null>(null);
  toastMessage = signal<string | null>(null);
  uploadProgress = signal<boolean>(false);

  // Word counter
  readonly MAX_WORDS = 300;
  descriptionWordCount = signal(0);

  // File attachments
  attachedFiles: File[] = [];
  isDragOver = false;
  private readonly MAX_FILE_SIZE = 50 * 1024 * 1024; // 50 MB

  ticketForm: FormGroup;

  categories = ['Bug', 'Enhancement', 'Feature Requests', 'Service Requests', 'Customization', 'Incident', 'Environment Issues', 'Change Request', 'New Features'];
  priorities = ['Critical', 'High', 'Medium', 'Low'];

  agents = signal<any[]>([]);
  loadingAgents = signal<boolean>(false);
  // True only when the current user's role is 'team_lead'
  isTeamLead = signal<boolean>(false);

  private authService = inject(AuthService);
  private http = inject(HttpClient);

  constructor(
    private fb: FormBuilder,
    private ticketService: TicketService,
  ) {
    this.ticketForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      category: ['Bug', Validators.required],
      priority: ['Medium', Validators.required],
      description: ['', [Validators.required, Validators.maxLength(5000)]],
      assigneeId: [null],
      assigneeName: [''],
    });
  }

  ngOnInit() {
    const user = this.authService.currentUser();
    const role = user?.role ?? '';
    const userId = user?.id ?? '';

    if (role === 'team_lead') {
      this.isTeamLead.set(true);
      this.loadGroupAgents(userId);
    }
  }

  /**
   * Mirrors the team_lead branch of loadAgentsList() in ticket-details:
   * 1. Fetch the caller's groups → take the first group
   * 2. Fetch that group's members
   * 3. Keep non-leads (excluding self) whose roles include *_agent
   */
  private loadGroupAgents(userId: string): void {
    this.loadingAgents.set(true);

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

            const nonLeads = members.filter((m: any) => {
              const isLead = m.isLead === true || m.IsLead === true;
              const memberId = m.userId ?? m.UserId;
              return !isLead && memberId !== userId;
            });

            if (nonLeads.length === 0) {
              this.agents.set([]);
              this.loadingAgents.set(false);
              return;
            }

            // Filter to members who actually have an agent role
            const roleCalls = nonLeads.map((m: any) =>
              this.ticketService.getUserWithRoles(m.userId ?? m.UserId).pipe(
                map((details: any) => {
                  const roles: string[] = (details?.roles ?? []).map((r: any) =>
                    (r.name ?? '').toLowerCase(),
                  );
                  const isAgent = roles.some((r) => r === 'agent' || r.endsWith('_agent'));
                  return isAgent
                    ? {
                        id: m.userId ?? m.UserId,
                        firstName: m.firstName ?? m.FirstName,
                        lastName: m.lastName ?? m.LastName,
                        email: m.email ?? m.Email,
                      }
                    : null;
                }),
                catchError(() => of(null)),
              ),
            );

            forkJoin(roleCalls).subscribe({
              next: (results) => {
                this.agents.set(results.filter(Boolean) as any[]);
                this.loadingAgents.set(false);
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
      },
      error: () => {
        this.agents.set([]);
        this.loadingAgents.set(false);
      },
    });
  }

  getControl(controlName: string): FormControl {
    return this.ticketForm.get(controlName) as FormControl;
  }

  toggleDropdown(dropdownName: string) {
    this.activeDropdown.set(this.activeDropdown() === dropdownName ? null : dropdownName);
  }

  selectOption(controlName: string, value: string) {
    this.ticketForm.get(controlName)?.setValue(value);
    this.activeDropdown.set(null);
  }

  selectAssignee(id: string | null, name: string) {
    this.ticketForm.get('assigneeId')?.setValue(id);
    this.ticketForm.get('assigneeName')?.setValue(id ? name : '');
    this.activeDropdown.set(null);
  }

  // ── Word counter ────────────────────────────────────────────────────────

  onDescriptionInput(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    const words = textarea.value
      .trim()
      .split(/\s+/)
      .filter((w) => w.length > 0);

    if (words.length > this.MAX_WORDS) {
      textarea.value = words.slice(0, this.MAX_WORDS).join(' ');
      this.ticketForm.get('description')?.setValue(textarea.value);
      this.descriptionWordCount.set(this.MAX_WORDS);
      return;
    }
    this.descriptionWordCount.set(words.length);
  }

  // ── File attachments ────────────────────────────────────────────────────

  onFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;
    for (const file of Array.from(input.files)) {
      if (file.size > this.MAX_FILE_SIZE) {
        this.submitError.set(`"${file.name}" exceeds the 50 MB limit.`);
        setTimeout(() => this.submitError.set(null), 3000);
        continue;
      }
      this.attachedFiles.push(file);
    }
  }

  onFileDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
    if (event.dataTransfer?.files) {
      for (const file of Array.from(event.dataTransfer.files)) {
        if (file.size > this.MAX_FILE_SIZE) {
          this.submitError.set(`"${file.name}" exceeds the 50 MB limit.`);
          setTimeout(() => this.submitError.set(null), 3000);
          continue;
        }
        this.attachedFiles.push(file);
      }
    }
  }

  removeFile(index: number) {
    this.attachedFiles.splice(index, 1);
  }

  // ── Cancel flow ─────────────────────────────────────────────────────────

  attemptCancel() {
    if (this.ticketForm.dirty || this.attachedFiles.length > 0) {
      this.showCancelConfirm.set(true);
    } else {
      this.confirmCancel();
    }
  }

  confirmCancel() {
    this.ticketForm.reset({ category: 'Bug', priority: 'Medium', assigneeId: null, assigneeName: '' });
    this.attachedFiles = [];
    this.descriptionWordCount.set(0);
    this.showCancelConfirm.set(false);
    this.activeDropdown.set(null);
    this.submitError.set(null);
    this.toastMessage.set(null);
    this.closeModal.emit();
  }

  // ── Submit ──────────────────────────────────────────────────────────────

  submitTicket() {
    if (this.ticketForm.invalid || this.submitting()) return;

    const v = this.ticketForm.value;

    this.submitting.set(true);
    this.submitError.set(null);

    this.ticketService
      .createTicket({
        title: v.title,
        description: v.description ?? '',
        priority: v.priority,
        type: v.category,
        assigneeId: v.assigneeId || null,
        moduleName: null,
      })
      .subscribe({
        next: (res: any) => {
          const ticketId = res.TicketId ?? res.ticketId;
          if (this.attachedFiles.length > 0) {
            this.uploadFiles(ticketId);
          } else {
            this.onSuccess();
          }
        },
        error: (err) => {
          this.submitting.set(false);
          this.submitError.set(err?.error?.detail ?? 'Failed to create ticket. Please try again.');
        },
      });
  }

  private uploadFiles(ticketId: string): void {
    this.uploadProgress.set(true);
    let completed = 0;
    const total = this.attachedFiles.length;

    this.attachedFiles.forEach((file) => {
      const formData = new FormData();
      formData.append('File', file);

      this.http
        .post(`${environment.apiUrl}/api/tickets/${ticketId}/attachments`, formData)
        .subscribe({
          next: () => {
            completed++;
            if (completed === total) this.onSuccess();
          },
          error: () => {
            completed++;
            if (completed === total) this.onSuccess(); // proceed even if some uploads fail
          },
        });
    });
  }

  private onSuccess(): void {
    this.submitting.set(false);
    this.uploadProgress.set(false);
    this.toastMessage.set('Ticket created successfully!');
    this.ticketCreated.emit();
    setTimeout(() => {
      this.toastMessage.set(null);
      this.confirmCancel();
    }, 1500);
  }
}