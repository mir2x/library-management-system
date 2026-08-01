import { api } from '../../lib/api';
import type { PagedResult } from '../../lib/types';
import type { Branch, CreateBranchRequest, UpdateBranchRequest } from './types';

export interface GetBranchesParams {
  search?: string;
  pageNumber: number;
  pageSize: number;
}

export async function getBranches(params: GetBranchesParams): Promise<PagedResult<Branch>> {
  const { data } = await api.get<PagedResult<Branch>>('/api/branches', {
    params: {
      Search: params.search || undefined,
      PageNumber: params.pageNumber,
      PageSize: params.pageSize,
    },
  });
  return data;
}

export async function createBranch(request: CreateBranchRequest): Promise<Branch> {
  const { data } = await api.post<Branch>('/api/branches', request);
  return data;
}

export async function updateBranch(id: string, request: UpdateBranchRequest): Promise<void> {
  await api.patch(`/api/branches/${id}`, request);
}

export async function deleteBranch(id: string): Promise<void> {
  await api.delete(`/api/branches/${id}`);
}
