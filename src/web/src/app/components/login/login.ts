import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, isUserRole, loginErrorMessage } from '../../core/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  //Password toggle
  showPassword = false;
  isSubmitting = false;

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  //login
  loginObj = new LoginModel();
  errorMessage = '';

  onLogin(): void {
    if (this.isSubmitting) {
      return;
    }

    this.errorMessage = '';
    this.isSubmitting = true;
    this.auth.login(this.loginObj.email, this.loginObj.password).subscribe({
      next: (response) => {
        if (!isUserRole(response.role)) {
          this.errorMessage = 'This account has an unsupported role.';
          this.isSubmitting = false;
          return;
        }

        void this.router.navigateByUrl(this.destinationForRole(response.role));
        this.isSubmitting = false;
      },
      error: (error: unknown) => {
        this.errorMessage = loginErrorMessage(error);
        this.isSubmitting = false;
      },
    });
  }

  private destinationForRole(role: 'Requester' | 'Staff' | 'Admin'): string {
    switch (role) {
      case 'Requester':
        return '/requester';
      case 'Staff':
        return '/staff';
      case 'Admin':
        return '/admin';
    }
  }
}


class LoginModel {
  email = '';
  password = '';
}