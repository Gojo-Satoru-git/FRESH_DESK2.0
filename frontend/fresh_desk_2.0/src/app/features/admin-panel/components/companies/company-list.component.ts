import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { AdminService, Company } from '../../services/admin.service';
import { UiButtonComponent } from '../../../../shared/components/ui-button/ui-button.component';
import { CompanyCreateModalComponent } from './company-create-modal.component';

@Component({
  selector: 'app-company-list',
  standalone: true,
  imports: [CommonModule, DatePipe, UiButtonComponent, CompanyCreateModalComponent],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-2">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">Customer Companies</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Manage your organization's customer portfolio, SLAs, and dedicated support tiers.
          </p>
        </div>

        <div class="w-48">
          <app-ui-button (click)="openCreateModal()">
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
              New Company
            </div>
          </app-ui-button>
        </div>
      </div>

      <!-- Companies Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        @if (isLoading()) {
          <div class="col-span-full py-12 flex justify-center">
            <div class="w-8 h-8 rounded-full border-4 border-primary-blue border-t-transparent animate-spin"></div>
          </div>
        } @else if (companies().length === 0) {
          <div class="col-span-full py-12 text-center bg-surface border border-dashed border-gray-300 dark:border-gray-700 rounded-xl">
            <h3 class="text-lg font-bold text-text-main mb-2">No Companies Found</h3>
            <p class="text-text-muted text-sm">Create an organization to start managing their tickets.</p>
          </div>
        } @else {
          @for (company of companies(); track company.id) {
            <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 hover:shadow-md transition-shadow relative group/card flex flex-col">
              
              <div class="flex items-start justify-between mb-4">
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 rounded-full bg-primary-blue/10 flex items-center justify-center text-primary-blue font-bold text-sm uppercase">
                    {{ company.name.substring(0, 2) }}
                  </div>
                  <div>
                    <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white leading-tight truncate max-w-[150px]" [title]="company.name">
                      {{ company.name }}
                    </h3>
                    <p class="text-xs font-sans text-text-light dark:text-text-muted mt-0.5">
                      {{ company.industry || 'Unknown Industry' }}
                    </p>
                  </div>
                </div>
                
                <span [class]="'px-2 py-1 text-[10px] font-bold rounded-full uppercase tracking-wider border ' + getStatusClass(company.status)">
                  {{ company.status || 'Active' }}
                </span>
              </div>

              <div class="space-y-3 mb-6 flex-1">
                <div class="flex justify-between items-center text-sm">
                  <span class="text-text-light dark:text-text-muted">Region</span>
                  <span class="font-semibold text-text-dark dark:text-text-white">{{ company.geoRegion || 'Global' }}</span>
                </div>
                <div class="flex justify-between items-center text-sm">
                  <span class="text-text-light dark:text-text-muted">Support Tier</span>
                  <span class="font-semibold text-text-dark dark:text-text-white">{{ company.supportTier || 'Standard' }}</span>
                </div>
                <div class="flex justify-between items-center text-sm">
                  <span class="text-text-light dark:text-text-muted">Since</span>
                  <span class="font-semibold text-text-dark dark:text-text-white">{{ company.createdAt | date:'MMM yyyy' }}</span>
                </div>
              </div>

              <div class="pt-4 border-t border-table-dark-gray dark:border-gray-800 flex justify-between gap-2">
                <button class="flex-1 py-2 text-sm font-semibold text-primary-blue hover:bg-primary-blue/5 rounded-lg transition-colors">
                  View Details
                </button>
                <button class="flex-1 py-2 text-sm font-semibold text-primary-blue hover:bg-primary-blue/5 rounded-lg transition-colors">
                  Assign Group
                </button>
              </div>
            </div>
          }
        }
      </div>
      
      <!-- Basic pagination placeholder if more than one page -->
      @if (totalPages() > 1) {
        <div class="flex justify-center gap-2 mt-6">
          <button 
            [disabled]="currentPage() === 1"
            (click)="changePage(currentPage() - 1)"
            class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg disabled:opacity-50 text-text-main hover:bg-disabled-gray/30 transition-colors">
            Prev
          </button>
          <span class="px-4 py-2 text-text-main font-semibold">Page {{ currentPage() }} of {{ totalPages() }}</span>
          <button 
            [disabled]="currentPage() === totalPages()"
            (click)="changePage(currentPage() + 1)"
            class="px-4 py-2 border border-gray-300 dark:border-gray-700 rounded-lg disabled:opacity-50 text-text-main hover:bg-disabled-gray/30 transition-colors">
            Next
          </button>
        </div>
      }
    </div>

    <app-company-create-modal
      [isOpen]="isModalOpen()"
      (close)="isModalOpen.set(false)"
      (companyCreated)="onCompanyCreated()">
    </app-company-create-modal>
  `
})
export class CompanyListComponent implements OnInit {
  private adminService = inject(AdminService);

  companies = signal<Company[]>([]);
  isLoading = signal(false);
  isModalOpen = signal(false);
  
  currentPage = signal(1);
  totalPages = signal(1);

  ngOnInit() {
    this.loadCompanies();
  }

  loadCompanies() {
    this.isLoading.set(true);
    this.adminService.getCompanies(this.currentPage(), 12).subscribe({
      next: (res) => {
        this.companies.set(res.items);
        this.totalPages.set(res.totalPages || 1);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load companies', err);
        this.isLoading.set(false);
      }
    });
  }

  changePage(page: number) {
    this.currentPage.set(page);
    this.loadCompanies();
  }

  getStatusClass(status: string): string {
    const s = (status || 'active').toLowerCase();
    if (s === 'active') return 'bg-success-green/10 text-success-green border-success-green/20';
    if (s === 'inactive') return 'bg-gray-100 text-gray-800 border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700';
    return 'bg-warning-yellow/10 text-warning-yellow border-warning-yellow/20';
  }

  openCreateModal() {
    this.isModalOpen.set(true);
  }

  onCompanyCreated() {
    this.isModalOpen.set(false);
    this.loadCompanies();
  }
}
