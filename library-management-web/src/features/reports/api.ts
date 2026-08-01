import { api } from '../../lib/api';
import type { PagedResult } from '../../lib/types';
import type {
  BranchInventorySummary,
  MemberActivity,
  MostBorrowedBook,
  OverdueLoan,
  ReservationQueueSummary,
} from './types';

export interface OverdueLoansParams {
  branchId?: string;
  pageNumber: number;
  pageSize: number;
}

export async function getOverdueLoans(params: OverdueLoansParams): Promise<PagedResult<OverdueLoan>> {
  const { data } = await api.get<PagedResult<OverdueLoan>>('/api/reports/overdue-loans', {
    params: { BranchId: params.branchId || undefined, PageNumber: params.pageNumber, PageSize: params.pageSize },
  });
  return data;
}

export interface MostBorrowedBooksParams {
  branchId?: string;
  top: number;
}

export async function getMostBorrowedBooks(params: MostBorrowedBooksParams): Promise<MostBorrowedBook[]> {
  const { data } = await api.get<MostBorrowedBook[]>('/api/reports/most-borrowed-books', {
    params: { BranchId: params.branchId || undefined, Top: params.top },
  });
  return data;
}

export async function getBranchInventory(branchId?: string): Promise<BranchInventorySummary[]> {
  const { data } = await api.get<BranchInventorySummary[]>('/api/reports/branch-inventory', {
    params: { BranchId: branchId || undefined },
  });
  return data;
}

export interface MemberActivityParams {
  branchId?: string;
  pageNumber: number;
  pageSize: number;
}

export async function getMemberActivity(params: MemberActivityParams): Promise<PagedResult<MemberActivity>> {
  const { data } = await api.get<PagedResult<MemberActivity>>('/api/reports/member-activity', {
    params: { BranchId: params.branchId || undefined, PageNumber: params.pageNumber, PageSize: params.pageSize },
  });
  return data;
}

export async function getReservationQueues(branchId?: string): Promise<ReservationQueueSummary[]> {
  const { data } = await api.get<ReservationQueueSummary[]>('/api/reports/reservation-queues', {
    params: { BranchId: branchId || undefined },
  });
  return data;
}
