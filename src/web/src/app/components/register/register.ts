import { Component } from '@angular/core';

@Component({
  selector: 'app-register',
  imports: [],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {

    //Password toggle
  showPassword = false;

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}
