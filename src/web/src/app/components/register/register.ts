import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
@Component({ selector: 'app-register', imports: [FormsModule, RouterLink], templateUrl: './register.html', styleUrl: './register.css' })
export class Register {
  private auth = inject(AuthService); private router = inject(Router);
  showPassword = false; loading = false; errorMessage = '';
  model = { name: '', surname: '', email: '', password: '' };
  togglePassword(): void { this.showPassword = !this.showPassword; }
  onRegister(): void {
    this.errorMessage = ''; this.loading = true;
    this.auth.register(`${this.model.name.trim()} ${this.model.surname.trim()}`.trim(), this.model.email.trim(), this.model.password).subscribe({
      next: () => this.auth.login(this.model.email.trim(), this.model.password).subscribe({ next: r => { this.loading = false; void this.router.navigateByUrl(this.auth.destinationFor(r.role)); }, error: () => { this.loading = false; void this.router.navigateByUrl('/login'); } }),
      error: e => { this.loading = false; this.errorMessage = e?.error?.detail || 'Registration failed. Check your details and try again.'; }
    });
  }
}
