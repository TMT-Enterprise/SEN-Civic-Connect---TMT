import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginResponse, UserProfile } from '../models/request.model';

const API = '/api/v1/auth';
interface JwtPayload {
  sub?: string;
  role?: string;
  acct?: string;
  email?: string;
  exp?: number;
  [key: string]: unknown;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly tokenKey = 'civicconnect.accessToken';
  private readonly roleKey = 'civicconnect.role';

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API}/login`, { email, password }).pipe(tap(r => {
      localStorage.setItem(this.tokenKey, r.accessToken);
      localStorage.setItem(this.roleKey, r.role);
    }));
  }

  register(name: string, email: string, password: string): Observable<unknown> {
    return this.http.post(`${API}/register`, { name, email, password });
  }

  me(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${API}/me`);
  }

  get token(): string | null {
    if (typeof localStorage === 'undefined') return null;
    return localStorage.getItem(this.tokenKey);
  }

  private parseTokenPayload(): JwtPayload | null {
    const token = this.token;
    if (!token) return null;
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(json) as JwtPayload;
    } catch {
      return null;
    }
  }

  get role(): string | null {
    const payload = this.parseTokenPayload();
    if (!payload) return null;

    if (payload.exp && Date.now() >= payload.exp * 1000) {
      this.logout();
      return null;
    }

    const tokenRole = payload.role || (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as string);
    return tokenRole || (typeof localStorage !== 'undefined' ? localStorage.getItem(this.roleKey) : null);
  }

  get isAuthenticated(): boolean {
    const payload = this.parseTokenPayload();
    if (!payload) return false;
    if (payload.exp && Date.now() >= payload.exp * 1000) {
      this.logout();
      return false;
    }
    return !!this.token;
  }

  logout(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.roleKey);
    }
  }

  destinationFor(role: string | null): string {
    switch (role) {
      case 'Admin': return '/admin/requests';
      case 'Staff': return '/staff';
      case 'Requester': return '/requester';
      default: return '/login';
    }
  }
}
