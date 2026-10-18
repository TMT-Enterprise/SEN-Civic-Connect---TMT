import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LogoutModal } from '../../../shared/logout-modal/logout-modal';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    LogoutModal,
  ],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  isLogoutOpen = false;

openLogout(): void {
  this.isLogoutOpen = true;
}

closeLogout(): void {
  this.isLogoutOpen = false;
}
}
