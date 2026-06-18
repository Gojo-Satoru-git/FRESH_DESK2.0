import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, Role, Permission } from '../../services/admin.service';
import { UiButtonComponent } from '../../../../shared/components/ui-button/ui-button.component';
import { RoleCreateModalComponent } from '../roles/role-create-modal.component';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [CommonModule, FormsModule, UiButtonComponent, RoleCreateModalComponent],
  template: `
    <div class="max-w-7xl mx-auto space-y-6 animate-fade-in pb-12">
      
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-2">
        <div>
          <h2 class="text-2xl font-bold font-heading text-text-dark dark:text-text-white">Roles & Permissions</h2>
          <p class="text-sm font-sans text-text-light dark:text-text-muted mt-1">
            Manage custom roles and system access policies.
          </p>
        </div>

        <div class="w-40">
          <app-ui-button (click)="openCreateModal()">
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
              New Role
            </div>
          </app-ui-button>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- Roles List -->
        <div class="lg:col-span-1 bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm overflow-hidden flex flex-col h-[600px]">
          <div class="p-4 border-b border-table-dark-gray dark:border-gray-800 bg-table-light-gray dark:bg-gray-900/50">
            <h3 class="font-bold font-heading text-text-dark dark:text-text-white">System Roles</h3>
          </div>
          
          <div class="flex-1 overflow-y-auto p-2">
            @if (isLoadingRoles()) {
              <div class="p-8 text-center text-sm text-text-light flex flex-col items-center gap-3">
                <div class="w-6 h-6 rounded-full border-2 border-primary-blue border-t-transparent animate-spin"></div>
                Loading roles...
              </div>
            } @else {
              <div class="space-y-1">
                @for (role of roles(); track role.id) {
                  <button 
                    (click)="selectRole(role)"
                    [class.bg-primary-blue]="selectedRole()?.id === role.id"
                    [class.text-white]="selectedRole()?.id === role.id"
                    [class.hover:bg-table-light-gray]="selectedRole()?.id !== role.id"
                    [class.dark:hover:bg-gray-800]="selectedRole()?.id !== role.id"
                    class="w-full text-left px-4 py-3 rounded-lg transition-colors flex flex-col items-start gap-1"
                  >
                    <div class="flex items-center justify-between w-full">
                      <span class="font-semibold text-sm">{{ role.name }}</span>
                      @if (role.isSystemRole) {
                        <svg class="w-4 h-4 opacity-70" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M2.166 4.999A11.954 11.954 0 0010 1.944 11.954 11.954 0 0017.834 5c.11.65.166 1.32.166 2.001 0 5.225-3.34 9.67-8 11.317C5.34 16.67 2 12.225 2 7c0-.682.057-1.35.166-2.001zm11.541 3.708a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path></svg>
                      }
                    </div>
                    <span class="text-xs opacity-80 truncate w-full">{{ role.description || 'No description' }}</span>
                  </button>
                }
              </div>
            }
          </div>
        </div>

        <!-- Role Details & Permissions -->
        <div class="lg:col-span-2 bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm h-[600px] flex flex-col">
          @if (selectedRole()) {
            <div class="p-6 border-b border-table-dark-gray dark:border-gray-800">
              <div class="flex items-start justify-between">
                <div>
                  <h3 class="text-xl font-bold font-heading text-text-dark dark:text-text-white">{{ selectedRole()!.name }}</h3>
                  <p class="text-sm text-text-light mt-1">{{ selectedRole()!.description }}</p>
                </div>
                @if (selectedRole()!.isSystemRole) {
                  <span class="bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-300 text-xs font-semibold px-2.5 py-1 rounded-md border border-gray-200 dark:border-gray-700">
                    System Role (Immutable)
                  </span>
                }
              </div>
            </div>

            <div class="flex-1 overflow-y-auto p-6 bg-bg-light dark:bg-bg-dark/50">
              <h4 class="text-sm font-bold text-text-dark dark:text-text-white mb-4">Assigned Permissions</h4>
              
              @if (isLoadingPermissions()) {
                <div class="py-8 text-center text-sm text-text-light flex justify-center">
                  <div class="w-5 h-5 rounded-full border-2 border-primary-blue border-t-transparent animate-spin"></div>
                </div>
              } @else {
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  @for (perm of allPermissions(); track perm.id) {
                    <label 
                      class="flex items-center gap-3 p-3 rounded-lg border border-table-dark-gray dark:border-gray-800 bg-card-white dark:bg-surface hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors cursor-pointer"
                      [class.opacity-60]="selectedRole()!.isSystemRole"
                    >
                      <input 
                        type="checkbox" 
                        [checked]="isPermissionAssigned(perm.id)"
                        (change)="togglePermission(perm.id)"
                        [disabled]="selectedRole()!.isSystemRole"
                        class="w-4 h-4 text-primary-blue rounded border-gray-300 focus:ring-primary-blue"
                      >
                      <div>
                        <div class="text-sm font-semibold text-text-dark dark:text-text-white">{{ perm.resource }}:{{ perm.action }}</div>
                        <div class="text-xs text-text-light mt-0.5">{{ perm.description }}</div>
                      </div>
                    </label>
                  }
                </div>
              }
            </div>

            <div class="p-4 border-t border-table-dark-gray dark:border-gray-800 bg-card-white dark:bg-surface flex justify-end">
              <div class="w-32">
                <app-ui-button 
                  (click)="savePermissions()" 
                  [disabled]="selectedRole()!.isSystemRole || !hasUnsavedChanges() || isSaving()"
                >
                  @if (isSaving()) {
                    <span class="flex items-center"><svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>Saving...</span>
                  } @else {
                    Save Changes
                  }
                </app-ui-button>
              </div>
            </div>
          } @else {
            <div class="flex-1 flex flex-col items-center justify-center text-text-light p-8 text-center">
              <svg class="w-16 h-16 mb-4 opacity-50 text-text-light" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path></svg>
              <h3 class="text-lg font-bold text-text-dark dark:text-text-white mb-2">No Role Selected</h3>
              <p class="text-sm max-w-sm">Select a role from the list to view and manage its associated permissions.</p>
            </div>
          }
        </div>
      </div>
    </div>

    <app-role-create-modal 
      [isOpen]="isModalOpen()" 
      (close)="isModalOpen.set(false)"
      (roleCreated)="onRoleCreated()">
    </app-role-create-modal>
  `
})
export class RoleListComponent implements OnInit {
  private adminService = inject(AdminService);

  roles = signal<Role[]>([]);
  allPermissions = signal<Permission[]>([]);
  
  selectedRole = signal<Role | null>(null);
  assignedPermissionIds = signal<Set<string>>(new Set());
  draftPermissionIds = signal<Set<string>>(new Set());
  
  isLoadingRoles = signal(false);
  isLoadingPermissions = signal(false);
  isSaving = signal(false);
  
  isModalOpen = signal(false);

  ngOnInit() {
    this.loadRoles();
    this.loadAllPermissions();
  }

  loadRoles() {
    this.isLoadingRoles.set(true);
    this.adminService.getRoles().subscribe({
      next: (res) => {
        this.roles.set(res);
        this.isLoadingRoles.set(false);
      },
      error: (err) => {
        console.error('Error loading roles', err);
        this.isLoadingRoles.set(false);
      }
    });
  }

  loadAllPermissions() {
    this.adminService.getPermissions().subscribe({
      next: (res) => this.allPermissions.set(res),
      error: (err) => console.error('Error loading permissions', err)
    });
  }

  selectRole(role: Role) {
    this.selectedRole.set(role);
    this.loadRolePermissions(role.id);
  }

  loadRolePermissions(roleId: string) {
    this.isLoadingPermissions.set(true);
    this.adminService.getRolePermissions(roleId).subscribe({
      next: (res) => {
        const ids = new Set(res.permissions.map(p => p.id));
        this.assignedPermissionIds.set(ids);
        this.draftPermissionIds.set(new Set(ids));
        this.isLoadingPermissions.set(false);
      },
      error: (err) => {
        console.error('Error loading role permissions', err);
        this.isLoadingPermissions.set(false);
      }
    });
  }

  isPermissionAssigned(permId: string): boolean {
    return this.draftPermissionIds().has(permId);
  }

  togglePermission(permId: string) {
    if (this.selectedRole()?.isSystemRole) return;
    
    const draft = new Set(this.draftPermissionIds());
    if (draft.has(permId)) {
      draft.delete(permId);
    } else {
      draft.add(permId);
    }
    this.draftPermissionIds.set(draft);
  }

  hasUnsavedChanges(): boolean {
    const assigned = this.assignedPermissionIds();
    const draft = this.draftPermissionIds();
    if (assigned.size !== draft.size) return true;
    for (const item of draft) {
      if (!assigned.has(item)) return true;
    }
    return false;
  }

  savePermissions() {
    const roleId = this.selectedRole()?.id;
    if (!roleId || this.selectedRole()?.isSystemRole) return;

    this.isSaving.set(true);
    const permIds = Array.from(this.draftPermissionIds());
    this.adminService.setRolePermissions(roleId, permIds).subscribe({
      next: () => {
        this.assignedPermissionIds.set(new Set(permIds));
        this.isSaving.set(false);
      },
      error: (err) => {
        console.error('Error saving permissions', err);
        this.isSaving.set(false);
      }
    });
  }

  openCreateModal() {
    this.isModalOpen.set(true);
  }

  onRoleCreated() {
    this.isModalOpen.set(false);
    this.loadRoles();
  }
}
