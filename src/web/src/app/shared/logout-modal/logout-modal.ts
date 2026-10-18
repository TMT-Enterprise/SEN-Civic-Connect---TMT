import { Component, HostListener, inject, input, output } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-logout-modal',
  standalone: true,
  imports: [],
  templateUrl: './logout-modal.html',
  styleUrl: './logout-modal.css',
})
export class LogoutModal {
  private router = inject(Router);

  isOpen = input(false);
  closed = output<void>();

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isOpen()) {
      this.close();
    }
  }

  close(): void {
    this.closed.emit();
  }

  confirm(): void {
    // Replace with your auth service, e.g. this.auth.logout();
    localStorage.clear();
    sessionStorage.clear();

    this.closed.emit();
    this.router.navigate(['/login']);
  }
}