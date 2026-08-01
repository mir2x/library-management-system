import { useMutation, useQueryClient } from '@tanstack/react-query';
import { cancelReservation, createMyReservation, createReservation, fulfillReservation } from './api';
import type { CreateMyReservationRequest, CreateReservationRequest } from './types';

function useInvalidatingMutation<TVariables>(mutationFn: (variables: TVariables) => Promise<unknown>) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['reservations'] }),
  });
}

export function useCreateReservation() {
  return useInvalidatingMutation((request: CreateReservationRequest) => createReservation(request));
}

export function useCreateMyReservation() {
  return useInvalidatingMutation((request: CreateMyReservationRequest) => createMyReservation(request));
}

export function useFulfillReservation() {
  // A fulfilled reservation becomes a loan, so the loans list needs invalidating too.
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => fulfillReservation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations'] });
      queryClient.invalidateQueries({ queryKey: ['loans'] });
    },
  });
}

export function useCancelReservation() {
  return useInvalidatingMutation((id: string) => cancelReservation(id));
}
