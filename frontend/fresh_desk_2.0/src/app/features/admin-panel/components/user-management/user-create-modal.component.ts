import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { UiInputComponent } from '../../../../shared/components/ui-input/ui-input.component';
import { UiButtonComponent } from '../../../../shared/components/ui-button/ui-button.component';

@Component({
  selector: 'app-user-create-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, UiInputComponent, UiButtonComponent],
  template: `
    @if (isOpen) {
      <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 transition-opacity animate-fade-in">
        <div class="bg-surface w-full max-w-lg rounded-2xl shadow-2xl overflow-hidden flex flex-col border border-disabled-gray dark:border-gray-800">
          <div class="px-8 py-6 border-b border-disabled-gray dark:border-gray-800 flex justify-between items-center bg-background">
            <div>
              <h3 class="text-2xl font-bold font-heading text-text-main">Create New User</h3>
              <p class="text-sm font-sans text-text-muted mt-1">Provision a new internal agent or manager</p>
            </div>
            <button (click)="closeModal()" class="text-text-muted hover:text-text-main hover:bg-disabled-gray/30 p-2 rounded-lg transition-colors">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
          </div>

          <div class="p-8">
            <form [formGroup]="userForm" (ngSubmit)="onSubmit()" class="space-y-5">
              
              <div class="grid grid-cols-2 gap-5">
                <app-ui-input 
                  [control]="getControl('firstName')"
                  label="First Name"
                  id="firstName"
                  placeholder="John"
                ></app-ui-input>

                <app-ui-input 
                  [control]="getControl('lastName')"
                  label="Last Name"
                  id="lastName"
                  placeholder="Doe"
                ></app-ui-input>
              </div>

              <app-ui-input 
                [control]="getControl('email')"
                label="Email Address"
                id="email"
                type="email"
                placeholder="john.doe@company.com"
              ></app-ui-input>

              <app-ui-input 
                [control]="getControl('phone')"
                label="Phone (Optional)"
                id="phone"
                type="text"
                placeholder="+1 234 567 890"
              ></app-ui-input>

              <div class="space-y-1 w-full">
                <label class="block text-xl font-medium text-text-main transition-colors">
                  System Role
                </label>
                <div class="relative transition-all">
                  <select formControlName="roleName" class="w-full px-5 py-3 text-lg bg-surface text-text-main shadow-lg border border-gray-300 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all cursor-pointer appearance-none">
                    <option value="" disabled selected>Select a role...</option>
                    <option value="Agent">Agent</option>
                    <option value="SeniorAgent">Senior Agent</option>
                    <option value="TeamLead">Team Lead</option>
                    <option value="Manager">Manager</option>
                    <option value="Admin">System Administrator</option>
                  </select>
                  <div class="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none">
                    <svg class="w-5 h-5 text-text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
                    </svg>
                  </div>
                </div>
              </div>

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
                  <app-ui-button type="submit" [disabled]="userForm.invalid || isSubmitting()">
                    @if (isSubmitting()) {
                      <svg class="animate-spin -ml-1 mr-2 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      Creating...
                    } @else {
                      Create User
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
export class UserCreateModalComponent {
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();
  @Output() userCreated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);

  isSubmitting = signal(false);
  errorMsg = signal('');

  userForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    roleName: ['', Validators.required]
  });

  getControl(name: string): FormControl {
    return this.userForm.get(name) as FormControl;
  }

  closeModal() {
    this.userForm.reset();
    this.errorMsg.set('');
    this.close.emit();
  }

  onSubmit() {
    if (this.userForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMsg.set('');

    const formValue = this.userForm.value;
    const request = {
      firstName: formValue.firstName!,
      lastName: formValue.lastName!,
      email: formValue.email!,
      phone: formValue.phone || undefined,
      roleName: formValue.roleName!
    };

    this.adminService.createInternalUser(request).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.userForm.reset();
        this.userCreated.emit();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMsg.set(err.error?.error || 'Failed to create user. Ensure email is unique.');
      }
    });
  }
}
