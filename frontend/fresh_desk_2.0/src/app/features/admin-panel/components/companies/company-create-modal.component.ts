import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { UiInputComponent } from '../../../../shared/components/ui-input/ui-input.component';
import { UiButtonComponent } from '../../../../shared/components/ui-button/ui-button.component';

@Component({
  selector: 'app-company-create-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, UiInputComponent, UiButtonComponent],
  template: `
    @if (isOpen) {
      <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 transition-opacity animate-fade-in">
        <div class="bg-surface w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col border border-disabled-gray dark:border-gray-800">
          <div class="px-8 py-6 border-b border-disabled-gray dark:border-gray-800 flex justify-between items-center bg-background">
            <div>
              <h3 class="text-2xl font-bold font-heading text-text-main">Create Customer Company</h3>
              <p class="text-sm font-sans text-text-muted mt-1">Onboard a new organization into the ticketing system</p>
            </div>
            <button (click)="closeModal()" class="text-text-muted hover:text-text-main hover:bg-disabled-gray/30 p-2 rounded-lg transition-colors">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
          </div>

          <div class="p-8 max-h-[80vh] overflow-y-auto">
            <form [formGroup]="companyForm" (ngSubmit)="onSubmit()" class="space-y-6">
              
              <app-ui-input 
                [control]="getControl('name')"
                label="Company Name"
                id="name"
                placeholder="e.g. Acme Corporation"
              ></app-ui-input>

              <div class="grid grid-cols-2 gap-5">
                <app-ui-input 
                  [control]="getControl('geoRegion')"
                  label="Geographic Region"
                  id="geoRegion"
                  placeholder="e.g. NA, EMEA, APAC"
                ></app-ui-input>

                <div class="space-y-1 w-full">
                  <label class="block text-xl font-medium text-text-main transition-colors">
                    Support Tier
                  </label>
                  <div class="relative transition-all">
                    <select formControlName="supportTier" class="w-full px-5 py-3 text-lg bg-surface text-text-main shadow-lg border border-gray-300 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all cursor-pointer appearance-none">
                      <option value="Standard">Standard</option>
                      <option value="Premium">Premium</option>
                      <option value="Enterprise">Enterprise</option>
                    </select>
                    <div class="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none">
                      <svg class="w-5 h-5 text-text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
                      </svg>
                    </div>
                  </div>
                </div>
              </div>

              <app-ui-input 
                [control]="getControl('industry')"
                label="Industry (Optional)"
                id="industry"
                placeholder="e.g. Healthcare, Finance, Technology"
              ></app-ui-input>

              <div class="space-y-1 w-full">
                <label for="notes" class="block text-xl font-medium text-text-main transition-colors">
                  Notes
                </label>
                <textarea 
                  id="notes" 
                  formControlName="notes" 
                  rows="3" 
                  placeholder="Any additional information..."
                  class="w-full px-5 py-3 text-lg bg-surface text-text-main shadow-lg border border-gray-300 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all placeholder:text-text-muted">
                </textarea>
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
                  <app-ui-button type="submit" [disabled]="companyForm.invalid || isSubmitting()">
                    @if (isSubmitting()) {
                      <svg class="animate-spin -ml-1 mr-2 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      Creating...
                    } @else {
                      Create Company
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
export class CompanyCreateModalComponent {
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();
  @Output() companyCreated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);

  isSubmitting = signal(false);
  errorMsg = signal('');

  companyForm = this.fb.group({
    name: ['', Validators.required],
    geoRegion: ['', Validators.required],
    supportTier: ['Standard', Validators.required],
    industry: [''],
    notes: ['']
  });

  getControl(name: string): FormControl {
    return this.companyForm.get(name) as FormControl;
  }

  closeModal() {
    this.companyForm.reset({ supportTier: 'Standard' });
    this.errorMsg.set('');
    this.close.emit();
  }

  onSubmit() {
    if (this.companyForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMsg.set('');

    const formValue = this.companyForm.value;
    const request = {
      name: formValue.name!,
      geoRegion: formValue.geoRegion!,
      supportTier: formValue.supportTier!,
      industry: formValue.industry || undefined,
      notes: formValue.notes || undefined
    };

    this.adminService.createCompany(request).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.companyForm.reset({ supportTier: 'Standard' });
        this.companyCreated.emit();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMsg.set(err.error?.error || 'Failed to create company.');
      }
    });
  }
}
