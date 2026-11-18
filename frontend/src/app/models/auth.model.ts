export interface AuthResponse {
  userId: number;
  email: string;
  role: 'User' | 'Admin';
  token: string;
}

export interface RegisterResponse {
  id: number;
  email: string;
  role: string;
}
