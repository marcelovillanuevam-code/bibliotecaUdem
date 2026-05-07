import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BookDetail,
  BookFilters,
  BookRecord,
  BookSaveRequest
} from '../../shared/models/book.model';

interface ApiBookAuthorDto {
  id: string;
  fullName: string;
  contribution: string | null;
}

interface ApiBookSubjectDto {
  id: string;
  code: string;
  name: string;
}

interface ApiBookDto {
  id: string;
  title: string;
  subtitle: string | null;
  isbn: string | null;
  publisher: string | null;
  publicationYear: number | null;
  edition: string | null;
  language: string;
  authors: ApiBookAuthorDto[];
  subjects: ApiBookSubjectDto[];
  createdAt: string;
  updatedAt: string;
}

interface ApiBookDetailDto extends ApiBookDto {
  summaryJson: string | null;
  metadataJson: string | null;
}

@Injectable({ providedIn: 'root' })
export class BooksApiService {
  private readonly httpClient = inject(HttpClient);

  listBooks(filters: BookFilters): Observable<BookRecord[]> {
    let params = new HttpParams();

    for (const [key, value] of Object.entries(filters)) {
      const normalizedValue = value?.trim();
      if (normalizedValue) {
        params = params.set(key, normalizedValue);
      }
    }

    return this.httpClient
      .get<ApiBookDto[]>(`${environment.apiBaseUrl}/libros`, { params })
      .pipe(map((books) => books.map((book) => this.toBookRecord(book))));
  }

  getBook(id: string): Observable<BookDetail> {
    return this.httpClient
      .get<ApiBookDetailDto>(`${environment.apiBaseUrl}/libros/${id}`)
      .pipe(map((book) => this.toBookDetail(book)));
  }

  createBook(request: BookSaveRequest): Observable<BookDetail> {
    return this.httpClient
      .post<ApiBookDetailDto>(`${environment.apiBaseUrl}/libros`, this.toApiSaveRequest(request))
      .pipe(map((book) => this.toBookDetail(book)));
  }

  updateBook(id: string, request: BookSaveRequest): Observable<BookDetail> {
    return this.httpClient
      .put<ApiBookDetailDto>(`${environment.apiBaseUrl}/libros/${id}`, this.toApiSaveRequest(request))
      .pipe(map((book) => this.toBookDetail(book)));
  }

  deleteBook(id: string): Observable<void> {
    return this.httpClient.delete<void>(`${environment.apiBaseUrl}/libros/${id}`);
  }

  private toBookRecord(book: ApiBookDto): BookRecord {
    return {
      id: book.id,
      title: book.title,
      subtitle: book.subtitle,
      isbn: book.isbn,
      publisher: book.publisher,
      publicationYear: book.publicationYear,
      edition: book.edition,
      language: book.language,
      authors: [...book.authors],
      subjects: [...book.subjects],
      createdAt: book.createdAt,
      updatedAt: book.updatedAt
    };
  }

  private toBookDetail(book: ApiBookDetailDto): BookDetail {
    return {
      ...this.toBookRecord(book),
      summaryJson: book.summaryJson,
      metadataJson: book.metadataJson
    };
  }

  private toApiSaveRequest(request: BookSaveRequest) {
    return {
      title: request.title.trim(),
      subtitle: this.normalizeOptional(request.subtitle),
      isbn: this.normalizeOptional(request.isbn),
      publisher: this.normalizeOptional(request.publisher),
      publicationYear: request.publicationYear ?? null,
      edition: this.normalizeOptional(request.edition),
      language: request.language.trim() || 'es',
      summaryJson: this.normalizeOptional(request.summaryJson),
      metadataJson: this.normalizeOptional(request.metadataJson),
      authors: request.authors.map((author) => ({
        fullName: author.fullName.trim(),
        contribution: this.normalizeOptional(author.contribution)
      })),
      subjectCodes: request.subjectCodes.map((subjectCode) => subjectCode.trim())
    };
  }

  private normalizeOptional(value: string | null | undefined): string | null {
    const normalizedValue = value?.trim();
    return normalizedValue ? normalizedValue : null;
  }
}
