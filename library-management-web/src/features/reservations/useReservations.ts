import { useQuery } from '@tanstack/react-query';
import { getMyReservations, getReservations, type GetMyReservationsParams, type GetReservationsParams } from './api';

export function useReservations(params: GetReservationsParams) {
  return useQuery({
    queryKey: ['reservations', params],
    queryFn: () => getReservations(params),
    placeholderData: (previousData) => previousData,
  });
}

export function useMyReservations(params: GetMyReservationsParams) {
  return useQuery({
    queryKey: ['reservations', 'me', params],
    queryFn: () => getMyReservations(params),
    placeholderData: (previousData) => previousData,
  });
}
