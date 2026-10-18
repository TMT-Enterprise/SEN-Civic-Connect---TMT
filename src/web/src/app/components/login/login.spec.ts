import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../core/auth';
import { Login } from './login';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let auth: AuthService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    auth = TestBed.inject(AuthService);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it.each([
    ['Requester', '/requester'],
    ['Staff', '/staff'],
    ['Admin', '/admin'],
  ])('signs in a %s and opens its dashboard', (role, destination) => {
    vi.spyOn(auth, 'login').mockReturnValue(
      of({ accessToken: 'signed-token', tokenType: 'Bearer', expiresAt: '', role }),
    );
    const navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    component.loginObj.email = 'person@example.com';
    component.loginObj.password = 'correct-password';

    component.onLogin();

    expect(auth.login).toHaveBeenCalledWith('person@example.com', 'correct-password');
    expect(navigateSpy).toHaveBeenCalledWith(destination);
    expect(component.errorMessage).toBe('');
  });

  it('shows an error when the API rejects the credentials', () => {
    vi.spyOn(auth, 'login').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401 })),
    );
    const navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigateByUrl');

    component.onLogin();

    expect(navigateSpy).not.toHaveBeenCalled();
    expect(component.errorMessage).toBe('Wrong email or password');
  });

  it('rejects an unsupported account role', () => {
    vi.spyOn(auth, 'login').mockReturnValue(
      of({ accessToken: 'signed-token', tokenType: 'Bearer', expiresAt: '', role: 'Other' }),
    );
    const navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigateByUrl');

    component.onLogin();

    expect(navigateSpy).not.toHaveBeenCalled();
    expect(component.errorMessage).toBe('This account has an unsupported role.');
  });
});
