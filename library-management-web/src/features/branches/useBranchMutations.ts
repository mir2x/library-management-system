import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createBranch, deleteBranch, updateBranch } from './api';
import type { CreateBranchRequest, UpdateBranchRequest } from './types';

export function useCreateBranch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateBranchRequest) => createBranch(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['branches'] }),
  });
}

export function useUpdateBranch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateBranchRequest }) => updateBranch(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['branches'] }),
  });
}

export function useDeleteBranch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteBranch(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['branches'] }),
  });
}
