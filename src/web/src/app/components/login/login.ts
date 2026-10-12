import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private router = inject(Router);
  //Password toggle
  showPassword = false;

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  //login
  loginObj = new LoginModel()
  errorMessage = '';

  onLogin(): void {
    // Hardcoded login - to be removed, testing purposes only
    if (this.loginObj.email === 'test@gmail.com' && this.loginObj.password === 'test123') {
      this.errorMessage = '';
      this.router.navigateByUrl(''); // route will depend on requester vs employee
    } else {
      this.errorMessage = 'Wrong email or password';
    }
  }
}


class LoginModel {
  email = '';
  password = '';
}