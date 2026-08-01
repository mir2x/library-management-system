import { useQuery } from '@tanstack/react-query';
import { getBranches, type GetBranchesParams } from './api';

export function useBranches(params: GetBranchesParams) {
  return useQuery({
    queryKey: ['branches', params],
    queryFn: () => getBranches(params),
    placeholderData: (previousData) => previousData,
  });
}
