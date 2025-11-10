export interface User {
  id: string;
  email: string;
  emailConfirmed: boolean;
  fullName?: string;
  avatarUrl?: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}
