import { useQuery } from '@tanstack/react-query';
import {
  getBranchInventory,
  getMemberActivity,
  getMostBorrowedBooks,
  getOverdueLoans,
  getReservationQueues,
  type MemberActivityParams,
  type MostBorrowedBooksParams,
  type OverdueLoansParams,
} from './api';

export function useOverdueLoansReport(params: OverdueLoansParams) {
  return useQuery({
    queryKey: ['reports', 'overdue-loans', params],
    queryFn: () => getOverdueLoans(params),
    placeholderData: (previousData) => previousData,
  });
}

export function useMostBorrowedBooksReport(params: MostBorrowedBooksParams) {
  return useQuery({
    queryKey: ['reports', 'most-borrowed-books', params],
    queryFn: () => getMostBorrowedBooks(params),
  });
}

export function useBranchInventoryReport(branchId?: string) {
  return useQuery({
    queryKey: ['reports', 'branch-inventory', branchId],
    queryFn: () => getBranchInventory(branchId),
  });
}

export function useMemberActivityReport(params: MemberActivityParams) {
  return useQuery({
    queryKey: ['reports', 'member-activity', params],
    queryFn: () => getMemberActivity(params),
    placeholderData: (previousData) => previousData,
  });
}

export function useReservationQueuesReport(branchId?: string) {
  return useQuery({
    queryKey: ['reports', 'reservation-queues', branchId],
    queryFn: () => getReservationQueues(branchId),
  });
}
