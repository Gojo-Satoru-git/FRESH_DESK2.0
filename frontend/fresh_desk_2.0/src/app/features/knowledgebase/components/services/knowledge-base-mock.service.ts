import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { KnowledgeBaseService, KbArticleSummaryDto, KbFolderTreeNodeDto, KbArticleWithAttachmentsDto } from './knowledge-base.service';

@Injectable()
export class MockKnowledgeBaseService extends KnowledgeBaseService {
  
  // Dummy Data
  private mockDrafts: KbArticleSummaryDto[] = [
    { id: '1', title: 'Connecting to the VPN', articleType: 0, status: 0, isPublished: false, updatedAt: new Date().toISOString() },
    { id: '2', title: 'Leave Policy 2026', articleType: 2, status: 0, isPublished: false, updatedAt: new Date(Date.now() - 86400000).toISOString() }
  ];

  private mockFolders: KbFolderTreeNodeDto[] = [
    { id: 'f1', name: 'IT Support', displayOrder: 0, children: [] },
    { id: 'f2', name: 'HR Policies', displayOrder: 1, children: [] }
  ];

  override searchArticles(query: string): Observable<{ items: KbArticleSummaryDto[] }> {
    return of({ items: this.mockDrafts }).pipe(delay(400));
  }

  override getDrafts(): Observable<{ items: KbArticleSummaryDto[] }> {
    return of({ items: this.mockDrafts }).pipe(delay(300));
  }

  override getFolderTree(): Observable<KbFolderTreeNodeDto[]> {
    return of(this.mockFolders).pipe(delay(300));
  }

  override getPublishedArticles(): Observable<{ items: KbArticleSummaryDto[] }> {
    return of({ items: [] }).pipe(delay(300));
  }

  override getFolderArticles(folderId: string): Observable<{ items: KbArticleSummaryDto[] }> {
    return of({ items: this.mockDrafts }).pipe(delay(300));
  }

  override getArticleWithAttachments(id: string): Observable<KbArticleWithAttachmentsDto> {
    return of({
      article: { id, title: 'Mock Article', content: '<p>This is mock content loaded from the mock service!</p>', articleType: 0, status: 0, isPublished: false, folderId: null, updatedAt: new Date().toISOString() },
      attachments: []
    }).pipe(delay(500));
  }

  override createFolder(payload: any): Observable<{ id: string }> {
    return of({ id: 'new-folder-' + Math.random() }).pipe(delay(400));
  }

  override createArticle(payload: any): Observable<{ id: string }> {
    return of({ id: 'new-article-' + Math.random() }).pipe(delay(400));
  }

  override updateArticle(id: string, payload: any): Observable<void> {
    return of(void 0).pipe(delay(400));
  }

  override publishArticle(id: string): Observable<void> {
    return of(void 0).pipe(delay(400));
  }

  override uploadAttachment(articleId: string, formData: FormData): Observable<void> {
    return of(void 0).pipe(delay(600));
  }
}