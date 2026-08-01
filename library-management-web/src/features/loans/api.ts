import { api } from '../../lib/api';
import type { PagedResult } from '../../lib/types';
import type { BorrowBookRequest, Loan } from './types';

export interface GetLoansParams {
  onlyOverdue?: boolean;
  pageNumber: number;
  pageSize: number;
}

export async function getLoans(params: GetLoansParams): Promise<PagedResult<Loan>> {
  const { data } = await api.get<PagedResult<Loan>>('/api/loans', {
    params: {
      OnlyOverdue: params.onlyOverdue || undefined,
      PageNumber: params.pageNumber,
      PageSize: params.pageSize,
    },
  });
  return data;
}

export interface GetMyLoansParams {
  pageNumber: number;
  pageSize: number;
}

export async function getMyLoans(params: GetMyLoansParams): Promise<PagedResult<Loan>> {
  const { data } = await api.get<PagedResult<Loan>>('/api/loans/me', {
    params: { PageNumber: params.pageNumber, PageSize: params.pageSize },
  });
  return data;
}

export async function borrowBook(request: BorrowBookRequest): Promise<Loan> {
  const { data } = await api.post<Loan>('/api/loans', request);
  return data;
}

export async function returnBook(id: string): Promise<void> {
  await api.post(`/api/loans/${id}/return`);
}
