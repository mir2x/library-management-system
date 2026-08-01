import { createContext } from 'react';
import type { CurrentUser, LoginRequest } from './types';

export interface AuthContextValue {
  user: CurrentUser | null;
  isLoading: boolean;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
