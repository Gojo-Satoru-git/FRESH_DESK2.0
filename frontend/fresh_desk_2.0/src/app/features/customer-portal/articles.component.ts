import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { environment } from '../../../environments/environment';

interface KbArticleSummary {
  id: string;
  title: string;
  articleType: number | string;
  status: string;
  folderId?: string;
  updatedAt?: string;
}

interface KbAttachment {
  id: string;
  fileName: string;
  fileUrl: string;
  fileSizeBytes?: number;
  mimeType?: string;
}

interface KbArticleDetail {
  id: string;
  title: string;
  content?: string;
  articleType: number | string;
  status: string;
  createdAt: string;
  updatedAt?: string;
}

@Component({
  selector: 'app-articles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex h-[calc(100vh-64px)] bg-[#F8FAFC] text-slate-800 font-sans overflow-hidden">
      <!-- ================= LEFT PANEL: ARTICLES LIST ================= -->
      <aside
        class="w-80 md:w-96 bg-white border-r border-slate-200 flex flex-col flex-shrink-0 shadow-sm z-10"
      >
        <!-- Header -->
        <div class="p-5 border-b border-slate-100 bg-white space-y-4">
          <div class="flex items-center gap-3">
            <button
              (click)="goBack()"
              class="p-2 hover:bg-slate-100 rounded-lg text-slate-600 transition font-bold"
              title="Back to Knowledge Base"
            >
              ← Back
            </button>
            <h2 class="text-xl font-bold text-[#0F172A] truncate">
              {{ listHeader() }}
            </h2>
          </div>

          <!-- Inline Search -->
          <div class="relative w-full">
            <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm">🔍</span>
            <input
              type="text"
              placeholder="Search in results..."
              [(ngModel)]="localSearchQuery"
              (ngModelChange)="onLocalSearch()"
              class="w-full pl-9 pr-4 py-2 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all placeholder:text-slate-400 text-slate-700"
            />
          </div>
        </div>

        <!-- Scrollable Articles List -->
        <div class="flex-1 overflow-y-auto divide-y divide-slate-100">
          @if (isLoadingList()) {
            @for (i of [1, 2, 3, 4]; track i) {
              <div class="p-5 animate-pulse space-y-3">
                <div class="h-5 w-3/4 bg-slate-200 rounded"></div>
                <div class="h-4 w-1/2 bg-slate-100 rounded"></div>
              </div>
            }
          } @else if (filteredArticles().length === 0) {
            <div
              class="p-8 text-center text-slate-400 flex flex-col items-center justify-center h-48"
            >
              <span class="text-3xl mb-2">📄</span>
              <p class="text-sm font-medium">No articles found</p>
              <p class="text-xs text-slate-400 mt-1">Try adapting your search parameters</p>
            </div>
          } @else {
            @for (article of filteredArticles(); track article.id) {
              <div
                (click)="selectArticle(article.id)"
                class="p-5 hover:bg-slate-50/80 cursor-pointer transition-all duration-150 border-l-4"
                [class.border-blue-600]="selectedArticleId() === article.id"
                [class.bg-blue-50/20]="selectedArticleId() === article.id"
                [class.border-transparent]="selectedArticleId() !== article.id"
              >
                <h4 class="text-sm font-semibold text-slate-800 mb-1.5 line-clamp-2">
                  {{ article.title }}
                </h4>
                <div
                  class="flex items-center justify-between text-[10px] text-slate-400 font-medium mt-2 font-mono"
                >
                  <span
                    class="uppercase tracking-wider px-1.5 py-0.5 bg-slate-100 text-slate-500 rounded font-bold"
                  >
                    {{ formatArticleType(article.articleType) }}
                  </span>
                  <span>Updated {{ article.updatedAt || '' | date: 'shortDate' }}</span>
                </div>
              </div>
            }
          }
        </div>
      </aside>

      <!-- ================= RIGHT PANEL: ARTICLE DETAIL ================= -->
      <main class="flex-1 bg-[#F8FAFC] flex flex-col min-w-0 overflow-y-auto">
        @if (selectedArticleId()) {
          @if (isLoadingDetails()) {
            <div class="p-8 animate-pulse space-y-6 max-w-4xl mx-auto w-full">
              <div class="space-y-3">
                <div class="h-8 w-2/3 bg-slate-200 rounded"></div>
                <div class="h-4 w-1/4 bg-slate-100 rounded"></div>
              </div>
              <div class="h-64 w-full bg-slate-200 rounded-2xl"></div>
            </div>
          } @else if (articleDetail(); as art) {
            <div class="p-6 md:p-8 space-y-6 max-w-4xl mx-auto w-full">
              <!-- Article Card -->
              <div
                class="bg-white rounded-2xl p-6 md:p-8 border border-slate-200/60 shadow-sm relative overflow-hidden space-y-6"
              >
                <div
                  class="absolute top-0 left-0 right-0 h-1.5 bg-gradient-to-r from-blue-500 via-indigo-500 to-purple-500"
                ></div>

                <div
                  class="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 pb-4"
                >
                  <div class="flex items-center gap-2">
                    <span
                      class="text-xs font-extrabold uppercase tracking-wider px-2.5 py-1 bg-blue-50 text-blue-700 border border-blue-100 rounded-lg"
                    >
                      {{ formatArticleType(art.articleType) }}
                    </span>
                  </div>
                  <span class="text-xs text-slate-400 font-medium">
                    Published {{ art.createdAt | date: 'mediumDate' }}
                    @if (art.updatedAt) {
                      • Updated {{ art.updatedAt | date: 'mediumDate' }}
                    }
                  </span>
                </div>

                <h1 class="text-2xl md:text-3xl font-extrabold text-slate-900 leading-tight">
                  {{ art.title }}
                </h1>

                <!-- Article Body Content -->
                <div
                  class="text-sm md:text-base text-slate-600 leading-relaxed font-normal prose prose-slate max-w-none"
                  [innerHTML]="art.content || 'This article has no content body.'"
                ></div>

                <!-- Attachments Section -->
                @if (attachments().length > 0) {
                  <div class="border-t border-slate-100 pt-6 mt-6 space-y-3">
                    <h4 class="text-xs font-bold text-slate-400 uppercase tracking-wider">
                      📎 Attachments ({{ attachments().length }})
                    </h4>
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                      @for (file of attachments(); track file.id) {
                        <a
                          [href]="getAbsoluteFileUrl(file.fileUrl)"
                          target="_blank"
                          class="flex items-center justify-between p-3.5 border border-slate-200 hover:border-blue-400 hover:bg-blue-50/20 rounded-xl transition group text-left cursor-pointer"
                        >
                          <div class="flex items-center gap-2.5 min-w-0">
                            <span class="text-lg">📄</span>
                            <div class="min-w-0">
                              <p
                                class="text-xs font-bold text-slate-700 group-hover:text-blue-600 transition truncate"
                              >
                                {{ file.fileName }}
                              </p>
                              @if (file.fileSizeBytes) {
                                <p class="text-[10px] text-slate-400 font-medium mt-0.5">
                                  {{ formatBytes(file.fileSizeBytes) }}
                                </p>
                              }
                            </div>
                          </div>
                          <span
                            class="text-slate-400 group-hover:text-blue-500 text-xs font-bold transition"
                            >↓</span
                          >
                        </a>
                      }
                    </div>
                  </div>
                }
              </div>
            </div>
          }
        } @else {
          <!-- Empty State -->
          <div
            class="flex-1 flex flex-col items-center justify-center p-8 text-center text-slate-400"
          >
            <div
              class="w-24 h-24 bg-blue-50 text-blue-500 rounded-full flex items-center justify-center text-4xl mb-4 shadow-inner"
            >
              📄
            </div>
            <h3 class="text-xl font-bold text-slate-800 mb-1">Select an Article</h3>
            <p class="text-sm max-w-sm text-slate-500">
              Choose an article from the left panel list to read detailed documentation, guides, or
              manuals.
            </p>
          </div>
        }
      </main>
    </div>
  `,
})
export class ArticlesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);
  private router = inject(Router);

  articles = signal<KbArticleSummary[]>([]);
  filteredArticles = signal<KbArticleSummary[]>([]);
  selectedArticleId = signal<string | null>(null);
  articleDetail = signal<KbArticleDetail | null>(null);
  attachments = signal<KbAttachment[]>([]);

  isLoadingList = signal<boolean>(false);
  isLoadingDetails = signal<boolean>(false);

  listHeader = signal<string>('All Articles');
  localSearchQuery = '';

  folderId: string | null = null;
  searchQuery: string | null = null;
  // Full folder tree node passed via router state — used to collect descendant IDs.
  // Must be read in constructor because getCurrentNavigation() returns null after navigation completes.
  folderNode: any | null = null;

  constructor() {
    this.folderNode = this.router.getCurrentNavigation()?.extras?.state?.['folderNode'] ?? null;
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.folderId = params['folderId'] || null;
      this.searchQuery = params['search'] || null;
      const initialArticleId: string | null = params['articleId'] || null;

      if (this.folderId) {
        this.listHeader.set('Folder Category');
        this.http
          .get<any>(`${environment.apiBaseUrl}/api/kb/folders/${this.folderId}`)
          .subscribe((f: any) => {
            if (f && f.name) this.listHeader.set(f.name);
          });
      } else if (this.searchQuery) {
        this.listHeader.set(`Search: "${this.searchQuery}"`);
      } else {
        this.listHeader.set('All Articles');
      }

      this.fetchArticles(initialArticleId);
    });
  }

  // Collect the given tree node's ID plus all descendant IDs recursively.
  private collectFolderIds(node: any): string[] {
    const ids: string[] = [node.id as string];
    if (node.children && node.children.length) {
      for (const child of node.children) {
        const childIds = this.collectFolderIds(child);
        childIds.forEach((id: string) => ids.push(id));
      }
    }
    return ids;
  }

  fetchArticles(initialArticleId: string | null = null) {
    this.isLoadingList.set(true);

    if (this.folderId) {
      // The API folderId filter is exact-match only — it does NOT recurse into children.
      // We must collect all descendant folder IDs and fire one request per folder, then merge.
      if (this.folderNode) {
        const folderIds = this.collectFolderIds(this.folderNode);
        this.fetchArticlesForFolderIds(folderIds, initialArticleId);
      } else {
        // Fallback when navigated directly via URL (no router state): fetch one level of children.
        this.http
          .get<any[]>(`${environment.apiBaseUrl}/api/kb/folders/${this.folderId}/children`)
          .subscribe({
            next: (children: any[]) => {
              const folderIds: string[] = [
                this.folderId!,
                ...(children || []).map((c: any) => c.id as string),
              ];
              this.fetchArticlesForFolderIds(folderIds, initialArticleId);
            },
            error: () => {
              this.fetchArticlesForFolderIds([this.folderId!], initialArticleId);
            },
          });
      }
    } else {
      let url = `${environment.apiBaseUrl}/api/kb/articles?status=1&pageSize=1000`;
      if (this.searchQuery) {
        url += `&titleQuery=${encodeURIComponent(this.searchQuery)}`;
      }
      this.http.get<{ items: KbArticleSummary[] }>(url).subscribe({
        next: (res: { items: KbArticleSummary[] }) =>
          this.applyArticleResults(res.items || [], initialArticleId),
        error: () => this.isLoadingList.set(false),
      });
    }
  }

  // Fire parallel requests for each folder ID, then merge and deduplicate results.
  private fetchArticlesForFolderIds(folderIds: string[], initialArticleId: string | null) {
    const requests = folderIds.map((id: string) =>
      this.http.get<{ items: KbArticleSummary[] }>(
        `${environment.apiBaseUrl}/api/kb/articles?status=1&pageSize=1000&folderId=${id}`,
      ),
    );

    forkJoin(requests).subscribe({
      next: (responses: { items: KbArticleSummary[] }[]) => {
        const seen = new Set<string>();
        const merged: KbArticleSummary[] = [];
        for (const res of responses) {
          for (const art of res.items || []) {
            if (!seen.has(art.id)) {
              seen.add(art.id);
              merged.push(art);
            }
          }
        }
        this.applyArticleResults(merged, initialArticleId);
      },
      error: () => this.isLoadingList.set(false),
    });
  }

  private applyArticleResults(items: KbArticleSummary[], initialArticleId: string | null) {
    this.articles.set(items);
    this.filteredArticles.set(items);
    this.isLoadingList.set(false);

    if (initialArticleId) {
      this.selectArticle(initialArticleId);
    } else if (items.length > 0) {
      this.selectArticle(items[0].id);
    } else {
      this.selectedArticleId.set(null);
      this.articleDetail.set(null);
      this.attachments.set([]);
    }
  }

  selectArticle(id: string) {
    this.selectedArticleId.set(id);
    this.isLoadingDetails.set(true);
    this.attachments.set([]);
    this.articleDetail.set(null);

    this.http
      .get<{
        article: KbArticleDetail;
        attachments: KbAttachment[];
      }>(`${environment.apiBaseUrl}/api/kb/articles/${id}/attachments`)
      .subscribe({
        next: (res: { article: KbArticleDetail; attachments: KbAttachment[] }) => {
          this.articleDetail.set(res.article);
          this.attachments.set(res.attachments || []);
          this.isLoadingDetails.set(false);
        },
        error: () => {
          this.isLoadingDetails.set(false);
        },
      });
  }

  onLocalSearch() {
    const query = this.localSearchQuery.trim().toLowerCase();
    if (!query) {
      this.filteredArticles.set(this.articles());
    } else {
      this.filteredArticles.set(
        this.articles().filter((art: KbArticleSummary) => art.title.toLowerCase().includes(query)),
      );
    }
  }

  getAbsoluteFileUrl(relativeUrl: string): string {
    if (relativeUrl.startsWith('http')) return relativeUrl;
    return `${environment.apiBaseUrl}${relativeUrl}`;
  }

  formatBytes(bytes?: number): string {
    if (!bytes) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  formatArticleType(type: number | string): string {
    const types = ['FAQ', 'Release Note', 'Manual', 'Patch', 'Process Flow'];
    if (typeof type === 'number') return types[type] || 'Article';
    return type;
  }

  goBack() {
    this.router.navigate(['/customer-portal/knowledge-base']);
  }
}
