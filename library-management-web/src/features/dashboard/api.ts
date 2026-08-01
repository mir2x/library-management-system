import { api } from '../../lib/api';
import type { PagedResult } from '../../lib/types';

async function fetchTotalCount(url: string): Promise<number> {
  const { data } = await api.get<PagedResult<unknown>>(url);
  return data.totalCount;
}

export const getBranchCount = () => fetchTotalCount('/api/branches?PageSize=1');
export const getBookCount = () => fetchTotalCount('/api/books?PageSize=1');
export const getOverdueLoanCount = () => fetchTotalCount('/api/reports/overdue-loans?PageSize=1');
export const getPendingReservationCount = () => fetchTotalCount('/api/reservations?Status=Pending&PageSize=1');

// /me endpoints have no "only active" filter, so a page of recent history is fetched and
// filtered client-side rather than adding a backend-only-for-this-widget parameter.
export async function getMyActiveLoanCount(): Promise<number> {
  const { data } = await api.get<PagedResult<{ status: string }>>('/api/loans/me?PageSize=100');
  return data.items.filter((loan) => loan.status === 'Active').length;
}

export async function getMyActiveReservationCount(): Promise<number> {
  const { data } = await api.get<PagedResult<{ status: string }>>('/api/reservations/me?PageSize=100');
  return data.items.filter((reservation) => reservation.status === 'Pending' || reservation.status === 'Ready')
    .length;
}
