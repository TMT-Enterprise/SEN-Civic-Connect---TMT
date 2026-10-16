import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RequestCard, ServiceRequest } from '../../../shared/request-card/request-card';

@Component({
  selector: 'app-requester-home',
  standalone: true,
  imports: [
    RouterLink,
    RequestCard
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
}
