import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, Group } from '../../services/admin.service';
import { UiButtonComponent } from '../../../../shared/components/ui-button/ui-button.component';
import { GroupCreateModalComponent } from './group-create-modal.component';
import { GroupMembersModalComponent } from './group-members-modal.component';

@Component({
  selector: 'app-group-list',
  standalone: true,
  imports: [CommonModule, UiButtonComponent, GroupCreateModalComponent, GroupMembersModalComponent],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-2">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">Support Groups</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Manage agent groups, routing strategies, and escalation logic.
          </p>
        </div>

        <div class="w-48">
          <app-ui-button (click)="openCreateModal()">
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
              New Group
            </div>
          </app-ui-button>
        </div>
      </div>

      <!-- Groups Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        @if (isLoading()) {
          <div class="col-span-full py-12 flex justify-center">
            <div class="w-8 h-8 rounded-full border-4 border-primary-blue border-t-transparent animate-spin"></div>
          </div>
        } @else if (groups().length === 0) {
          <div class="col-span-full py-12 text-center bg-surface border border-dashed border-gray-300 dark:border-gray-700 rounded-xl">
            <h3 class="text-lg font-bold text-text-main mb-2">No Groups Found</h3>
            <p class="text-text-muted text-sm">Create a group to start routing tickets.</p>
          </div>
        } @else {
          @for (group of groups(); track group.id) {
            <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm p-6 hover:shadow-md transition-shadow relative group/card">
              
              <div class="absolute top-4 right-4 flex gap-2 opacity-0 group-hover/card:opacity-100 transition-opacity">
                <button (click)="deleteGroup(group.id)" class="p-1.5 text-error-red hover:bg-error-red/10 rounded-lg transition-colors" title="Delete Group">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg>
                </button>
              </div>

              <div class="flex items-center gap-3 mb-4">
                <div class="w-10 h-10 rounded-full bg-primary-blue/10 flex items-center justify-center">
                  <svg class="w-5 h-5 text-primary-blue" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>
                </div>
                <div>
                  <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white leading-tight">{{ group.name }}</h3>
                  <p class="text-xs font-sans text-text-light dark:text-text-muted mt-0.5">
                    {{ getStrategyName(group.assignmentStrategy) }}
                  </p>
                </div>
              </div>

              <div class="space-y-3 mb-6">
                <div class="flex justify-between items-center text-sm">
                  <span class="text-text-light dark:text-text-muted">Region</span>
                  <span class="font-semibold text-text-dark dark:text-text-white">{{ group.regionCode || 'Global' }}</span>
                </div>
                <div class="flex justify-between items-center text-sm">
                  <span class="text-text-light dark:text-text-muted">Tier</span>
                  <span class="font-semibold text-text-dark dark:text-text-white">{{ group.tierCode || 'Any' }}</span>
                </div>
                <div class="flex justify-between items-center text-sm">
                  <span class="text-text-light dark:text-text-muted">Escalation Timer</span>
                  <span class="font-semibold text-error-red">{{ group.unattendedAlertMinutes }} mins</span>
                </div>
              </div>

              <div class="pt-4 border-t border-table-dark-gray dark:border-gray-800">
                <button (click)="openMembersModal(group)" class="w-full py-2 text-sm font-semibold text-primary-blue hover:bg-primary-blue/5 rounded-lg transition-colors">
                  Manage Members
                </button>
              </div>
            </div>
          }
        }
      </div>
    </div>

    <app-group-create-modal
      [isOpen]="isModalOpen()"
      (close)="isModalOpen.set(false)"
      (groupCreated)="onGroupCreated()">
    </app-group-create-modal>

    <app-group-members-modal
      [isOpen]="isMembersModalOpen()"
      [group]="selectedGroup()"
      (close)="closeMembersModal()">
    </app-group-members-modal>
  `
})
export class GroupListComponent implements OnInit {
  private adminService = inject(AdminService);

  groups = signal<Group[]>([]);
  isLoading = signal(false);
  isModalOpen = signal(false);

  isMembersModalOpen = signal(false);
  selectedGroup = signal<Group | null>(null);

  ngOnInit() {
    this.loadGroups();
  }

  loadGroups() {
    this.isLoading.set(true);
    this.adminService.getGroups().subscribe({
      next: (res) => {
        this.groups.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load groups', err);
        this.isLoading.set(false);
      }
    });
  }

  getStrategyName(strategy: number): string {
    switch (strategy) {
      case 0: return 'Round Robin';
      case 1: return 'Load Balanced';
      case 2: return 'Skills Based';
      default: return 'Manual Assignment';
    }
  }

  deleteGroup(id: string) {
    if (confirm('Are you sure you want to delete this group? All associated members will be removed from it.')) {
      this.adminService.deleteGroup(id).subscribe({
        next: () => this.loadGroups(),
        error: (err) => console.error('Failed to delete group', err)
      });
    }
  }

  openCreateModal() {
    this.isModalOpen.set(true);
  }

  onGroupCreated() {
    this.isModalOpen.set(false);
    this.loadGroups();
  }

  openMembersModal(group: Group) {
    this.selectedGroup.set(group);
    this.isMembersModalOpen.set(true);
  }

  closeMembersModal() {
    this.isMembersModalOpen.set(false);
    this.selectedGroup.set(null);
  }
}
