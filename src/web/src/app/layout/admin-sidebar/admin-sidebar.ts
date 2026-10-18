import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LogoutModal } from '../../shared/logout-modal/logout-modal';

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [
    RouterLink, 
    RouterLinkActive,
    LogoutModal
  ],
  templateUrl: './admin-sidebar.html',
  styleUrl: './admin-sidebar.css',
})
export class AdminSidebar {
  isLogoutOpen = false;

openLogout(): void {
  this.isLogoutOpen = true;
}

closeLogout(): void {
  this.isLogoutOpen = false;
}
}
