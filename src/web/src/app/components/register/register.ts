import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, isUserRole, registerErrorMessage } from '../../core/auth';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  showPassword = false;
  isSubmitting = false;
  errorMessage = '';

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onRegister(): void {
    if (this.isSubmitting) {
      return;
    }

    this.errorMessage = '';
    this.isSubmitting = true;
    const name = `${this.registration.firstName.trim()} ${this.registration.surname.trim()}`.trim();

    this.auth.register({
      name,
      email: this.registration.email.trim(),
      phoneNumber: this.registration.phoneNumber.trim(),
      password: this.registration.password,
    }).subscribe({
      next: () => {
        this.auth.login(this.registration.email.trim(), this.registration.password).subscribe({
          next: (response) => {
            if (!isUserRole(response.role) || response.role !== 'Requester') {
              this.errorMessage = 'Your account was created, but automatic sign-in failed. Please sign in.';
              this.isSubmitting = false;
              return;
            }

            void this.router.navigateByUrl('/requester');
            this.isSubmitting = false;
          },
          error: () => {
            this.errorMessage =
              'Your account was created, but automatic sign-in failed. Please sign in; if it still fails, the server may need its latest database migrations.';
            this.isSubmitting = false;
          },
        });
      },
      error: (error: unknown) => {
        this.errorMessage = registerErrorMessage(error);
        this.isSubmitting = false;
      },
    });
  }

  readonly registration = {
    firstName: '',
    surname: '',
    phoneNumber: '',
    email: '',
    password: '',
  };
}
