import { Component, input, output, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
} from '@angular/forms';
import { UiInputComponent } from '../../../shared/components/ui-input/ui-input.component';
import { TicketService } from '../services/ticket.service';
import { AuthService } from '../../../core/auth/auth.service';

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
          <div
            class="px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center bg-gray-50/50 dark:bg-gray-900/50 rounded-t-2xl"
          >
            <h2 class="text-xl font-bold text-text-main">Create New Ticket</h2>
            <button
              (click)="attemptCancel()"
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

          <div class="p-6 overflow-y-auto flex-1 scrollbar-hide" (click)="activeDropdown.set(null)">
            <form [formGroup]="ticketForm" class="space-y-6">
              <app-ui-input
                id="title"
                label="Title *"
                [control]="getControl('title')"
                placeholder="Briefly describe the issue..."
              >
              </app-ui-input>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <div
                  class="relative"
                  (click)="$event.stopPropagation(); toggleDropdown('category')"
                >
                  <app-ui-input
                    id="category"
                    label="Category"
                    [control]="getControl('category')"
                    [isDropdown]="true"
                  >
                  </app-ui-input>

                  @if (activeDropdown() === 'category') {
                    <div
                      class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1"
                    >
                      @for (opt of categories; track opt) {
                        <div
                          (click)="selectOption('category', opt)"
                          class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium"
                        >
                          {{ opt }}
                        </div>
                      }
                    </div>
                  }
                </div>

                <div
                  class="relative"
                  (click)="$event.stopPropagation(); toggleDropdown('priority')"
                >
                  <app-ui-input
                    id="priority"
                    label="Priority"
                    [control]="getControl('priority')"
                    [isDropdown]="true"
                  >
                  </app-ui-input>

                  @if (activeDropdown() === 'priority') {
                    <div
                      class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1"
                    >
                      @for (opt of priorities; track opt) {
                        <div
                          (click)="selectOption('priority', opt)"
                          class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium"
                        >
                          {{ opt }}
                        </div>
                      }
                    </div>
                  }
                </div>
              </div>

              <!-- Module selector & Assignee (rendered side-by-side or stacked) -->
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <div class="relative" (click)="$event.stopPropagation(); toggleDropdown('module')">
                  <app-ui-input
                    id="module"
                    label="Module Name"
                    [control]="getControl('module')"
                    [isDropdown]="true"
                    placeholder="Select module..."
                  >
                  </app-ui-input>

                  @if (activeDropdown() === 'module') {
                    <div
                      class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1 max-h-48 overflow-y-auto"
                    >
                      @for (opt of modules; track opt) {
                        <div
                          (click)="selectOption('module', opt)"
                          class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium"
                        >
                          {{ opt }}
                        </div>
                      }
                    </div>
                  }
                </div>

                @if (isStaff()) {
                  <div
                    class="relative"
                    (click)="$event.stopPropagation(); toggleDropdown('assignee')"
                  >
                    <app-ui-input
                      id="assigneeName"
                      label="Assignee"
                      [control]="getControl('assigneeName')"
                      [isDropdown]="true"
                      placeholder="Select assignee..."
                    >
                    </app-ui-input>

                    @if (activeDropdown() === 'assignee') {
                      <div
                        class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1 max-h-48 overflow-y-auto"
                      >
                        <div
                          (click)="selectAssignee(null, '— Unassigned —')"
                          class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-muted font-medium"
                        >
                          — Unassigned —
                        </div>
                        @for (agent of agents(); track agent.id) {
                          <div
                            (click)="
                              selectAssignee(
                                agent.id,
                                (agent.firstName || '') + ' ' + (agent.lastName || '')
                              )
                            "
                            class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium"
                          >
                            {{ agent.firstName }} {{ agent.lastName }}
                          </div>
                        }
                      </div>
                    }
                  </div>
                }
              </div>

              <app-ui-input
                id="tags"
                label="Tags (comma-separated)"
                [control]="getControl('tags')"
                placeholder="auth, mobile, backend..."
              >
              </app-ui-input>

              <div class="space-y-1 w-full">
                <label
                  for="description"
                  class="block text-xl font-medium text-text-main transition-colors"
                  >Description *</label
                >
                <div class="relative transition-all">
                  <textarea
                    id="description"
                    formControlName="description"
                    rows="4"
                    placeholder="Steps to reproduce, context, expected vs actual..."
                    class="w-full px-5 py-3 text-lg bg-surface text-text-main shadow-lg border border-gray-300 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all placeholder:text-text-muted/50 resize-y"
                    [class.border-red-500]="
                      ticketForm.get('description')?.touched &&
                      ticketForm.get('description')?.invalid
                    "
                  ></textarea>
                </div>
                @if (
                  ticketForm.get('description')?.touched && ticketForm.get('description')?.invalid
                ) {
                  <div class="pt-1">
                    <span class="text-xs font-medium text-red-500 dark:text-red-400"
                      >Description is required</span
                    >
                  </div>
                }
              </div>
            </form>
          </div>

          <div
            class="px-6 py-4 border-t border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-900/50 rounded-b-2xl"
          >
            @if (submitError()) {
              <p class="text-xs text-red-600 dark:text-red-400 mb-3">{{ submitError() }}</p>
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
                    <circle
                      class="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      stroke-width="4"
                    ></circle>
                    <path
                      class="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                    ></path>
                  </svg>
                  Creating…
                } @else {
                  Create Ticket
                }
              </button>
            </div>
          </div>

          @if (showCancelConfirm()) {
            <div
              class="absolute inset-0 z-60 bg-surface/90 backdrop-blur-sm rounded-2xl flex items-center justify-center p-6"
            >
              <div
                class="bg-background border border-gray-200 dark:border-gray-700 p-6 rounded-xl shadow-2xl max-w-sm w-full text-center animate-fade-in"
              >
                <div
                  class="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400 flex items-center justify-center mx-auto mb-4"
                >
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                    ></path>
                  </svg>
                </div>
                <h3 class="text-lg font-bold text-text-main mb-2">Discard details?</h3>
                <p class="text-sm text-text-muted mb-6">
                  Are you sure you want to discard the details? All entered information will be
                  lost.
                </p>
                <div class="flex justify-center gap-3">
                  <button
                    (click)="showCancelConfirm.set(false)"
                    class="px-4 py-2 text-sm font-medium border border-gray-300 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                  >
                    No, keep editing
                  </button>
                  <button
                    (click)="confirmCancel()"
                    class="px-4 py-2 text-sm font-medium bg-red-600 hover:bg-red-700 text-white rounded-lg shadow-md shadow-red-500/20 transition-colors"
                  >
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

  ticketForm: FormGroup;

  categories = ['Bug', 'FeatureRequest', 'Support', 'ChangeRequest'];
  // 'Critical' maps to backend TicketPriority.Critical
  priorities = ['Critical', 'High', 'Medium', 'Low'];
  modules = [
    'Authentication',
    'Mobile App',
    'Backend Core',
    'UI/UX',
    'Payroll',
    'Leave Management',
    'HR Core',
  ];
  agents = signal<any[]>([]);
  isStaff = signal<boolean>(false);

  private authService = inject(AuthService);

  constructor(
    private fb: FormBuilder,
    private ticketService: TicketService,
  ) {
    this.ticketForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      category: ['Bug', Validators.required],
      priority: ['Medium', Validators.required],
      module: [''],
      tags: [''],
      description: ['', [Validators.required, Validators.maxLength(5000)]],
      assigneeId: [null],
      assigneeName: [''],
    });
  }

  ngOnInit() {
    const role = this.authService.currentUser()?.role;
    if (role && role !== 'customer') {
      this.isStaff.set(true);
      this.ticketService.getAgents().subscribe({
        next: (res) => {
          if (res && res.items) {
            this.agents.set(res.items);
          }
        },
        error: (err) => {
          console.error('Failed to load agents', err);
        },
      });
    }
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

  attemptCancel() {
    if (this.ticketForm.dirty) {
      this.showCancelConfirm.set(true);
    } else {
      this.confirmCancel();
    }
  }

  confirmCancel() {
    this.ticketForm.reset({
      category: 'Bug',
      priority: 'Medium',
      assigneeId: null,
      assigneeName: '',
    });
    this.showCancelConfirm.set(false);
    this.activeDropdown.set(null);
    this.submitError.set(null);
    this.closeModal.emit();
  }

  submitTicket() {
    if (this.ticketForm.invalid || this.submitting()) return;
    const v = this.ticketForm.value;
    const rawTags: string = v.tags ?? '';
    const tags = rawTags
      .split(',')
      .map((t: string) => t.trim())
      .filter((t: string) => t.length > 0);

    this.submitting.set(true);
    this.submitError.set(null);

    // ActorId & IsCustomer are injected server-side from the JWT — do NOT send them
    this.ticketService
      .createTicket({
        title: v.title,
        description: v.description ?? '',
        priority: v.priority, // Must match TicketPriority enum: Critical|High|Medium|Low
        type: v.category, // Must match TicketType enum: Bug|FeatureRequest|Support|ChangeRequest
        tags,
        assigneeId: v.assigneeId || null,
        moduleName: v.module || null,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.ticketCreated.emit();
          this.confirmCancel();
        },
        error: (err) => {
          this.submitting.set(false);
          this.submitError.set(err?.error?.detail ?? 'Failed to create ticket. Please try again.');
        },
      });
  }
}
