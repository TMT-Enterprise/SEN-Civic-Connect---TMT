import { Component } from '@angular/core';

@Component({
  selector: 'app-login',
  imports: [],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  //Password toggle
  showPassword = false;

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}
