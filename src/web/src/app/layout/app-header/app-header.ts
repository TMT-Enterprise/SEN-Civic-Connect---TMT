import { Component } from '@angular/core';
import { LogoutModal } from '../../shared/logout-modal/logout-modal';

@Component({
  selector: 'app-app-header',
  imports: [
    LogoutModal
  ],
  templateUrl: './app-header.html',
  styleUrl: './app-header.css',
})
export class AppHeader {
  isLogoutOpen = false;

openLogout(): void {
  this.isLogoutOpen = true;
}

closeLogout(): void {
  this.isLogoutOpen = false;
}
}
