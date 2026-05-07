export interface BookAuthor {
  id: string;
  fullName: string;
  contribution: string | null;
}

export interface BookSubject {
  id: string;
  code: string;
  name: string;
}

export interface BookRecord {
  id: string;
  title: string;
  subtitle: string | null;
  isbn: string | null;
  publisher: string | null;
  publicationYear: number | null;
  edition: string | null;
  language: string;
  authors: BookAuthor[];
  subjects: BookSubject[];
  createdAt: string;
  updatedAt: string;
}

export interface BookDetail extends BookRecord {
  summaryJson: string | null;
  metadataJson: string | null;
}

export interface BookFilters {
  title?: string | null;
  author?: string | null;
  subject?: string | null;
  isbn?: string | null;
  publisher?: string | null;
  language?: string | null;
}

export interface BookAuthorInput {
  fullName: string;
  contribution?: string | null;
}

export interface BookSaveRequest {
  title: string;
  subtitle?: string | null;
  isbn?: string | null;
  publisher?: string | null;
  publicationYear?: number | null;
  edition?: string | null;
  language: string;
  summaryJson?: string | null;
  metadataJson?: string | null;
  authors: BookAuthorInput[];
  subjectCodes: string[];
}
