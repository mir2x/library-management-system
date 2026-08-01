import { useQuery } from '@tanstack/react-query';
import { getBookById, getBooks, type GetBooksParams } from './api';

export function useBooks(params: GetBooksParams) {
  return useQuery({
    queryKey: ['books', params],
    queryFn: () => getBooks(params),
    placeholderData: (previousData) => previousData,
  });
}

export function useBook(id: string) {
  return useQuery({
    queryKey: ['books', id],
    queryFn: () => getBookById(id),
  });
}
