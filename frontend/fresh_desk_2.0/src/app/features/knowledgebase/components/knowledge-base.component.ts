import { Component, signal, OnInit, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { environment } from '../../../../environments/environment.development';
import { Subject, Subscription, debounceTime, distinctUntilChanged } from 'rxjs';

// --- Interfaces ---
interface KbFolderTreeNodeDto { id: string; name: string; parentId?: string; displayOrder: number; children: KbFolderTreeNodeDto[]; }
interface KbArticleSummaryDto { id: string; title: string; articleType: number; status: number; isPublished: boolean; folderId?: string; updatedAt?: string; }
interface KbArticleDto { id: string; title: string; content: string | null; articleType: number; status: number; isPublished: boolean; folderId: string | null; updatedAt: string | null; }
interface KbAttachmentDto { id: string; articleId: string; fileUrl: string; fileName: string; fileSizeBytes: number | null; mimeType: string | null; }
interface KbArticleWithAttachmentsDto { article: KbArticleDto; attachments: KbAttachmentDto[]; }
interface CategoryItem { id: string; title: string; items: { id: string; name: string; count: string }[]; }
interface DraftItem { id: string; title: string; lastEdited: string; }

@Component({
  selector: 'app-knowledge-base',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="h-full flex flex-col animate-fade-in bg-background relative overflow-hidden">
      
      <div class="bg-card-white dark:bg-surface border-b border-table-dark-gray dark:border-gray-800 p-4 flex flex-col sm:flex-row gap-4 items-center flex-shrink-0">
        <div class="relative flex-1 w-full lg:max-w-2xl">
          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
            <svg class="w-5 h-5 text-text-light" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
          </div>
          <input 
            type="text" 
            placeholder="Search articles (min 3 characters)..." 
            (input)="onSearchInput($event)"
            class="w-full pl-11 pr-4 py-2 bg-background border border-outline-gray dark:border-gray-700 rounded-full text-sm font-sans focus:outline-none focus:ring-2 focus:ring-primary-blue text-text-dark dark:text-text-white placeholder:text-text-light transition-all shadow-sm" 
          />
        </div>

        <div class="flex items-center gap-3 w-full sm:w-auto ml-auto">
          <button (click)="isCreateFolderModalOpen.set(true)" class="px-5 py-2 bg-card-white dark:bg-surface border border-outline-gray dark:border-gray-700 text-text-dark dark:text-text-white font-bold font-sans rounded-full hover:bg-table-light-gray dark:hover:bg-gray-800 transition-colors shadow-sm flex items-center gap-2 text-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 13h6m-3-3v6m-9 1V7a2 2 0 012-2h6l2 2h6a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2z"></path></svg>
            New Folder
          </button>
          
          <div class="flex rounded-full shadow-sm">
            <button (click)="isCreateArticleModalOpen.set(true)" class="px-5 py-2 bg-primary-blue hover:bg-primary-hover text-text-white font-bold font-sans rounded-l-full transition-colors text-sm flex items-center gap-2 border-r border-primary-hover/50">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
              New article
            </button>
            <button class="px-3 py-2 bg-primary-blue hover:bg-primary-hover text-text-white font-bold rounded-r-full transition-colors flex items-center justify-center">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
            </button>
          </div>
        </div>
      </div>

      <div class="flex-1 overflow-y-auto p-6 lg:p-8 space-y-10">
        
        @if (isSearching()) {
          <section class="animate-fade-in">
            <h3 class="text-xl font-bold font-heading text-text-dark dark:text-text-white mb-6">Search Results</h3>
            <div class="bg-card-white dark:bg-surface border border-table-dark-gray dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
              @if (searchResults().length === 0) {
                <div class="py-12 text-center text-sm font-sans text-text-light">No results found.</div>
              } @else {
                <div class="divide-y divide-table-dark-gray dark:divide-gray-800">
                  @for (result of searchResults(); track result.id) {
                    <div (click)="openArticleDrawer(result.id)" class="p-5 hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors cursor-pointer group">
                      <h4 class="text-base font-bold font-heading text-text-dark dark:text-text-white group-hover:text-primary-blue transition-colors">{{ result.title }}</h4>
                      <p class="text-xs font-sans text-text-light mt-1">
                        <span [class.text-success-green]="result.isPublished">{{ result.isPublished ? 'Published' : 'Draft' }}</span> • Last updated {{ result.updatedAt ? formatTimeAgo(result.updatedAt) : 'Recently' }}
                      </p>
                    </div>
                  }
                </div>
              }
            </div>
          </section>
        } 
        
        @else if (activeFolder()) {
          <section class="animate-fade-in">
            <div class="flex items-center gap-2 mb-6 text-sm font-sans font-semibold text-text-light">
              <button (click)="closeFolder()" class="hover:text-primary-blue hover:underline transition-colors">Knowledge Base</button>
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
              <span class="font-bold text-text-dark dark:text-text-white">{{ activeFolder()?.title }}</span>
            </div>
            
            <div class="bg-card-white dark:bg-surface border border-table-dark-gray dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
              @if (isLoadingFolder()) {
                <div class="py-12 flex justify-center"><div class="w-8 h-8 border-4 border-primary-blue border-t-transparent rounded-full animate-spin"></div></div>
              } @else if (folderArticles().length === 0) {
                <div class="py-12 text-center text-sm font-sans text-text-light flex flex-col items-center">
                  <svg class="w-12 h-12 text-outline-gray mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path></svg>
                  This folder is empty.
                </div>
              } @else {
                <div class="divide-y divide-table-dark-gray dark:divide-gray-800">
                  @for (article of folderArticles(); track article.id) {
                    <div (click)="openArticleDrawer(article.id)" class="p-5 hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors cursor-pointer group">
                      <h4 class="text-base font-bold font-heading text-text-dark dark:text-text-white group-hover:text-primary-blue transition-colors">{{ article.title }}</h4>
                      <p class="text-xs font-sans text-text-light mt-1">
                        <span [class.text-success-green]="article.isPublished">{{ article.isPublished ? 'Published' : 'Draft' }}</span> • Last updated {{ article.updatedAt ? formatTimeAgo(article.updatedAt) : 'Recently' }}
                      </p>
                    </div>
                  }
                </div>
              }
            </div>
          </section>
        }

        @else {
          <section>
            <div class="flex justify-between items-center mb-6">
              <h3 class="text-xl font-bold font-heading text-text-dark dark:text-text-white">My drafts</h3>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              @if (drafts().length === 0) {
                <div class="col-span-full py-12 text-center flex flex-col items-center justify-center bg-empty-state-gray dark:bg-gray-800 border border-outline-gray/20 rounded-xl">
                  <h4 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">Nothing to see right now!</h4>
                  <p class="text-sm font-sans text-text-light mt-1">No draft articles found.</p>
                </div>
              } @else {
                @for (draft of drafts(); track draft.id) {
                  <div (click)="openArticleDrawer(draft.id)" class="bg-card-white dark:bg-surface border border-table-dark-gray dark:border-gray-800 rounded-xl p-5 hover:shadow-md transition-shadow cursor-pointer group flex flex-col justify-between min-h-[120px]">
                    <h4 class="text-base font-bold font-heading text-text-dark dark:text-text-white mb-3 group-hover:text-primary-blue transition-colors line-clamp-2">{{ draft.title }}</h4>
                    <p class="text-xs font-sans text-text-light">Last edited {{ draft.lastEdited }}</p>
                  </div>
                }
              }
            </div>
          </section>

          <section>
            <div class="mb-6">
              <h3 class="text-xl font-bold font-heading text-text-dark dark:text-text-white">Categories</h3>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              @if (categories().length === 0) {
                <div class="col-span-full py-12 text-center text-sm font-sans text-text-light bg-empty-state-gray dark:bg-gray-800 border border-outline-gray/20 rounded-xl">
                  No categories found.
                </div>
              } @else {
                @for (category of categories(); track category.id) {
                  <div (click)="openFolder(category.id, category.title)" class="bg-card-white dark:bg-surface border border-table-dark-gray dark:border-gray-800 rounded-xl p-6 hover:shadow-md transition-shadow cursor-pointer flex flex-col h-full group">
                    <div class="flex items-center gap-3 mb-6">
                      <div class="p-2 bg-light-blue/20 dark:bg-primary-blue/20 rounded-lg group-hover:bg-primary-blue/20 transition-colors">
                        <svg class="w-6 h-6 text-primary-blue" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"></path></svg>
                      </div>
                      <h4 class="text-base font-bold font-heading text-text-dark dark:text-text-white group-hover:text-primary-blue transition-colors">{{ category.title }}</h4>
                    </div>
                    
                    <div class="space-y-4 mt-auto">
                      @for (item of category.items; track item.id) {
                        <div (click)="$event.stopPropagation(); openFolder(item.id, item.name)" class="flex justify-between items-center group/item pb-2 border-b border-table-light-gray dark:border-gray-800 last:border-0 last:pb-0 hover:bg-table-light-gray dark:hover:bg-gray-800/50 -mx-2 px-2 rounded transition-colors">
                          <span class="text-sm font-sans font-semibold text-text-light dark:text-text-muted group-hover/item:text-primary-blue transition-colors">{{ item.name }}</span>
                          <span class="text-sm font-bold font-heading text-text-dark dark:text-text-white bg-table-light-gray dark:bg-gray-800 px-2 py-0.5 rounded-md">{{ item.count }}</span>
                        </div>
                      }
                    </div>
                  </div>
                }
              }
            </div>
          </section>
        }
      </div>

      @if (isArticleDrawerOpen()) {
        <div class="absolute inset-0 z-40 bg-overlay-gray backdrop-blur-sm animate-fade-in" (click)="closeArticleDrawer()"></div>
        <div class="absolute inset-y-0 right-0 z-50 w-full max-w-3xl bg-card-white dark:bg-surface shadow-2xl flex flex-col transform transition-transform duration-300 translate-x-0 border-l border-table-dark-gray dark:border-gray-800 animate-fade-in">
          
          <div class="h-16 px-6 border-b border-table-dark-gray dark:border-gray-800 flex justify-between items-center bg-table-light-gray dark:bg-gray-900/50 flex-shrink-0">
            <span class="px-2.5 py-1 text-xs font-bold rounded-full bg-light-blue dark:bg-primary-blue/20 text-primary-blue">Article Details</span>
            <button (click)="closeArticleDrawer()" class="p-2 text-text-light hover:text-text-dark dark:hover:text-text-white rounded-full hover:bg-disabled-gray/30 transition-colors">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
          </div>

          <div class="flex-1 overflow-y-auto p-8">
            @if (isLoadingArticle()) {
              <div class="h-full flex flex-col items-center justify-center space-y-4">
                <div class="w-10 h-10 border-4 border-primary-blue border-t-transparent rounded-full animate-spin"></div>
                <p class="text-sm font-sans text-text-light">Loading article...</p>
              </div>
            } @else if (viewedArticle()) {
              <div class="mb-8 border-b border-outline-gray/20 pb-6">
                <h2 class="text-3xl font-bold font-heading text-text-dark dark:text-text-white mb-4">{{ viewedArticle()!.article.title }}</h2>
                <div class="flex flex-wrap gap-4 text-sm font-sans text-text-light">
                  <span class="flex items-center gap-1.5">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path></svg>
                    Last updated: {{ viewedArticle()!.article.updatedAt ? formatTimeAgo(viewedArticle()!.article.updatedAt!) : 'Unknown' }}
                  </span>
                  <span class="flex items-center gap-1.5">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                    Status: <strong [class.text-success-green]="viewedArticle()!.article.isPublished">{{ viewedArticle()!.article.isPublished ? 'Published' : 'Draft' }}</strong>
                  </span>
                </div>
              </div>

              <div class="prose prose-sm sm:prose lg:prose-lg max-w-none text-text-dark dark:text-text-white font-sans mb-10" [innerHTML]="viewedArticle()!.article.content || '<p class=&quot;text-text-light italic&quot;>No content provided.</p>'"></div>

              @if (viewedArticle()!.attachments.length > 0) {
                <div>
                  <h4 class="text-sm font-bold font-heading text-text-dark dark:text-text-white mb-3">Attachments</h4>
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    @for (file of viewedArticle()!.attachments; track file.id) {
                      <a [href]="file.fileUrl" target="_blank" class="flex items-center gap-3 p-3 border border-outline-gray/30 rounded-lg hover:border-primary-blue hover:bg-light-blue/10 transition-colors group">
                        <div class="p-2 bg-table-light-gray dark:bg-gray-800 rounded text-text-light group-hover:text-primary-blue">
                          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path></svg>
                        </div>
                        <div class="flex-1 min-w-0">
                          <p class="text-sm font-bold font-sans text-text-dark dark:text-text-white truncate group-hover:text-primary-blue">{{ file.fileName }}</p>
                          <p class="text-xs text-text-light">{{ (file.fileSizeBytes! / 1024).toFixed(1) }} KB</p>
                        </div>
                      </a>
                    }
                  </div>
                </div>
              }
            }
          </div>

          <div class="p-4 border-t border-table-dark-gray dark:border-gray-800 bg-card-white dark:bg-surface flex justify-end gap-3 shadow-[0_-4px_6px_-1px_rgba(0,0,0,0.05)]">
            <button (click)="editArticle(viewedArticle()!)" class="px-6 py-2 border border-outline-gray dark:border-gray-700 text-text-dark dark:text-text-white font-bold font-sans rounded-full hover:bg-table-light-gray dark:hover:bg-gray-800 transition-colors">Edit Article</button>
            @if (viewedArticle() && !viewedArticle()!.article.isPublished) {
              <button (click)="publishArticle(viewedArticle()!.article.id)" [disabled]="isPublishing()" class="px-6 py-2 bg-primary-blue hover:bg-primary-hover disabled:opacity-50 text-text-white font-bold font-sans rounded-full transition-colors flex items-center gap-2">
                @if (isPublishing()) {
                  <svg class="animate-spin h-4 w-4 text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                  Publishing...
                } @else {
                  Publish to Portal
                }
              </button>
            }
          </div>
        </div>
      }

      @if (isCreateArticleModalOpen()) {
        <div class="fixed inset-0 z-[60] flex items-center justify-center bg-overlay-gray backdrop-blur-sm p-4 animate-fade-in">
          <div class="bg-card-white dark:bg-surface w-full max-w-4xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
            
            <div class="px-6 py-4 border-b border-table-dark-gray dark:border-gray-800 flex justify-between items-center bg-table-light-gray dark:bg-gray-900/50">
              <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">{{ editingArticleId() ? 'Edit Article' : 'Create Knowledge Base Article' }}</h3>
              <button (click)="closeArticleModal()" class="text-text-light hover:text-text-dark hover:bg-disabled-gray/30 p-2 rounded-lg transition-colors">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
              </button>
            </div>

            <div class="p-6 overflow-y-auto flex-1 scrollbar-hide">
              <form [formGroup]="articleForm" class="space-y-6">
                <div>
                  <label class="block text-sm font-semibold font-sans text-text-dark dark:text-text-white mb-1.5">Article Title <span class="text-error-red">*</span></label>
                  <input type="text" formControlName="title" placeholder="e.g. How to reset your password" class="w-full bg-background border border-outline-gray dark:border-gray-700 rounded-lg px-4 py-2.5 text-sm font-sans focus:ring-2 focus:ring-primary-blue outline-none transition-all text-text-dark dark:text-text-white" />
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
                  <div>
                    <label class="block text-sm font-semibold font-sans text-text-dark dark:text-text-white mb-1.5" [class.text-text-light]="editingArticleId()">Article Type <span class="text-error-red">*</span></label>
                    <select formControlName="articleType" class="w-full bg-background border border-outline-gray dark:border-gray-700 rounded-lg px-4 py-2.5 text-sm font-sans focus:ring-2 focus:ring-primary-blue outline-none transition-all text-text-dark dark:text-text-white disabled:opacity-50 disabled:bg-table-light-gray">
                      <option [value]="0">FAQ</option>
                      <option [value]="1">Release Note</option>
                      <option [value]="2">User Manual</option>
                      <option [value]="3">Patch</option>
                      <option [value]="4">Process Flow</option>
                    </select>
                    @if (editingArticleId()) { <p class="text-xs text-text-light mt-1">Type cannot be changed after creation.</p> }
                  </div>
                  <div>
                    <label class="block text-sm font-semibold font-sans text-text-dark dark:text-text-white mb-1.5" [class.text-text-light]="editingArticleId()">Destination Folder</label>
                    <select formControlName="folderId" class="w-full bg-background border border-outline-gray dark:border-gray-700 rounded-lg px-4 py-2.5 text-sm font-sans focus:ring-2 focus:ring-primary-blue outline-none transition-all text-text-dark dark:text-text-white disabled:opacity-50 disabled:bg-table-light-gray">
                      <option [value]="null">-- Root Directory --</option>
                      @for (cat of categories(); track cat.id) {
                        <optgroup [label]="cat.title">
                          <option [value]="cat.id">{{ cat.title }} (Root)</option>
                          @for (sub of cat.items; track sub.id) {
                            <option [value]="sub.id">&nbsp;&nbsp;&nbsp;↳ {{ sub.name }}</option>
                          }
                        </optgroup>
                      }
                    </select>
                    @if (editingArticleId()) { <p class="text-xs text-text-light mt-1">Use the 'Move' action to change folders.</p> }
                  </div>
                </div>

                <div>
                  <label class="block text-sm font-semibold font-sans text-text-dark dark:text-text-white mb-1.5">Article Content</label>
                  <div class="border border-outline-gray dark:border-gray-700 rounded-lg overflow-hidden flex flex-col">
                    <div class="bg-table-light-gray dark:bg-gray-900 border-b border-outline-gray dark:border-gray-700 p-2 flex gap-2">
                      <button type="button" class="p-1.5 hover:bg-disabled-gray/30 rounded text-text-dark dark:text-text-white font-bold font-serif">B</button>
                      <button type="button" class="p-1.5 hover:bg-disabled-gray/30 rounded text-text-dark dark:text-text-white italic font-serif">I</button>
                      <button type="button" class="p-1.5 hover:bg-disabled-gray/30 rounded text-text-dark dark:text-text-white underline font-serif">U</button>
                      <div class="w-px bg-outline-gray mx-1"></div>
                      <button type="button" class="p-1.5 hover:bg-disabled-gray/30 rounded text-text-dark dark:text-text-white">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1"></path></svg>
                      </button>
                    </div>
                    <textarea formControlName="content" rows="8" placeholder="Type your article content here..." class="w-full bg-background p-4 text-sm font-sans focus:outline-none resize-y text-text-dark dark:text-text-white"></textarea>
                  </div>
                </div>

                <div>
                  <label class="block text-sm font-semibold font-sans text-text-dark dark:text-text-white mb-1.5">Attachment (Max 50MB)</label>
                  <div class="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-dashed border-outline-gray rounded-lg hover:bg-table-light-gray dark:hover:bg-gray-800/50 transition-colors">
                    <div class="space-y-1 text-center">
                      <svg class="mx-auto h-12 w-12 text-text-light" stroke="currentColor" fill="none" viewBox="0 0 48 48" aria-hidden="true"><path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" /></svg>
                      <div class="flex text-sm text-text-dark dark:text-text-white justify-center">
                        <label for="file-upload" class="relative cursor-pointer bg-transparent rounded-md font-bold text-primary-blue hover:text-primary-hover">
                          <span>Upload an additional file</span>
                          <input id="file-upload" name="file-upload" type="file" class="sr-only" (change)="onFileSelected($event)">
                        </label>
                      </div>
                      <p class="text-xs text-text-light">{{ selectedFile ? selectedFile.name : 'PDF, DOCX, PNG up to 50MB' }}</p>
                    </div>
                  </div>
                </div>
              </form>
            </div>

            <div class="px-6 py-4 border-t border-table-dark-gray dark:border-gray-800 flex justify-end gap-3 bg-table-light-gray dark:bg-gray-900/50">
              <button (click)="closeArticleModal()" class="px-5 py-2.5 text-sm font-bold font-sans text-text-dark border border-outline-gray rounded-full transition-colors">Cancel</button>
              <button (click)="submitArticle()" [disabled]="articleForm.invalid || isSubmitting()" class="px-5 py-2.5 text-sm font-bold font-sans bg-primary-blue hover:bg-primary-hover disabled:opacity-50 text-text-white rounded-full shadow-lg transition-all flex items-center gap-2">
                @if (isSubmitting()) {
                  <svg class="animate-spin h-4 w-4 text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                  Saving...
                } @else {
                  {{ editingArticleId() ? 'Save Changes' : 'Save as Draft' }}
                }
              </button>
            </div>
          </div>
        </div>
      }

      @if (isCreateFolderModalOpen()) {
        <div class="fixed inset-0 z-[60] flex items-center justify-center bg-overlay-gray backdrop-blur-sm p-4 animate-fade-in">
          <div class="bg-card-white dark:bg-surface w-full max-w-md rounded-2xl shadow-2xl overflow-hidden flex flex-col">
            <div class="px-6 py-4 border-b border-table-dark-gray dark:border-gray-800 flex justify-between items-center bg-table-light-gray dark:bg-gray-900/50">
              <h3 class="text-lg font-bold font-heading text-text-dark dark:text-text-white">Create New Folder</h3>
              <button (click)="closeFolderModal()" class="text-text-light hover:text-text-dark p-2 rounded-lg transition-colors">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
              </button>
            </div>
            <div class="p-6">
              <form [formGroup]="folderForm" class="space-y-5">
                <div>
                  <label class="block text-sm font-semibold font-sans text-text-dark dark:text-text-white mb-1.5">Folder Name <span class="text-error-red">*</span></label>
                  <input type="text" formControlName="name" placeholder="e.g. Troubleshooting" class="w-full bg-background border border-outline-gray dark:border-gray-700 rounded-lg px-4 py-2.5 text-sm font-sans focus:ring-2 focus:ring-primary-blue outline-none transition-all text-text-dark dark:text-text-white" />
                </div>
                <div>
                  <label class="block text-sm font-semibold font-sans text-text-dark dark:text-text-white mb-1.5">Parent Folder (Optional)</label>
                  <select formControlName="parentId" class="w-full bg-background border border-outline-gray dark:border-gray-700 rounded-lg px-4 py-2.5 text-sm font-sans focus:ring-2 focus:ring-primary-blue outline-none transition-all text-text-dark dark:text-text-white">
                    <option [value]="null">-- Root Folder --</option>
                    @for (cat of categories(); track cat.id) {
                      <option [value]="cat.id">{{ cat.title }}</option>
                    }
                  </select>
                </div>
              </form>
            </div>
            <div class="px-6 py-4 border-t border-table-dark-gray dark:border-gray-800 flex justify-end gap-3 bg-table-light-gray dark:bg-gray-900/50">
              <button (click)="closeFolderModal()" class="px-5 py-2.5 text-sm font-bold font-sans text-text-dark border border-outline-gray rounded-full transition-colors">Cancel</button>
              <button (click)="submitFolder()" [disabled]="folderForm.invalid || isSubmitting()" class="px-5 py-2.5 text-sm font-bold font-sans bg-primary-blue hover:bg-primary-hover disabled:opacity-50 text-text-white rounded-full shadow-lg transition-all flex items-center gap-2">
                @if (isSubmitting()) {
                  <svg class="animate-spin h-4 w-4 text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                  Saving...
                } @else {
                  Create
                }
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class KnowledgeBaseComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  
  // Base State
  drafts = signal<DraftItem[]>([]);
  categories = signal<CategoryItem[]>([]);
  
  // Search State
  isSearching = signal<boolean>(false);
  searchResults = signal<KbArticleSummaryDto[]>([]);
  private searchSubject = new Subject<string>();
  private searchSubscription!: Subscription;

  // Folder Drill-Down State
  activeFolder = signal<{id: string, title: string} | null>(null);
  folderArticles = signal<KbArticleSummaryDto[]>([]);
  isLoadingFolder = signal<boolean>(false);

  // Article Drawer State
  isArticleDrawerOpen = signal<boolean>(false);
  isLoadingArticle = signal<boolean>(false);
  viewedArticle = signal<KbArticleWithAttachmentsDto | null>(null);
  isPublishing = signal<boolean>(false);

  // Modal Creation / Editing State
  isCreateArticleModalOpen = signal<boolean>(false);
  isCreateFolderModalOpen = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  editingArticleId = signal<string | null>(null);

  // Forms
  articleForm: FormGroup;
  folderForm: FormGroup;
  selectedFile: File | null = null;

  constructor() {
    this.articleForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(300)]],
      articleType: [0, Validators.required],
      folderId: [null],
      content: ['']
    });

    this.folderForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(150)]],
      parentId: [null],
      displayOrder: [0]
    });
  }

  ngOnInit(): void {
    this.loadKBData();
    this.setupSearch();
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) this.searchSubscription.unsubscribe();
  }

  // ==========================================
  // VIEW LOGIC (Search & Folders)
  // ==========================================
  onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }

  private setupSearch(): void {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(query => {
      const trimmedQuery = query.trim();
      if (trimmedQuery.length >= 3) {
        this.isSearching.set(true);
        this.activeFolder.set(null); // Clear folder view when searching
        this.http.get<{ items: KbArticleSummaryDto[] }>(`${environment.apiUrl}/api/kb/articles?titleQuery=${encodeURIComponent(trimmedQuery)}&pageSize=20`)
          .subscribe({
            next: (res) => this.searchResults.set(res.items || []),
            error: () => this.searchResults.set([])
          });
      } else {
        this.isSearching.set(false);
        this.searchResults.set([]);
      }
    });
  }

  openFolder(id: string, title: string) {
    this.activeFolder.set({ id, title });
    this.isLoadingFolder.set(true);
    // Fetch all articles (draft and published) for this folder context
    this.http.get<{items: KbArticleSummaryDto[]}>(`${environment.apiUrl}/api/kb/articles?folderId=${id}&pageSize=100`)
      .subscribe({
        next: (res) => {
          this.folderArticles.set(res.items || []);
          this.isLoadingFolder.set(false);
        },
        error: () => {
          this.folderArticles.set([]);
          this.isLoadingFolder.set(false);
        }
      });
  }

  closeFolder() {
    this.activeFolder.set(null);
    this.folderArticles.set([]);
  }

  // ==========================================
  // ARTICLE DRAWER & ACTIONS
  // ==========================================
  openArticleDrawer(id: string) {
    this.isArticleDrawerOpen.set(true);
    this.isLoadingArticle.set(true);
    
    this.http.get<KbArticleWithAttachmentsDto>(`${environment.apiUrl}/api/kb/articles/${id}/attachments`)
      .subscribe({
        next: (data) => {
          this.viewedArticle.set(data);
          this.isLoadingArticle.set(false);
        },
        error: (err) => {
          console.error("Failed to load article", err);
          this.isLoadingArticle.set(false);
        }
      });
  }

  closeArticleDrawer() {
    this.isArticleDrawerOpen.set(false);
    this.viewedArticle.set(null);
  }

  publishArticle(id: string) {
    this.isPublishing.set(true);
    this.http.post(`${environment.apiUrl}/api/kb/articles/${id}/publish`, {}).subscribe({
      next: () => {
        this.isPublishing.set(false);
        this.closeArticleDrawer();
        this.loadKBData(); // Refresh drafts/categories
        if (this.activeFolder()) this.openFolder(this.activeFolder()!.id, this.activeFolder()!.title); // Refresh folder view if open
      },
      error: (err) => {
        console.error('Failed to publish', err);
        this.isPublishing.set(false);
      }
    });
  }

  editArticle(data: KbArticleWithAttachmentsDto) {
    this.editingArticleId.set(data.article.id);
    this.articleForm.patchValue({
      title: data.article.title,
      articleType: data.article.articleType,
      folderId: data.article.folderId,
      content: data.article.content
    });
    
    // API Rule: Only newTitle and newContent can be updated via PUT
    this.articleForm.get('articleType')?.disable();
    this.articleForm.get('folderId')?.disable();
    
    this.closeArticleDrawer();
    this.isCreateArticleModalOpen.set(true);
  }

  // ==========================================
  // FORM SUBMISSIONS
  // ==========================================
  submitFolder() {
    if (this.folderForm.invalid) return;
    this.isSubmitting.set(true);

    this.http.post<{ id: string }>(`${environment.apiUrl}/api/kb/folders`, this.folderForm.value).subscribe({
      next: () => {
        this.loadKBData();
        this.closeFolderModal();
      },
      error: () => this.isSubmitting.set(false)
    });
  }

  submitArticle() {
    if (this.articleForm.invalid) return;
    this.isSubmitting.set(true);

    if (this.editingArticleId()) {
      // Flow: PUT Update
      const payload = {
        newTitle: this.articleForm.get('title')?.value,
        newContent: this.articleForm.get('content')?.value
      };
      
      this.http.put(`${environment.apiUrl}/api/kb/articles/${this.editingArticleId()}`, payload).subscribe({
        next: () => this.handleAttachmentUpload(this.editingArticleId()!),
        error: () => this.isSubmitting.set(false)
      });
    } else {
      // Flow: POST Create
      const payload = {
        title: this.articleForm.get('title')?.value,
        articleType: Number(this.articleForm.get('articleType')?.value),
        folderId: this.articleForm.get('folderId')?.value,
        content: this.articleForm.get('content')?.value
      };

      this.http.post<{ id: string }>(`${environment.apiUrl}/api/kb/articles`, payload).subscribe({
        next: (res) => this.handleAttachmentUpload(res.id),
        error: () => this.isSubmitting.set(false)
      });
    }
  }

  handleAttachmentUpload(articleId: string) {
    if (this.selectedFile) {
      const formData = new FormData();
      formData.append('file', this.selectedFile);

      this.http.post(`${environment.apiUrl}/api/kb/articles/${articleId}/attachments`, formData).subscribe({
        next: () => this.finishArticleSubmit(),
        error: () => this.finishArticleSubmit() // Even if attachment fails, article is created
      });
    } else {
      this.finishArticleSubmit();
    }
  }

  finishArticleSubmit() {
    this.loadKBData();
    if (this.activeFolder()) this.openFolder(this.activeFolder()!.id, this.activeFolder()!.title);
    this.closeArticleModal();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) this.selectedFile = input.files[0];
  }

  closeArticleModal() {
    this.isCreateArticleModalOpen.set(false);
    this.isSubmitting.set(false);
    this.editingArticleId.set(null);
    this.articleForm.enable();
    this.articleForm.reset({ articleType: 0, folderId: null });
    this.selectedFile = null;
  }

  closeFolderModal() {
    this.isCreateFolderModalOpen.set(false);
    this.isSubmitting.set(false);
    this.folderForm.reset({ parentId: null, displayOrder: 0 });
  }

  // ==========================================
  // DATA LOADING 
  // ==========================================
  private loadKBData(): void {
    this.http.get<{ items: KbArticleSummaryDto[] }>(`${environment.apiUrl}/api/kb/articles?status=0&pageSize=4`).subscribe({
      next: (res) => {
        this.drafts.set((res.items || []).map(art => ({
          id: art.id, title: art.title, lastEdited: art.updatedAt ? this.formatTimeAgo(art.updatedAt) : 'Recently'
        })));
      }
    });

    this.http.get<KbFolderTreeNodeDto[]>(`${environment.apiUrl}/api/kb/folders/tree`).subscribe({
      next: (tree) => {
        this.http.get<{ items: KbArticleSummaryDto[] }>(`${environment.apiUrl}/api/kb/articles?status=1&pageSize=1000`).subscribe({
          next: (artRes) => {
            const folderCounts: { [folderId: string]: number } = {};
            (artRes.items || []).forEach(art => {
              if (art.folderId) folderCounts[art.folderId] = (folderCounts[art.folderId] || 0) + 1;
            });

            this.categories.set(tree.map(node => ({
              id: node.id, title: node.name, items: (node.children || []).map(child => ({
                id: child.id, name: child.name, count: (folderCounts[child.id] || 0).toString().padStart(2, '0')
              }))
            })));
          },
          error: () => this.categories.set([])
        });
      },
      error: () => this.categories.set([])
    });
  }

  formatTimeAgo(dateStr: string): string {
    const diffMins = Math.floor((new Date().getTime() - new Date(dateStr).getTime()) / 60000);
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} minutes ago`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;
    return `${Math.floor(diffHours / 24)} days ago`;
  }
}