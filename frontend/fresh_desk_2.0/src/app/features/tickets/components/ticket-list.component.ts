import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full flex flex-col animate-fade-in relative">
      
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center py-4 border-b border-gray-200 dark:border-gray-800 gap-4 mb-4">
        
        <div class="flex items-center gap-4 text-sm">
          <div class="flex items-center gap-2 cursor-pointer hover:text-primary transition-colors text-text-muted">
            <input type="checkbox" class="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary" />
          </div>
          <button class="flex items-center gap-1 text-text-muted hover:text-text-main transition-colors font-medium">
            Sort by: Date created
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
          </button>
        </div>

        <div class="flex items-center gap-4 text-sm">
          <button class="flex items-center gap-1 text-text-muted hover:text-text-main transition-colors">
            Layout: Card view
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
          </button>
          
          <div class="h-6 w-px bg-gray-200 dark:bg-gray-800"></div>
          
          <button class="flex items-center gap-1 text-text-main font-medium border border-gray-300 dark:border-gray-700 px-3 py-1.5 rounded hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"></path></svg>
            Export
          </button>
          
          <div class="flex items-center gap-2 text-text-muted">
            <span class="mr-2">1 - 11 of 11</span>
            <button class="p-1 rounded hover:bg-gray-200 dark:hover:bg-gray-800 disabled:opacity-50"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"></path></svg></button>
            <button class="p-1 rounded hover:bg-gray-200 dark:hover:bg-gray-800 disabled:opacity-50"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg></button>
          </div>
        </div>
      </div>

      <div class="flex flex-1 overflow-hidden gap-6 pb-6">
        
        <div class="flex-1 overflow-y-auto space-y-3 pr-2 scrollbar-hide">
          
          @for (ticket of tickets(); track ticket.id) {
            <div class="bg-surface border border-gray-200 dark:border-gray-800 rounded-lg p-4 flex gap-4 hover:shadow-md transition-all cursor-pointer group">
              
              <div class="flex-shrink-0 pt-1">
                <input type="checkbox" class="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary bg-transparent" />
              </div>
              
              <div class="flex-shrink-0">
                <div [class]="'w-10 h-10 rounded flex items-center justify-center font-bold text-lg ' + ticket.avatarColor">
                  {{ ticket.requester.charAt(0) }}
                </div>
              </div>
              
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-3 mb-1.5">
                  @if (ticket.isNew) {
                    <span class="text-[10px] font-bold px-1.5 py-0.5 rounded bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400 uppercase tracking-wide">New</span>
                  }
                  <h4 class="text-base font-semibold text-text-main truncate group-hover:text-primary transition-colors">
                    {{ ticket.subject }} 
                    <span class="text-text-muted font-normal ml-1">#{{ ticket.id }}</span>
                  </h4>
                </div>
                
                <div class="text-xs text-text-muted flex items-center gap-1.5 flex-wrap">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path></svg>
                  <span class="font-medium text-text-main">{{ ticket.requester }}</span>
                  <span>•</span>
                  <span>Created {{ ticket.created }}</span>
                  <span>•</span>
                  <span>{{ ticket.slaStatus }}</span>
                </div>
              </div>
              
              <div class="flex-shrink-0 w-32 md:w-48 text-sm flex flex-col gap-2 justify-center hidden sm:flex border-l border-gray-100 dark:border-gray-800 pl-4">
                
                <div class="flex items-center gap-2">
                  <span [class]="'w-2 h-2 rounded-full ' + getPriorityColor(ticket.priority)"></span>
                  <span class="text-text-muted">{{ ticket.priority }}</span>
                  <svg class="w-3 h-3 text-gray-400 ml-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                </div>
                
                <div class="flex items-center gap-2">
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>
                  <span class="text-text-muted truncate">{{ ticket.group }}</span>
                  <svg class="w-3 h-3 text-gray-400 ml-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                </div>
                
                <div class="flex items-center gap-2">
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                  <span class="text-text-main font-medium">{{ ticket.status }}</span>
                  <svg class="w-3 h-3 text-gray-400 ml-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                </div>

              </div>
            </div>
          }
          
        </div>

        <div class="hidden lg:flex w-72 flex-col bg-surface border border-gray-200 dark:border-gray-800 rounded-lg overflow-hidden flex-shrink-0 shadow-sm">
          
          <div class="p-4 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center bg-gray-50/50 dark:bg-gray-900/50">
            <span class="text-xs font-bold text-text-muted uppercase tracking-wider">Filters</span>
            <button class="p-1 hover:bg-gray-200 dark:hover:bg-gray-700 rounded"><svg class="w-4 h-4 text-text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg></button>
          </div>
          
          <div class="p-4 space-y-5 flex-1 overflow-y-auto">
            
            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-text-main">Agents</label>
              <select class="w-full bg-background border border-gray-300 dark:border-gray-700 text-text-main text-sm rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-primary focus:border-primary">
                <option>Any</option>
                <option>Me</option>
                <option>Unassigned</option>
              </select>
            </div>

            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-text-main">Resolution due by</label>
              <select class="w-full bg-background border border-gray-300 dark:border-gray-700 text-text-main text-sm rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-primary focus:border-primary">
                <option>Any</option>
                <option>Today</option>
                <option>Tomorrow</option>
              </select>
            </div>
            
            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-text-main">First response due by</label>
              <select class="w-full bg-background border border-gray-300 dark:border-gray-700 text-text-main text-sm rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-primary focus:border-primary">
                <option>Any</option>
              </select>
            </div>

            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-text-main">Status</label>
              <select class="w-full bg-background border border-gray-300 dark:border-gray-700 text-text-main text-sm rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-primary focus:border-primary">
                <option>Any</option>
                <option>Open</option>
                <option>Pending</option>
                <option>Resolved</option>
                <option>Closed</option>
              </select>
            </div>

            <div class="space-y-1.5">
              <label class="text-xs font-semibold text-text-main">Priority</label>
              <select class="w-full bg-background border border-gray-300 dark:border-gray-700 text-text-main text-sm rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-primary focus:border-primary">
                <option>Any</option>
                <option>Low</option>
                <option>Medium</option>
                <option>High</option>
                <option>Urgent</option>
              </select>
            </div>

          </div>
          
          <div class="p-4 border-t border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-900/50">
             <button class="w-full bg-primary hover:bg-primary-hover text-white font-semibold py-2 rounded-lg transition-colors">
               Apply
             </button>
          </div>
        </div>

      </div>
    </div>
  `
})
export class TicketListComponent {
  
  // Mock Data perfectly mirroring your screenshot
  tickets = signal([
    {
      id: '7',
      subject: 'How can I get a refund for my order?',
      requester: 'Matt Rogers',
      created: '3 hours ago',
      slaStatus: 'First response due in a day',
      priority: 'Low',
      group: 'Billing / --',
      status: 'Open',
      isNew: true,
      avatarColor: 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-400'
    },
    {
      id: '6',
      subject: 'How do I place a custom order?',
      requester: 'John',
      created: '3 hours ago',
      slaStatus: 'First response due in a day',
      priority: 'Medium',
      group: '-- / --',
      status: 'Open',
      isNew: true,
      avatarColor: 'bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-400'
    },
    {
      id: '5',
      subject: 'My return was not picked up',
      requester: 'Polly',
      created: '3 hours ago',
      slaStatus: 'Pending for 3 hours',
      priority: 'High',
      group: 'Billing / --',
      status: 'Pending',
      isNew: false,
      avatarColor: 'bg-purple-100 text-purple-700 dark:bg-purple-900/40 dark:text-purple-400'
    },
    {
      id: '4',
      subject: 'How much time does it take to get my money back!????',
      requester: 'Bob Tree',
      created: '3 hours ago',
      slaStatus: 'First response due in 16 hours',
      priority: 'Urgent',
      group: '-- / --',
      status: 'Open',
      isNew: true,
      avatarColor: 'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-400'
    },
    {
      id: '3',
      subject: 'Vintage table lamp - Out of stock?',
      requester: 'Matt Rogers',
      created: 'Agent responded 3 hours ago',
      slaStatus: 'Due in 3 days',
      priority: 'Low',
      group: '-- / --',
      status: 'Open',
      isNew: false,
      avatarColor: 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-400'
    }
  ]);

  getPriorityColor(priority: string): string {
    switch (priority.toLowerCase()) {
      case 'low': return 'bg-green-500';
      case 'medium': return 'bg-blue-500';
      case 'high': return 'bg-yellow-500';
      case 'urgent': return 'bg-red-500';
      default: return 'bg-gray-500';
    }
  }
}