import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { KnowledgeBaseService, KbArticleSummaryDto, KbFolderTreeNodeDto, KbArticleWithAttachmentsDto } from './knowledge-base.service';

@Injectable()
export class ApiKnowledgeBaseService extends KnowledgeBaseService {
  private http = inject(HttpClient);

  override searchArticles(query: string): Observable<{ items: KbArticleSummaryDto[] }> {
    return this.http.get<{ items: KbArticleSummaryDto[] }>(`${environment.apiBaseUrl}/api/kb/articles?titleQuery=${encodeURIComponent(query)}&pageSize=20`);
  }

  override getDrafts(): Observable<{ items: KbArticleSummaryDto[] }> {
    return this.http.get<{ items: KbArticleSummaryDto[] }>(`${environment.apiBaseUrl}/api/kb/articles?status=0&pageSize=4`);
  }

  override getFolderTree(): Observable<KbFolderTreeNodeDto[]> {
    return this.http.get<KbFolderTreeNodeDto[]>(`${environment.apiBaseUrl}/api/kb/folders/tree`);
  }

  override getPublishedArticles(): Observable<{ items: KbArticleSummaryDto[] }> {
    return this.http.get<{ items: KbArticleSummaryDto[] }>(`${environment.apiBaseUrl}/api/kb/articles?status=1&pageSize=1000`);
  }

  override getFolderArticles(folderId: string): Observable<{ items: KbArticleSummaryDto[] }> {
    return this.http.get<{ items: KbArticleSummaryDto[] }>(`${environment.apiBaseUrl}/api/kb/articles?folderId=${folderId}&pageSize=100`);
  }

  override getArticleWithAttachments(id: string): Observable<KbArticleWithAttachmentsDto> {
    return this.http.get<KbArticleWithAttachmentsDto>(`${environment.apiBaseUrl}/api/kb/articles/${id}/attachments`);
  }

  override createFolder(payload: any): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${environment.apiBaseUrl}/api/kb/folders`, payload);
  }

  override createArticle(payload: any): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${environment.apiBaseUrl}/api/kb/articles`, payload);
  }

  override updateArticle(id: string, payload: any): Observable<void> {
    return this.http.put<void>(`${environment.apiBaseUrl}/api/kb/articles/${id}`, payload);
  }

  override publishArticle(id: string): Observable<void> {
    return this.http.post<void>(`${environment.apiBaseUrl}/api/kb/articles/${id}/publish`, {});
  }

  override uploadAttachment(articleId: string, formData: FormData): Observable<void> {
    return this.http.post<void>(`${environment.apiBaseUrl}/api/kb/articles/${articleId}/attachments`, formData);
  }
}