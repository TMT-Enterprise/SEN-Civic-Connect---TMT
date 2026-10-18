import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Login } from './login';
import { AuthService } from '../../core/services/auth.service';

describe('Login', () => {
  let component: Login; let fixture: ComponentFixture<Login>;
  const authMock = {
    login: vi.fn((email: string, password: string) => email === 'test@gmail.com' && password === 'test123'
      ? of({ accessToken: 'token', tokenType: 'Bearer', expiresAt: '', role: 'Requester' as const })
      : throwError(() => ({ status: 401, error: { detail: 'Invalid email or password' } })),
    destinationFor: (role: string | null) => role === 'Admin' ? '/admin/requests' : role === 'Staff' ? '/staff' : '/requester'
  };
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Login], providers: [provideRouter([]), { provide: AuthService, useValue: authMock }] }).compileComponents();
    fixture = TestBed.createComponent(Login); component = fixture.componentInstance; await fixture.whenStable();
  });
  it('should create', () => expect(component).toBeTruthy());
  it('navigates to the role dashboard after successful authentication', () => {
    const navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    component.loginObj.email = 'test@gmail.com'; component.loginObj.password = 'test123'; component.onLogin();
    expect(navigateSpy).toHaveBeenCalledWith('/requester'); expect(component.errorMessage).toBe('');
  });
  it('shows an error when authentication fails', () => {
    const navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigateByUrl');
    component.loginObj.email = 'wrong@gmail.com'; component.loginObj.password = 'nope'; component.onLogin();
    expect(navigateSpy).not.toHaveBeenCalled(); expect(component.errorMessage).toBe('Invalid email or password');
  });
});
