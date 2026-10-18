import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { AuthService, authInterceptor } from './auth';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpClient;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    localStorage.removeItem('civic-connect.access-token');
    localStorage.removeItem('civic-connect.role');
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.removeItem('civic-connect.access-token');
    localStorage.removeItem('civic-connect.role');
  });

  it('sends credentials to the API and stores the successful login token and role', () => {
    let receivedRole: string | undefined;
    service.login('person@example.com', 'correct-password').subscribe((response) => {
      receivedRole = response.role;
    });

    const request = httpTesting.expectOne('/api/v1/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'person@example.com',
      password: 'correct-password',
    });
    request.flush({
      accessToken: 'signed-token',
      tokenType: 'Bearer',
      expiresAt: '2026-10-18T03:00:00Z',
      role: 'Staff',
    });

    expect(receivedRole).toBe('Staff');
    expect(localStorage.getItem('civic-connect.access-token')).toBe('signed-token');
    expect(localStorage.getItem('civic-connect.role')).toBe('Staff');

    http.get('/api/protected').subscribe();
    const protectedRequest = httpTesting.expectOne('/api/protected');
    expect(protectedRequest.request.headers.get('Authorization')).toBe('Bearer signed-token');
    protectedRequest.flush({});
  });

  it('does not store credentials when the API rejects the login', () => {
    service.login('person@example.com', 'wrong-password').subscribe({
      error: () => undefined,
    });

    httpTesting.expectOne('/api/v1/auth/login').flush(
      { title: 'Invalid email or password' },
      { status: 401, statusText: 'Unauthorized' },
    );

    expect(localStorage.getItem('civic-connect.access-token')).toBeNull();
    expect(localStorage.getItem('civic-connect.role')).toBeNull();
  });

  it('does not store a token when the login response has an unsupported role', () => {
    service.login('person@example.com', 'password').subscribe({
      error: () => undefined,
    });

    httpTesting.expectOne('/api/v1/auth/login').flush({
      accessToken: 'unexpected-token',
      tokenType: 'Bearer',
      expiresAt: '',
      role: 'Other',
    });

    expect(localStorage.getItem('civic-connect.access-token')).toBeNull();
    expect(localStorage.getItem('civic-connect.role')).toBeNull();
  });
});
