import { Component } from '@angular/core';
import { StatusBadge } from '../../../../shared/status-badge/status-badge';

@Component({
  selector: 'app-request-table',
  standalone: true,
  imports: [
    StatusBadge
  ],
  templateUrl: './request-table.html',
  styleUrl: './request-table.css',
})
export class RequestTable {}
