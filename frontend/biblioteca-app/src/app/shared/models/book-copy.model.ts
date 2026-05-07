export type BookCopyStatus =
  | 'AVAILABLE'
  | 'MAINTENANCE'
  | 'LOST'
  | 'RETIRED'
  | 'LOANED'
  | 'RESERVED';

export type BookCopyCondition = 'NEW' | 'GOOD' | 'WORN' | 'DAMAGED';

export interface BookCopy {
  id: string;
  bookId: string;
  barcode: string;
  locationId: string | null;
  locationName: string | null;
  status: BookCopyStatus;
  condition: BookCopyCondition | null;
  acquiredAt: string;
  createdAt: string;
  updatedAt: string;
}

export interface BookCopySaveRequest {
  barcode: string;
  locationId?: string | null;
  status?: BookCopyStatus;
  condition?: BookCopyCondition | null;
  acquiredAt: string;
}

export interface BookCopyUpdateRequest {
  status: BookCopyStatus;
  locationId?: string | null;
  condition?: BookCopyCondition | null;
}
