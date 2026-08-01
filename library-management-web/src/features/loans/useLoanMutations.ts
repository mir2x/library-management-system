import { useMutation, useQueryClient } from '@tanstack/react-query';
import { borrowBook, returnBook } from './api';
import type { BorrowBookRequest } from './types';

export function useBorrowBook() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: BorrowBookRequest) => borrowBook(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['loans'] }),
  });
}

export function useReturnBook() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => returnBook(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['loans'] }),
  });
}
