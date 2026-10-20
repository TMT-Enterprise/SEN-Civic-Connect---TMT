import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';

import { Register } from './register';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    localStorage.removeItem('civic-connect.access-token');
    localStorage.removeItem('civic-connect.role');
    await fixture.whenStable();
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.removeItem('civic-connect.access-token');
    localStorage.removeItem('civic-connect.role');
  });

  it('creates the account, signs in, and opens the requester dashboard', () => {
    const navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    component.registration.firstName = 'Ada';
    component.registration.surname = 'Lovelace';
    component.registration.phoneNumber = '+27123456789';
    component.registration.email = ' Ada@example.com ';
    component.registration.password = 'Str0ng!Passw0rd';

    component.onRegister();

    const registerRequest = httpTesting.expectOne('/api/v1/auth/register');
    expect(registerRequest.request.method).toBe('POST');
    expect(registerRequest.request.body).toEqual({
      name: 'Ada Lovelace',
      email: 'Ada@example.com',
      phoneNumber: '+27123456789',
      password: 'Str0ng!Passw0rd',
    });
    registerRequest.flush({
      id: 1,
      name: 'Ada Lovelace',
      email: 'ada@example.com',
      role: 'Requester',
    });

    const loginRequest = httpTesting.expectOne('/api/v1/auth/login');
    expect(loginRequest.request.body).toEqual({
      email: 'Ada@example.com',
      password: 'Str0ng!Passw0rd',
    });
    loginRequest.flush({
      accessToken: 'signed-token',
      tokenType: 'Bearer',
      expiresAt: '2026-10-18T03:00:00Z',
      role: 'Requester',
      name: 'Ada Lovelace',
    });

    expect(localStorage.getItem('civic-connect.access-token')).toBe('signed-token');
    expect(localStorage.getItem('civic-connect.role')).toBe('Requester');
    expect(navigateSpy).toHaveBeenCalledWith('/requester');
    expect(component.errorMessage).toBe('');
  });

  it('explains when registration fails because the email is already used', () => {
    component.onRegister();

    httpTesting.expectOne('/api/v1/auth/register').flush(
      { title: 'Email already registered' },
      { status: 409, statusText: 'Conflict' },
    );

    expect(component.errorMessage).toBe(
      'An account with this email already exists. Try signing in instead.',
    );
    expect(localStorage.getItem('civic-connect.access-token')).toBeNull();
  });

  it('points to a server or database problem when registration returns a server error', () => {
    component.onRegister();

    httpTesting.expectOne('/api/v1/auth/register').flush(
      { title: 'Internal Server Error' },
      { status: 500, statusText: 'Internal Server Error' },
    );

    expect(component.errorMessage).toContain('latest database migrations');
  });
});
