import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BookCopy, BookCopySaveRequest, BookCopyUpdateRequest } from '../../shared/models/book-copy.model';

@Injectable({ providedIn: 'root' })
export class BookCopiesApiService {
  private readonly httpClient = inject(HttpClient);

  listByBook(bookId: string): Observable<BookCopy[]> {
    return this.httpClient.get<BookCopy[]>(
      `${environment.apiBaseUrl}/libros/${bookId}/ejemplares`
    );
  }

  getById(id: string): Observable<BookCopy> {
    return this.httpClient.get<BookCopy>(
      `${environment.apiBaseUrl}/ejemplares/${id}`
    );
  }

  create(bookId: string, request: BookCopySaveRequest): Observable<BookCopy> {
    return this.httpClient.post<BookCopy>(
      `${environment.apiBaseUrl}/libros/${bookId}/ejemplares`,
      request
    );
  }

  update(id: string, request: BookCopyUpdateRequest): Observable<BookCopy> {
    return this.httpClient.put<BookCopy>(
      `${environment.apiBaseUrl}/ejemplares/${id}`,
      request
    );
  }

  delete(id: string): Observable<void> {
    return this.httpClient.delete<void>(
      `${environment.apiBaseUrl}/ejemplares/${id}`
    );
  }
}
