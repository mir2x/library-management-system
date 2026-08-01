import { api } from '../../lib/api';
import type { PagedResult } from '../../lib/types';
import type { Book, BookDetail, BookInventory, CreateBookRequest, UpdateBookRequest } from './types';

export interface GetBooksParams {
  search?: string;
  pageNumber: number;
  pageSize: number;
}

export async function getBooks(params: GetBooksParams): Promise<PagedResult<Book>> {
  const { data } = await api.get<PagedResult<Book>>('/api/books', {
    params: {
      Search: params.search || undefined,
      PageNumber: params.pageNumber,
      PageSize: params.pageSize,
    },
  });
  return data;
}

export async function getBookById(id: string): Promise<BookDetail> {
  const { data } = await api.get<BookDetail>(`/api/books/${id}`);
  return data;
}

export async function createBook(request: CreateBookRequest): Promise<Book> {
  const { data } = await api.post<Book>('/api/books', request);
  return data;
}

export async function updateBook(id: string, request: UpdateBookRequest): Promise<void> {
  await api.patch(`/api/books/${id}`, request);
}

export async function deleteBook(id: string): Promise<void> {
  await api.delete(`/api/books/${id}`);
}

export async function setBookInventory(bookId: string, branchId: string, totalCopies: number): Promise<BookInventory> {
  const { data } = await api.put<BookInventory>(`/api/books/${bookId}/inventory/${branchId}`, { totalCopies });
  return data;
}
