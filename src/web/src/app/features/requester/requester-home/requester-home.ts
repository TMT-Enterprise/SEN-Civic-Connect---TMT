import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RequestCard, ServiceRequest } from '../../../shared/request-card/request-card';
import { RequestDetailModal } from '../../../shared/request-detail-modal/request-detail-modal';

@Component({
  selector: 'app-requester-home',
  standalone: true,
  imports: [
    RouterLink,
    RequestCard,
    RequestDetailModal
  ],
  templateUrl: './requester-home.html',
  styleUrl: './requester-home.css',
})
export class RequesterHome {
  requests: ServiceRequest[] = [
    // Example mock data to test the grid
    {
      id: 'REQ-001',
      citizen: 'Jane Doe',
      category: 'Pothole Repair',
      description: 'Large pothole on Main Street causing traffic hazards.',
      priority: 'High',
      status: 'In Progress',
      dueDate: '2026-10-25'
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
