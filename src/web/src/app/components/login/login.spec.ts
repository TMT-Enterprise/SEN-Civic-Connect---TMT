import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Login } from './login';
import { provideRouter, Router } from '@angular/router';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  //cover the hardcoded login and will need updating with real backend
  it('should navigate to the requester dashboard with valid credentials', () => {
    const navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    component.loginObj.email = 'test@gmail.com';
    component.loginObj.password = 'test123';

    component.onLogin();

    expect(navigateSpy).toHaveBeenCalledWith('/layout/requester');
    expect(component.errorMessage).toBe('');
  });

  it('should show an error with invalid credentials', () => {
    const navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigateByUrl');
    component.loginObj.email = 'wrong@gmail.com';
    component.loginObj.password = 'nope';

    component.onLogin();

    expect(navigateSpy).not.toHaveBeenCalled();
    expect(component.errorMessage).toBe('Wrong email or password');
  });
});
