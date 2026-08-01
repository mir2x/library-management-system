import { useCallback, useEffect, type ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { AuthContext } from './AuthContext';
import { getCurrentUser, login as loginRequest, logout as logoutRequest } from './api';
import { tokenStorage } from '../../lib/tokenStorage';
import { ME_QUERY_KEY } from './queryKeys';
import type { LoginRequest } from './types';

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();

  const { data: user, isLoading } = useQuery({
    queryKey: ME_QUERY_KEY,
    queryFn: getCurrentUser,
    enabled: !!tokenStorage.getAccessToken(),
    retry: false,
    staleTime: Infinity,
  });

  useEffect(() => {
    // Dispatched by lib/api.ts when a 401 survives a refresh attempt — the session is
    // gone, so drop the cached user and let route guards redirect to /login.
    function handleSessionExpired() {
      queryClient.setQueryData(ME_QUERY_KEY, null);
    }

    window.addEventListener('auth:session-expired', handleSessionExpired);
    return () => window.removeEventListener('auth:session-expired', handleSessionExpired);
  }, [queryClient]);

  const login = useCallback(
    async (request: LoginRequest) => {
      const auth = await loginRequest(request);
      queryClient.setQueryData(ME_QUERY_KEY, {
        userId: auth.userId,
        email: auth.email,
        fullName: auth.fullName,
        roles: auth.roles,
      });
    },
    [queryClient],
  );

  const logout = useCallback(async () => {
    await logoutRequest();
    queryClient.setQueryData(ME_QUERY_KEY, null);
  }, [queryClient]);

  return (
    <AuthContext.Provider value={{ user: user ?? null, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
