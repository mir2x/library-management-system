import { useQuery } from '@tanstack/react-query';
import { getMembers, type GetMembersParams } from './api';

export function useMembers(params: GetMembersParams) {
  return useQuery({
    queryKey: ['members', params],
    queryFn: () => getMembers(params),
    placeholderData: (previousData) => previousData,
  });
}
