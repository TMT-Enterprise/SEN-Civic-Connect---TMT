import { Component } from '@angular/core';

import {
  RequestCard,
  ServiceRequest
} from '../../../shared/request-card/request-card';

import {
  RequestDetailModal
} from '../../../shared/request-detail-modal/request-detail-modal';

@Component({
  selector: 'app-request-history',
  standalone: true,
  imports: [
    RequestCard,
    RequestDetailModal
  ],
  templateUrl: './request-history.html',
  styleUrl: './request-history.css',
})
export class RequestHistory {
  requests: ServiceRequest[] = [
    {
      id: 'REQ-001',
      citizen: 'Jane Doe',
      category: 'Pothole Repair',
      description:
        'Large pothole on Main Street causing traffic hazards.',
      priority: 'High',
      status: 'In Progress',
      dueDate: '2026-10-25',
      contactPhone: '012 345 6789',
      contactEmail: 'jane.doe@example.com'
    },
    {
      id: 'REQ-002',
      citizen: 'John Smith',
      category: 'Damaged Equipment',
      description:
        'A damaged public facility needs inspection and repair.',
      priority: 'Medium',
      status: 'Assigned',
      dueDate: '2026-10-28',
      contactPhone: '011 234 5678',
      contactEmail: 'john.smith@example.com'
    }
  ];

  selectedRequest: ServiceRequest | null = null;
  isDetailsOpen = false;

  openDetails(request: ServiceRequest): void {
    this.selectedRequest = request;
    this.isDetailsOpen = true;
  }

  closeDetails(): void {
    this.isDetailsOpen = false;
    this.selectedRequest = null;
  }
}