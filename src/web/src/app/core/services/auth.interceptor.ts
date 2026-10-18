import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.token;

  const authReq = token && req.url.startsWith('/api/v1/')
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        // If unauthenticated or token is expired/invalid (and not the login attempt itself)
        if (error.status === 401 && !req.url.includes('/api/v1/auth/login')) {
          auth.logout();
          void router.navigateByUrl('/login');
        } else if (error.status === 403) {
          // Forbidden: user attempted to call an endpoint prohibited for their role
          void router.navigateByUrl(auth.destinationFor(auth.role));
        }
      }
      return throwError(() => error);
    })
  );
};

