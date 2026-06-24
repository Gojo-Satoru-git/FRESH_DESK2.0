import { Observable } from 'rxjs';

// --- Shared DTOs ---
export interface KbFolderTreeNodeDto { id: string; name: string; parentId?: string; displayOrder: number; children: KbFolderTreeNodeDto[]; }
export interface KbArticleSummaryDto { id: string; title: string; articleType: number; status: number; isPublished: boolean; folderId?: string; updatedAt?: string; }
export interface KbArticleDto { id: string; title: string; content: string | null; articleType: number; status: number; isPublished: boolean; folderId: string | null; updatedAt: string | null; }
export interface KbAttachmentDto { id: string; articleId: string; fileUrl: string; fileName: string; fileSizeBytes: number | null; mimeType: string | null; }
export interface KbArticleWithAttachmentsDto { article: KbArticleDto; attachments: KbAttachmentDto[]; }

export abstract class KnowledgeBaseService {
  abstract searchArticles(query: string): Observable<{ items: KbArticleSummaryDto[] }>;
  abstract getDrafts(): Observable<{ items: KbArticleSummaryDto[] }>;
  abstract getFolderTree(): Observable<KbFolderTreeNodeDto[]>;
  abstract getPublishedArticles(): Observable<{ items: KbArticleSummaryDto[] }>;
  abstract getFolderArticles(folderId: string): Observable<{ items: KbArticleSummaryDto[] }>;
  abstract getArticleWithAttachments(id: string): Observable<KbArticleWithAttachmentsDto>;
  
  abstract createFolder(payload: any): Observable<{ id: string }>;
  abstract createArticle(payload: any): Observable<{ id: string }>;
  abstract updateArticle(id: string, payload: any): Observable<void>;
  abstract publishArticle(id: string): Observable<void>;
  abstract uploadAttachment(articleId: string, formData: FormData): Observable<void>;
}