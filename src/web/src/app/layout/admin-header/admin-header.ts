import { Component } from '@angular/core';
import { LogoutModal } from '../../shared/logout-modal/logout-modal';

@Component({
  selector: 'app-admin-header',
  standalone: true,
  imports: [LogoutModal],
  templateUrl: './admin-header.html',
  styleUrl: './admin-header.css',
})
export class AdminHeader {
  isLogoutOpen = false;

openLogout(): void {
  this.isLogoutOpen = true;
}

closeLogout(): void {
  this.isLogoutOpen = false;
}
}
