import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createMember, deactivateMember, reactivateMember, suspendMember, updateMember } from './api';
import type { CreateMemberRequest, UpdateMemberRequest } from './types';

function useInvalidatingMutation<TVariables>(mutationFn: (variables: TVariables) => Promise<unknown>) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['members'] }),
  });
}

export function useCreateMember() {
  return useInvalidatingMutation((request: CreateMemberRequest) => createMember(request));
}

export function useUpdateMember(id: string) {
  return useInvalidatingMutation((request: UpdateMemberRequest) => updateMember(id, request));
}

export function useDeactivateMember() {
  return useInvalidatingMutation((id: string) => deactivateMember(id));
}

export function useSuspendMember() {
  return useInvalidatingMutation((id: string) => suspendMember(id));
}

export function useReactivateMember() {
  return useInvalidatingMutation((id: string) => reactivateMember(id));
}
