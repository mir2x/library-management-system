import { api } from '../../lib/api';
import type { PagedResult } from '../../lib/types';
import type { CreateMemberRequest, Member, UpdateMemberRequest } from './types';

export interface GetMembersParams {
  search?: string;
  pageNumber: number;
  pageSize: number;
}

export async function getMembers(params: GetMembersParams): Promise<PagedResult<Member>> {
  const { data } = await api.get<PagedResult<Member>>('/api/members', {
    params: {
      Search: params.search || undefined,
      PageNumber: params.pageNumber,
      PageSize: params.pageSize,
    },
  });
  return data;
}

export async function createMember(request: CreateMemberRequest): Promise<Member> {
  const { data } = await api.post<Member>('/api/members', request);
  return data;
}

export async function updateMember(id: string, request: UpdateMemberRequest): Promise<void> {
  await api.patch(`/api/members/${id}`, request);
}

export async function deactivateMember(id: string): Promise<void> {
  await api.delete(`/api/members/${id}`);
}

export async function suspendMember(id: string): Promise<void> {
  await api.post(`/api/members/${id}/suspend`);
}

export async function reactivateMember(id: string): Promise<void> {
  await api.post(`/api/members/${id}/reactivate`);
}
