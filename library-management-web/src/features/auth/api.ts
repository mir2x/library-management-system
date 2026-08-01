import { api } from '../../lib/api';
import { tokenStorage } from '../../lib/tokenStorage';
import type { AuthResponse, CurrentUser, LoginRequest } from './types';

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/api/auth/login', request);
  tokenStorage.setTokens(data.accessToken, data.refreshToken);
  return data;
}

export async function logout(): Promise<void> {
  const refreshToken = tokenStorage.getRefreshToken();
  tokenStorage.clear();

  if (refreshToken) {
    // Best-effort: the user is logged out locally regardless of whether this round-trip
    // succeeds, so a network failure here shouldn't block the logout flow.
    await api.post('/api/auth/logout', { refreshToken }).catch(() => {});
  }
}

export async function getCurrentUser(): Promise<CurrentUser> {
  const { data } = await api.get<CurrentUser>('/api/auth/me');
  return data;
}
