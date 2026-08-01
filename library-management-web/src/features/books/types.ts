export interface Book {
  id: string;
  title: string;
  author: string;
  isbn: string;
  genre: string;
  publishedYear: number;
  description: string | null;
  isActive: boolean;
}

export interface BookInventory {
  branchId: string;
  branchName: string;
  totalCopies: number;
  availableCopies: number;
}

export interface BookDetail extends Book {
  inventory: BookInventory[];
}

export interface CreateBookRequest {
  title: string;
  author: string;
  isbn: string;
  genre: string;
  publishedYear: number;
  description: string | null;
}

export interface UpdateBookRequest {
  title: string | null;
  author: string | null;
  genre: string | null;
  publishedYear: number | null;
  description: string | null;
}
