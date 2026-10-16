import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../shared/sidebar/sidebar';
import { AppHeader } from '../app-header/app-header';

@Component({
  selector: 'app-staff-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    Sidebar,
    AppHeader,
  ],
  templateUrl: './staff-layout.html',
  styleUrl: './staff-layout.css',
})
export class StaffLayout {}
