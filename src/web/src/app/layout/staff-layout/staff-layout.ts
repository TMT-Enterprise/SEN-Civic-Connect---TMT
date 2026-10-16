import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../shared/sidebar/sidebar';
import { AppHeader } from '../app-header/app-header';
import { RequestDetailModal } from '../../shared/request-detail-modal/request-detail-modal';
import { Toast } from '../../shared/toast/toast';

@Component({
  selector: 'app-staff-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    Sidebar,
    AppHeader,
    RequestDetailModal,
    Toast
  ],
  templateUrl: './staff-layout.html',
  styleUrl: './staff-layout.css',
})
export class StaffLayout {}
