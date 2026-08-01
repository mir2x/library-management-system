import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { tokenStorage } from './tokenStorage';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

api.interceptors.request.use((config) => {
  const accessToken = tokenStorage.getAccessToken();
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
}

// A single in-flight refresh shared by every request that races into a 401 at the same time,
// so a page that fires several queries at once doesn't trigger a refresh-token storm.
let refreshPromise: Promise<string> | null = null;

async function refreshAccessToken(): Promise<string> {
  refreshPromise ??= (async () => {
    const refreshToken = tokenStorage.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available.');
    }

    try {
      const response = await axios.post<RefreshResponse>(
        `${import.meta.env.VITE_API_URL}/api/auth/refresh`,
        { refreshToken },
      );
      tokenStorage.setTokens(response.data.accessToken, response.data.refreshToken);
      return response.data.accessToken;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

interface RetriableRequestConfig extends InternalAxiosRequestConfig {
  _retried?: boolean;
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config as RetriableRequestConfig | undefined;
    const isAuthEndpoint = config?.url?.includes('/api/auth/');

    if (error.response?.status !== 401 || !config || config._retried || isAuthEndpoint) {
      throw error;
    }

    config._retried = true;

    try {
      const accessToken = await refreshAccessToken();
      config.headers.Authorization = `Bearer ${accessToken}`;
      return await api.request(config);
    } catch (refreshError) {
      tokenStorage.clear();
      // The auth feature listens for this to redirect to /login — kept as an event rather
      // than a direct import so this file doesn't reach up into features/.
      window.dispatchEvent(new Event('auth:session-expired'));
      throw refreshError;
    }
  },
);
