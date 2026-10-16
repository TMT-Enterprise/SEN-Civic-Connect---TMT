import { Component } from '@angular/core';
import { RequestCard, ServiceRequest } from '../../../shared/request-card/request-card';

@Component({
  selector: 'app-request-history',
  standalone: true,
  imports: [
    RequestCard
  ],
  templateUrl: './request-history.html',
  styleUrl: './request-history.css',
})
export class RequestHistory {
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
}