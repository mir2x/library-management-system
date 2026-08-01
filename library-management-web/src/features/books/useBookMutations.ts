import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createBook, deleteBook, setBookInventory, updateBook } from './api';
import type { CreateBookRequest, UpdateBookRequest } from './types';

export function useCreateBook() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateBookRequest) => createBook(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['books'] }),
  });
}

export function useUpdateBook(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateBookRequest) => updateBook(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['books'] }),
  });
}

export function useDeleteBook() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteBook(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['books'] }),
  });
}

export function useSetBookInventory(bookId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ branchId, totalCopies }: { branchId: string; totalCopies: number }) =>
      setBookInventory(bookId, branchId, totalCopies),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['books'] }),
  });
}
