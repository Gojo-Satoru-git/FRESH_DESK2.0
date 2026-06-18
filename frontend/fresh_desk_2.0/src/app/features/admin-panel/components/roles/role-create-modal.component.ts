import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { UiInputComponent } from '../../../../shared/components/ui-input/ui-input.component';
import { UiButtonComponent } from '../../../../shared/components/ui-button/ui-button.component';

@Component({
  selector: 'app-role-create-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, UiInputComponent, UiButtonComponent],
  template: `
    @if (isOpen) {
      <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 transition-opacity animate-fade-in">
        <div class="bg-surface w-full max-w-lg rounded-2xl shadow-2xl overflow-hidden flex flex-col border border-disabled-gray dark:border-gray-800">
          <div class="px-8 py-6 border-b border-disabled-gray dark:border-gray-800 flex justify-between items-center bg-background">
            <div>
              <h3 class="text-2xl font-bold font-heading text-text-main">Create Custom Role</h3>
              <p class="text-sm font-sans text-text-muted mt-1">Define a new role and configure its permissions later</p>
            </div>
            <button (click)="closeModal()" class="text-text-muted hover:text-text-main hover:bg-disabled-gray/30 p-2 rounded-lg transition-colors">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
          </div>

          <div class="p-8">
            <form [formGroup]="roleForm" (ngSubmit)="onSubmit()" class="space-y-5">
              
              <app-ui-input 
                [control]="getControl('name')"
                label="Role Name"
                id="name"
                placeholder="e.g. Quality Assurance"
              ></app-ui-input>

              <app-ui-input 
                [control]="getControl('description')"
                label="Description"
                id="description"
                placeholder="Brief description of what this role does"
              ></app-ui-input>

              @if (errorMsg()) {
                <div class="p-4 bg-error-red/10 border border-error-red/20 rounded-xl mt-2">
                  <p class="text-sm font-semibold font-sans text-error-red">{{ errorMsg() }}</p>
                </div>
              }

              <div class="pt-6 flex justify-end gap-4 mt-8">
                <button type="button" (click)="closeModal()" class="px-6 py-3 text-lg font-bold font-sans text-text-main hover:bg-disabled-gray/30 rounded-xl transition-colors">
                  Cancel
                </button>
                <div class="w-48">
                  <app-ui-button type="submit" [disabled]="roleForm.invalid || isSubmitting()">
                    @if (isSubmitting()) {
                      <svg class="animate-spin -ml-1 mr-2 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      Creating...
                    } @else {
                      Create Role
                    }
                  </app-ui-button>
                </div>
              </div>
            </form>
          </div>
        </div>
      </div>
    }
  `
})
export class RoleCreateModalComponent {
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();
  @Output() roleCreated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);

  isSubmitting = signal(false);
  errorMsg = signal('');

  roleForm = this.fb.group({
    name: ['', Validators.required],
    description: ['']
  });

  getControl(name: string): FormControl {
    return this.roleForm.get(name) as FormControl;
  }

  closeModal() {
    this.roleForm.reset();
    this.errorMsg.set('');
    this.close.emit();
  }

  onSubmit() {
    if (this.roleForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMsg.set('');

    const formValue = this.roleForm.value;
    const request = {
      name: formValue.name!,
      description: formValue.description || undefined
    };

    this.adminService.createRole(request).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.roleForm.reset();
        this.roleCreated.emit();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMsg.set(err.error?.error || 'Failed to create role.');
      }
    });
  }
}
