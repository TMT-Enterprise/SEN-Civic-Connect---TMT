import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';

export type UserRole = 'Requester' | 'Staff' | 'Admin';

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  role: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  phoneNumber: string;
}

export interface RegisteredAccount {
  id: number;
  name: string;
  email: string;
  role: string;
}

export function isUserRole(role: string): role is UserRole {
  return role === 'Requester' || role === 'Staff' || role === 'Admin';
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorageKey = 'civic-connect.access-token';
  private readonly roleStorageKey = 'civic-connect.role';

  get accessToken(): string | null {
    return localStorage.getItem(this.tokenStorageKey);
  }

  register(request: RegisterRequest): Observable<RegisteredAccount> {
    return this.http.post<RegisteredAccount>('/api/v1/auth/register', request);
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/v1/auth/login', { email, password })
      .pipe(tap((response) => this.storeLogin(response)));
  }

  private storeLogin(response: LoginResponse): void {
    if (!response.accessToken || !isUserRole(response.role)) {
      throw new Error('The sign-in response contains invalid account details.');
    }

    localStorage.setItem(this.tokenStorageKey, response.accessToken);
    localStorage.setItem(this.roleStorageKey, response.role);
  }
}

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(AuthService).accessToken;

  return token
    ? next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }))
    : next(request);
};

export function loginErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 401) {
      return 'Wrong email or password';
    }

    if (error.status === 0) {
      return 'Unable to connect to the server. Please try again.';
    }

  }

  return 'Sign in failed. Please try again.';
}

export function registerErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 409) {
      return 'An account with this email already exists. Try signing in instead.';
    }

    if (error.status === 422) {
      return 'Check your name, email, phone number, and password, then try again.';
    }

    if (error.status === 0) {
      return 'Unable to connect to the server. Please try again.';
    }

    if (error.status >= 500) {
      return 'The server could not save your account. Check that the latest database migrations are applied, then try again.';
    }
  }

  return 'Account registration failed. Please try again.';
}
