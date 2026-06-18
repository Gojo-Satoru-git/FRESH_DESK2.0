import { Component, EventEmitter, Input, Output, inject, signal, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, Group } from '../../services/admin.service';
import { UiButtonComponent } from '../../../../shared/components/ui-button/ui-button.component';

@Component({
  selector: 'app-group-members-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, UiButtonComponent],
  template: `
    @if (isOpen && group) {
      <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 transition-opacity animate-fade-in">
        <div class="bg-surface w-full max-w-3xl rounded-2xl shadow-2xl overflow-hidden flex flex-col border border-disabled-gray dark:border-gray-800 h-[80vh]">
          
          <div class="px-8 py-6 border-b border-disabled-gray dark:border-gray-800 flex justify-between items-center bg-background shrink-0">
            <div>
              <h3 class="text-2xl font-bold font-heading text-text-main">Manage Members: {{ group.name }}</h3>
              <p class="text-sm font-sans text-text-muted mt-1">Assign agents and team leads to this routing group</p>
            </div>
            <button (click)="closeModal()" class="text-text-muted hover:text-text-main hover:bg-disabled-gray/30 p-2 rounded-lg transition-colors">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
          </div>

          <div class="flex-1 overflow-hidden flex flex-col p-8 bg-bg-light dark:bg-bg-dark/50">
            
            <div class="flex gap-4 mb-6">
              <div class="relative flex-1">
                <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <svg class="h-4 w-4 text-text-light" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <input 
                  type="text" 
                  [(ngModel)]="searchEmail"
                  placeholder="Find user by email to add..."
                  class="block w-full pl-10 pr-3 py-3 border border-gray-300 dark:border-gray-700 rounded-xl text-sm bg-surface text-text-main shadow-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-all"
                >
              </div>
              <div class="w-32">
                <app-ui-button (click)="searchAndAddUser()" [disabled]="!searchEmail || isAdding()">
                  @if (isAdding()) {
                    Adding...
                  } @else {
                    Add User
                  }
                </app-ui-button>
              </div>
            </div>

            @if (errorMsg()) {
              <div class="p-4 bg-error-red/10 border border-error-red/20 rounded-xl mb-6">
                <p class="text-sm font-semibold font-sans text-error-red">{{ errorMsg() }}</p>
              </div>
            }

            <div class="bg-card-white dark:bg-surface rounded-xl border border-table-dark-gray dark:border-gray-800 shadow-sm overflow-hidden flex-1 flex flex-col">
              <div class="p-4 border-b border-table-dark-gray dark:border-gray-800 bg-table-light-gray dark:bg-gray-900/50 flex justify-between items-center">
                <h4 class="font-bold font-heading text-text-dark dark:text-text-white">Current Members</h4>
                <span class="bg-primary-blue/10 text-primary-blue text-xs font-bold px-2.5 py-0.5 rounded-full border border-primary-blue/20">
                  {{ members().length }} Total
                </span>
              </div>
              
              <div class="flex-1 overflow-y-auto p-2">
                @if (isLoading()) {
                  <div class="p-8 flex justify-center text-text-light">
                    <div class="w-6 h-6 rounded-full border-2 border-primary-blue border-t-transparent animate-spin"></div>
                  </div>
                } @else if (members().length === 0) {
                  <div class="p-8 text-center text-text-light">
                    No members in this group yet.
                  </div>
                } @else {
                  <div class="space-y-2">
                    @for (member of members(); track member.id) {
                      <div class="flex items-center justify-between p-3 hover:bg-table-light-gray dark:hover:bg-gray-800 rounded-lg transition-colors border border-transparent hover:border-table-dark-gray dark:hover:border-gray-700">
                        <div class="flex items-center gap-3">
                          <div class="w-8 h-8 rounded-full bg-primary-blue/10 flex items-center justify-center text-primary-blue font-bold text-xs">
                            {{ member.firstName[0] }}{{ member.lastName[0] }}
                          </div>
                          <div>
                            <p class="text-sm font-bold text-text-dark dark:text-text-white">{{ member.firstName }} {{ member.lastName }}</p>
                            <p class="text-xs text-text-light">{{ member.email }}</p>
                          </div>
                        </div>
                        <div class="flex items-center gap-4">
                          @if (member.isLead) {
                            <span class="bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-300 text-[10px] font-bold px-2 py-0.5 rounded uppercase tracking-wider border border-purple-200 dark:border-purple-800">Team Lead</span>
                          }
                          <button (click)="removeMember(member.id)" class="text-error-red hover:bg-error-red/10 p-1.5 rounded transition-colors" title="Remove Member">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg>
                          </button>
                        </div>
                      </div>
                    }
                  </div>
                }
              </div>
            </div>

          </div>
        </div>
      </div>
    }
  `
})
export class GroupMembersModalComponent implements OnChanges {
  @Input() isOpen = false;
  @Input() group: Group | null = null;
  @Output() close = new EventEmitter<void>();

  private adminService = inject(AdminService);

  // In a real scenario we'd use a dedicated endpoint to fetch group members. 
  // For now, we will just simulate the members array or fetch from a hypothetical endpoint.
  // We'll add this endpoint to AdminService if it doesn't exist.
  members = signal<any[]>([]);
  isLoading = signal(false);
  isAdding = signal(false);
  
  searchEmail = '';
  errorMsg = signal('');

  ngOnChanges(changes: SimpleChanges) {
    if (changes['isOpen'] && this.isOpen && this.group) {
      this.loadMembers();
    }
  }

  loadMembers() {
    if (!this.group) return;
    this.isLoading.set(true);
    // Since we don't have GetMembers in AdminService yet, we'll fetch all users and filter by group...
    // Actually, GroupsController has GET /{id:guid}/members! We should add it to AdminService.
    this.adminService.getGroupMembers(this.group.id).subscribe({
      next: (res) => {
        this.members.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading members', err);
        this.isLoading.set(false);
      }
    });
  }

  searchAndAddUser() {
    if (!this.group || !this.searchEmail) return;
    
    this.isAdding.set(true);
    this.errorMsg.set('');

    // First find the user by email
    this.adminService.getUsers(1, 1, this.searchEmail).subscribe({
      next: (res) => {
        if (res.items && res.items.length > 0) {
          const userId = res.items[0].id;
          // Add them to the group
          this.adminService.addGroupMember(this.group!.id, userId, false).subscribe({
            next: () => {
              this.searchEmail = '';
              this.isAdding.set(false);
              this.loadMembers();
            },
            error: (err) => {
              this.errorMsg.set(err.error?.error || 'Failed to add user to group.');
              this.isAdding.set(false);
            }
          });
        } else {
          this.errorMsg.set('User with that email not found.');
          this.isAdding.set(false);
        }
      },
      error: () => {
        this.errorMsg.set('Failed to search for user.');
        this.isAdding.set(false);
      }
    });
  }

  removeMember(userId: string) {
    if (!this.group) return;
    if (confirm('Remove this user from the group?')) {
      this.adminService.removeGroupMember(this.group.id, userId).subscribe({
        next: () => this.loadMembers(),
        error: (err) => console.error('Failed to remove member', err)
      });
    }
  }

  closeModal() {
    this.searchEmail = '';
    this.errorMsg.set('');
    this.close.emit();
  }
}
