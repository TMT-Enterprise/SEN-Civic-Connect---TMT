import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
@Component({ selector: 'app-login', imports: [FormsModule, RouterLink], templateUrl: './login.html', styleUrl: './login.css' })
export class Login {
  private router = inject(Router); private auth = inject(AuthService);
  showPassword = false; loginObj = { email: '', password: '' }; errorMessage = ''; loading = false;
  togglePassword(): void { this.showPassword = !this.showPassword; }
  onLogin(): void {
    if (this.loading) return; this.loading = true; this.errorMessage = '';
    this.auth.login(this.loginObj.email.trim(), this.loginObj.password).subscribe({
      next: response => { this.loading = false; void this.router.navigateByUrl(this.auth.destinationFor(response.role)); },
      error: error => { this.loading = false; this.errorMessage = error?.status === 0 ? 'Cannot connect to Civic Connect. Check that the API is running.' : (error?.error?.detail || 'Invalid email or password.'); }
    });
  }
}
