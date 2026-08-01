import { useQuery } from '@tanstack/react-query';
import {
  getBookCount,
  getBranchCount,
  getMyActiveLoanCount,
  getMyActiveReservationCount,
  getOverdueLoanCount,
  getPendingReservationCount,
} from './api';

export function useStaffDashboardStats() {
  return {
    branchCount: useQuery({ queryKey: ['dashboard', 'branchCount'], queryFn: getBranchCount }),
    bookCount: useQuery({ queryKey: ['dashboard', 'bookCount'], queryFn: getBookCount }),
    overdueLoanCount: useQuery({ queryKey: ['dashboard', 'overdueLoanCount'], queryFn: getOverdueLoanCount }),
    pendingReservationCount: useQuery({
      queryKey: ['dashboard', 'pendingReservationCount'],
      queryFn: getPendingReservationCount,
    }),
  };
}

export function useMemberDashboardStats() {
  return {
    activeLoanCount: useQuery({ queryKey: ['dashboard', 'myActiveLoanCount'], queryFn: getMyActiveLoanCount }),
    activeReservationCount: useQuery({
      queryKey: ['dashboard', 'myActiveReservationCount'],
      queryFn: getMyActiveReservationCount,
    }),
  };
}
