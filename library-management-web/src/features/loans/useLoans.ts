import { useQuery } from '@tanstack/react-query';
import { getLoans, getMyLoans, type GetLoansParams, type GetMyLoansParams } from './api';

export function useLoans(params: GetLoansParams) {
  return useQuery({
    queryKey: ['loans', params],
    queryFn: () => getLoans(params),
    placeholderData: (previousData) => previousData,
  });
}

export function useMyLoans(params: GetMyLoansParams) {
  return useQuery({
    queryKey: ['loans', 'me', params],
    queryFn: () => getMyLoans(params),
    placeholderData: (previousData) => previousData,
  });
}
