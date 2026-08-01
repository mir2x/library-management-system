export interface CurrentUser {
  userId: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface AuthResponse extends CurrentUser {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
