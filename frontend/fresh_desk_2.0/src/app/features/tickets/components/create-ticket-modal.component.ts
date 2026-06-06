import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
// Ensure this path correctly points to your shared component
import { UiInputComponent } from '../../../shared/components/ui-input/ui-input.component';

@Component({
  selector: 'app-create-ticket-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, UiInputComponent],
  template: `
    @if (isOpen()) {
      <div class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 animate-fade-in">
        
        <div class="absolute inset-0 bg-black/50 backdrop-blur-sm" (click)="attemptCancel()"></div>

        <div class="relative bg-surface w-full max-w-2xl rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-800 flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center bg-gray-50/50 dark:bg-gray-900/50 rounded-t-2xl">
            <h2 class="text-xl font-bold text-text-main">Create New Ticket</h2>
            <button (click)="attemptCancel()" class="text-gray-400 hover:text-text-main hover:bg-gray-200 dark:hover:bg-gray-800 p-2 rounded-lg transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </button>
          </div>

          <div class="p-6 overflow-y-auto flex-1 scrollbar-hide" (click)="activeDropdown.set(null)">
            <form [formGroup]="ticketForm" class="space-y-6">
              
              <app-ui-input 
                id="title" 
                label="Title *" 
                [control]="getControl('title')" 
                placeholder="Briefly describe the issue...">
              </app-ui-input>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <div class="relative" (click)="$event.stopPropagation(); toggleDropdown('category')">
                  <app-ui-input 
                    id="category" 
                    label="Category" 
                    [control]="getControl('category')" 
                    [isDropdown]="true">
                  </app-ui-input>
                  
                  @if (activeDropdown() === 'category') {
                    <div class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1">
                      @for (opt of categories; track opt) {
                        <div (click)="selectOption('category', opt)" class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium">
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
                    [isDropdown]="true">
                  </app-ui-input>
                  
                  @if (activeDropdown() === 'priority') {
                    <div class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1">
                      @for (opt of priorities; track opt) {
                        <div (click)="selectOption('priority', opt)" class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium">
                          {{ opt }}
                        </div>
                      }
                    </div>
                  }
                </div>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <div class="relative" (click)="$event.stopPropagation(); toggleDropdown('module')">
                  <app-ui-input 
                    id="module" 
                    label="Module Name" 
                    [control]="getControl('module')" 
                    [isDropdown]="true"
                    placeholder="Select module...">
                  </app-ui-input>
                  
                  @if (activeDropdown() === 'module') {
                    <div class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1 max-h-48 overflow-y-auto">
                      @for (opt of modules; track opt) {
                        <div (click)="selectOption('module', opt)" class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium">
                          {{ opt }}
                        </div>
                      }
                    </div>
                  }
                </div>

                <div class="relative" (click)="$event.stopPropagation(); toggleDropdown('assignee')">
                  <app-ui-input 
                    id="assignee" 
                    label="Assignee" 
                    [control]="getControl('assignee')" 
                    [isDropdown]="true">
                  </app-ui-input>
                  
                  @if (activeDropdown() === 'assignee') {
                    <div class="absolute z-20 w-full mt-2 bg-background border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden animate-fade-in py-1 max-h-48 overflow-y-auto">
                      @for (opt of assignees; track opt) {
                        <div (click)="selectOption('assignee', opt)" class="px-5 py-3 text-sm hover:bg-primary/10 hover:text-primary cursor-pointer transition-colors text-text-main font-medium">
                          {{ opt }}
                        </div>
                      }
                    </div>
                  }
                </div>
              </div>

              <app-ui-input 
                id="tags" 
                label="Tags (comma-separated)" 
                [control]="getControl('tags')" 
                placeholder="auth, mobile, backend...">
              </app-ui-input>

              <div class="space-y-1 w-full">
                <label for="description" class="block text-xl font-medium text-text-main transition-colors">Description</label>
                <textarea
                  id="description"
                  formControlName="description"
                  rows="4"
                  placeholder="Steps to reproduce, context, expected vs actual..."
                  class="w-full px-5 py-3 text-lg bg-surface text-text-main shadow-lg border border-gray-300 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all placeholder:text-text-muted/50 resize-y"
                ></textarea>
              </div>

            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex justify-end gap-3 bg-gray-50/50 dark:bg-gray-900/50 rounded-b-2xl">
            <button (click)="attemptCancel()" class="px-5 py-2.5 text-sm font-semibold text-text-muted hover:text-text-main hover:bg-gray-200 dark:hover:bg-gray-700 rounded-xl transition-colors">
              Cancel
            </button>
            <button (click)="submitTicket()" [disabled]="ticketForm.invalid" class="px-5 py-2.5 text-sm font-semibold bg-primary hover:bg-primary-hover disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-xl shadow-lg shadow-primary/30 transition-all">
              Create Ticket
            </button>
          </div>

          @if (showCancelConfirm()) {
            <div class="absolute inset-0 z-60 bg-surface/90 backdrop-blur-sm rounded-2xl flex items-center justify-center p-6">
              <div class="bg-background border border-gray-200 dark:border-gray-700 p-6 rounded-xl shadow-2xl max-w-sm w-full text-center animate-fade-in">
                <div class="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400 flex items-center justify-center mx-auto mb-4">
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
                  </svg>
                </div>
                <h3 class="text-lg font-bold text-text-main mb-2">Discard details?</h3>
                <p class="text-sm text-text-muted mb-6">
                  Are you sure you want to discard the details? All entered information will be lost.
                </p>
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
  `
})
export class CreateTicketModalComponent {
  isOpen = input.required<boolean>();
  closeModal = output<void>();

  showCancelConfirm = signal<boolean>(false);
  activeDropdown = signal<string | null>(null);
  
  ticketForm: FormGroup;

  // Dropdown Data
  categories = ['Bug', 'Feature Request', 'Support', 'Billing'];
  priorities = ['Critical', 'High', 'Medium', 'Low'];
  modules = ['Authentication', 'Mobile App', 'Backend Core', 'UI/UX'];
  assignees = ['— Unassigned —', 'Rakesh Mondal (You)', 'Sarah Jenkins', 'Michael Chen'];

  constructor(private fb: FormBuilder) {
    this.ticketForm = this.fb.group({
      title: ['', Validators.required],
      category: ['Bug'],
      priority: ['Medium'],
      module: [''],
      assignee: ['Rakesh Mondal (You)'], 
      tags: [''],
      description: [''],
    });
  }

  // Helper method to safely cast to FormControl for the child component
  getControl(controlName: string): FormControl {
    return this.ticketForm.get(controlName) as FormControl;
  }

  toggleDropdown(dropdownName: string) {
    this.activeDropdown.set(this.activeDropdown() === dropdownName ? null : dropdownName);
  }

  selectOption(controlName: string, value: string) {
    this.ticketForm.get(controlName)?.setValue(value);
    this.activeDropdown.set(null); // Close dropdown after selection
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
      assignee: 'Rakesh Mondal (You)',
    });
    this.showCancelConfirm.set(false);
    this.activeDropdown.set(null);
    this.closeModal.emit();
  }

  submitTicket() {
    if (this.ticketForm.valid) {
      console.log('Ticket Submitted payload:', this.ticketForm.value);
      this.confirmCancel(); // Resets and closes
    }
  }
}