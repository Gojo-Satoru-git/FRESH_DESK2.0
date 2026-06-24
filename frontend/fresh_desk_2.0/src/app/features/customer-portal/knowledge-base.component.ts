import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-knowledge-base',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-[calc(100vh-64px)] bg-[#F8FAFC] pb-12 font-sans">
      <!-- HERO SECTION -->
      <div
        class="relative overflow-hidden bg-gradient-to-br from-[#012A4A] via-[#01497C] to-[#013A63] text-white py-16 px-6 shadow-md"
      >
        <div
          class="absolute inset-0 bg-[radial-gradient(circle_at_top_right,rgba(255,255,255,0.08),transparent)] pointer-events-none"
        ></div>
        <div class="max-w-4xl mx-auto text-center space-y-6">
          <h1 class="text-4xl md:text-5xl font-extrabold tracking-tight">
            How can we help you today?
          </h1>
          <p class="text-blue-100/90 text-lg md:text-xl max-w-xl mx-auto font-medium">
            Search our knowledge base for guides, FAQs, and release information.
          </p>

          <!-- SEARCH BAR -->
          <form
            (submit)="onSearchSubmit($event)"
            class="relative w-full max-w-2xl mx-auto mt-8 shadow-xl rounded-2xl"
          >
            <span class="absolute left-5 top-1/2 -translate-y-1/2 text-2xl text-slate-400">🔍</span>
            <input
              type="text"
              [(ngModel)]="searchQuery"
              name="searchQuery"
              placeholder="Search articles, processes, FAQs..."
              class="w-full pl-14 pr-32 py-4.5 bg-white text-slate-800 border-none rounded-2xl focus:outline-none focus:ring-2 focus:ring-blue-500 text-base shadow-inner placeholder:text-slate-400 font-medium transition-all"
            />
            <button
              type="submit"
              class="absolute right-2 top-1/2 -translate-y-1/2 px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-sm rounded-xl transition-all shadow-md shadow-blue-500/20"
            >
              Search
            </button>
          </form>
        </div>
      </div>

      <div class="max-w-6xl mx-auto px-6 mt-12 grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- LEFT/CENTER: CATEGORIES -->
        <div class="lg:col-span-2 space-y-6">
          <h2 class="text-2xl font-bold text-slate-800 flex items-center gap-2">
            <span>📁</span> Browse Categories
          </h2>

          @if (isLoading()) {
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              @for (i of [1, 2, 3, 4]; track i) {
                <div
                  class="bg-white p-6 rounded-2xl border border-slate-200 animate-pulse space-y-3"
                >
                  <div class="h-6 w-1/2 bg-slate-200 rounded"></div>
                  <div class="h-4 w-5/6 bg-slate-200 rounded"></div>
                  <div class="h-4 w-1/4 bg-slate-200 rounded"></div>
                </div>
              }
            </div>
          } @else if (folders().length === 0) {
            <div
              class="bg-white p-10 border border-slate-200 rounded-2xl text-center text-slate-400"
            >
              <span class="text-4xl block mb-2">📦</span>
              <p class="font-medium text-slate-500">No directories created yet.</p>
              <p class="text-xs text-slate-400 mt-1">Check back later for support articles.</p>
            </div>
          } @else {
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              @for (folder of folders(); track folder.id) {
                <div
                  (click)="viewFolder(folder.id)"
                  class="bg-white border border-slate-200 hover:border-blue-300 hover:shadow-lg rounded-2xl p-6 transition-all duration-300 cursor-pointer group flex flex-col justify-between h-48 relative overflow-hidden"
                >
                  <div class="space-y-2">
                    <div class="flex items-center justify-between">
                      <span class="text-2xl">📁</span>
                      <span
                        class="text-xs font-bold text-slate-400 group-hover:text-blue-500 transition-colors"
                        >Browse →</span
                      >
                    </div>
                    <h3
                      class="text-lg font-bold text-slate-800 group-hover:text-blue-600 transition-colors"
                    >
                      {{ folder.name }}
                    </h3>
                    <p class="text-xs text-slate-500 line-clamp-2 leading-relaxed">
                      {{ getFolderDescription(folder) }}
                    </p>
                  </div>
                  <div
                    class="border-t border-slate-100 pt-3 flex items-center justify-between text-xs font-semibold text-slate-500"
                  >
                    <span>Subcategories: {{ folder.children?.length || 0 }}</span>
                    <span class="px-2.5 py-1 bg-slate-100 text-slate-600 rounded-lg">
                      {{ getFolderArticleCount(folder) }} Articles
                    </span>
                  </div>
                </div>
              }
            </div>
          }
        </div>

        <!-- RIGHT: RECENT ARTICLES -->
        <div class="space-y-6">
          <h2 class="text-2xl font-bold text-slate-800 flex items-center gap-2">
            <span>✨</span> Recent Articles
          </h2>

          <div class="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm space-y-4">
            @if (isLoading()) {
              @for (i of [1, 2, 3]; track i) {
                <div class="animate-pulse space-y-2 py-2">
                  <div class="h-4 w-3/4 bg-slate-200 rounded"></div>
                  <div class="h-3 w-1/2 bg-slate-100 rounded"></div>
                </div>
              }
            } @else if (recentArticles().length === 0) {
              <div class="text-center py-6 text-slate-400 text-sm">
                No recent articles published.
              </div>
            } @else {
              <div class="divide-y divide-slate-100">
                @for (art of recentArticles(); track art.id; let last = $last) {
                  <div
                    (click)="viewArticle(art.id)"
                    class="py-3 hover:bg-slate-50/50 cursor-pointer transition-colors rounded-lg px-2 -mx-2"
                    [class.pb-0]="last"
                    [class.pt-0]="$first"
                  >
                    <h4
                      class="text-sm font-semibold text-slate-800 hover:text-blue-600 transition-colors line-clamp-1"
                    >
                      {{ art.title }}
                    </h4>
                    <div
                      class="flex justify-between items-center text-[10px] text-slate-400 mt-1 font-medium"
                    >
                      <span
                        class="uppercase tracking-wider px-1.5 py-0.5 bg-slate-100 text-slate-500 rounded font-bold"
                      >
                        {{ formatArticleType(art.articleType) }}
                      </span>
                      <span>{{ art.updatedAt || art.createdAt | date: 'shortDate' }}</span>
                    </div>
                  </div>
                }
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `,
})
export class KnowledgeBaseComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);

  folders = signal<any[]>([]);
  recentArticles = signal<any[]>([]);
  articleCounts = signal<Record<string, number>>({});
  isLoading = signal<boolean>(true);
  searchQuery = '';

  ngOnInit() {
    this.loadKBData();
  }

  loadKBData() {
    this.isLoading.set(true);

    this.http.get<any[]>(`${environment.apiBaseUrl}/api/kb/folders/tree`).subscribe({
      next: (tree) => {
        this.folders.set(tree || []);

        // Fetch published articles
        this.http
          .get<any>(`${environment.apiBaseUrl}/api/kb/articles?status=1&pageSize=1000`)
          .subscribe({
            next: (res) => {
              const articles = res.items || [];
              const counts: Record<string, number> = {};

              articles.forEach((art: any) => {
                if (art.folderId) {
                  counts[art.folderId] = (counts[art.folderId] || 0) + 1;
                }
              });

              this.articleCounts.set(counts);
              this.recentArticles.set(articles.slice(0, 5));
              this.isLoading.set(false);
            },
            error: () => {
              this.isLoading.set(false);
            },
          });
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  getFolderArticleCount(node: any): number {
    let count = this.articleCounts()[node.id] || 0;
    if (node.children) {
      node.children.forEach((child: any) => {
        count += this.getFolderArticleCount(child);
      });
    }
    return count;
  }

  getFolderDescription(folder: any): string {
    if (folder.children && folder.children.length > 0) {
      const subNames = folder.children
        .slice(0, 3)
        .map((c: any) => c.name)
        .join(', ');
      const suffix = folder.children.length > 3 ? '...' : '';
      return `Contains subcategories like ${subNames}${suffix}.`;
    }
    return `Documentation and support guides for ${folder.name}.`;
  }

  formatArticleType(type: number | string): string {
    const types = ['FAQ', 'Release Note', 'Manual', 'Patch', 'Process Flow'];
    if (typeof type === 'number') return types[type] || 'Article';
    return type;
  }

  onSearchSubmit(event: Event) {
    event.preventDefault();
    if (this.searchQuery.trim()) {
      this.router.navigate(['/customer-portal/knowledge-base/articles'], {
        queryParams: { search: this.searchQuery.trim() },
      });
    }
  }

  viewFolder(folderId: string) {
    // Find the clicked folder node so we can pass its full subtree in router state.
    // articles.component uses this to collect all descendant folder IDs and fetch
    // articles from each one (the API folderId filter is exact-match only).
    const node = this.findFolderNode(this.folders(), folderId);
    this.router.navigate(['/customer-portal/knowledge-base/articles'], {
      queryParams: { folderId },
      state: { folderNode: node },
    });
  }

  findFolderNode(nodes: any[], id: string): any | null {
    for (const node of nodes) {
      if (node.id === id) return node;
      if (node.children?.length) {
        const found = this.findFolderNode(node.children, id);
        if (found) return found;
      }
    }
    return null;
  }

  viewArticle(articleId: string) {
    this.router.navigate(['/customer-portal/knowledge-base/articles'], {
      queryParams: { articleId },
    });
  }
}
