import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-knowledge-base',
  standalone: true,
  template: `
    <div class="h-full flex flex-col animate-fade-in bg-background">
      
      <div class="bg-surface border-b border-gray-200 dark:border-gray-800 p-4 flex flex-col sm:flex-row gap-4 items-center flex-shrink-0">
        
        <div class="relative flex-1 w-full">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
          </div>
          <input type="text" placeholder="Search articles" class="w-full pl-10 pr-4 py-2 bg-background border border-gray-300 dark:border-gray-700 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary text-text-main placeholder:text-text-muted transition-all" />
        </div>

        <div class="flex items-center gap-3 w-full sm:w-auto">
          <button class="px-3 py-2 bg-surface border border-gray-300 dark:border-gray-700 text-text-main font-medium rounded-md hover:bg-background transition-colors shadow-sm flex items-center gap-2 text-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path></svg>
            Manage
          </button>
          
          <div class="flex rounded-md shadow-sm">
            <button class="px-4 py-2 bg-primary hover:bg-primary-hover text-white font-semibold rounded-l-md transition-colors text-sm flex items-center gap-2 border-r border-primary-hover">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
              New article
            </button>
            <button class="px-2 py-2 bg-primary hover:bg-primary-hover text-white font-semibold rounded-r-md transition-colors flex items-center justify-center">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
            </button>
          </div>

          <button class="p-2 bg-surface border border-gray-300 dark:border-gray-700 text-text-muted rounded-md hover:text-text-main hover:bg-background transition-colors shadow-sm">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"></path></svg>
          </button>
        </div>
      </div>

      <div class="flex-1 overflow-y-auto p-6 lg:p-8 space-y-10">
        
        <section>
          <div class="flex justify-between items-center mb-4">
            <h3 class="text-base font-bold text-text-main">My drafts</h3>
            <button class="text-sm font-semibold text-primary hover:underline">View all</button>
          </div>
          
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @for (draft of drafts(); track draft.title) {
              <div class="bg-surface border border-gray-200 dark:border-gray-800 rounded-lg p-5 hover:shadow-md transition-shadow cursor-pointer group">
                <h4 class="text-sm font-semibold text-text-main mb-2 group-hover:text-primary transition-colors">{{ draft.title }}</h4>
                <p class="text-xs text-text-muted">Last edited {{ draft.lastEdited }}</p>
              </div>
            }
          </div>
        </section>

        <section>
          <div class="mb-4">
            <h3 class="text-base font-bold text-text-main">Categories (6)</h3>
          </div>
          
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @for (category of categories(); track category.title) {
              <div class="bg-surface border border-gray-200 dark:border-gray-800 rounded-lg p-6 hover:shadow-md transition-shadow cursor-pointer flex flex-col h-full">
                
                <div class="flex items-center gap-3 mb-6">
                  <svg class="w-5 h-5 text-text-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"></path></svg>
                  <h4 class="text-sm font-bold text-text-main">{{ category.title }}</h4>
                </div>
                
                <div class="space-y-4 mt-auto">
                  @for (item of category.items; track item.name) {
                    <div class="flex justify-between items-center group/item">
                      <span class="text-sm text-text-muted group-hover/item:text-primary transition-colors">{{ item.name }}</span>
                      <span class="text-sm font-bold text-text-main">{{ item.count }}</span>
                    </div>
                  }
                </div>
                
              </div>
            }
          </div>
        </section>

      </div>
    </div>
  `
})
export class KnowledgeBaseComponent {
  
  // Data matching your screenshot
  drafts = signal([
    { title: 'Information safety and security', lastEdited: '3 minutes ago' },
    { title: 'Legal requirements', lastEdited: '3 minutes ago' },
    { title: 'Email-marketing', lastEdited: '3 minutes ago' }
  ]);

  categories = signal([
    {
      title: 'General',
      items: [
        { name: 'FAQ', count: '00' },
        { name: 'Getting Started', count: '00' }
      ]
    },
    {
      title: 'Getting started with us',
      items: [
        { name: 'Your account', count: '02' }
      ]
    },
    {
      title: 'Orders and refunds',
      items: [
        { name: 'Your order', count: '01' }
      ]
    },
    {
      title: 'FAQs',
      items: [
        { name: 'Shipping FAQs', count: '03' }
      ]
    },
    {
      title: 'Gifts and coupons',
      items: [
        { name: 'Gifts', count: '02' },
        { name: 'Coupons', count: '02' }
      ]
    },
    {
      title: 'Information collected',
      items: [
        { name: 'Privacy policy', count: '02' },
        { name: 'Opt-out policy', count: '02' }
      ]
    }
  ]);
}