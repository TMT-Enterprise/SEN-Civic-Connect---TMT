import { Component } from '@angular/core';
import { RequestDetailModal } from '../../../../shared/request-detail-modal/request-detail-modal';
import { ServiceRequest } from '../../../../shared/request-card/request-card';

@Component({
  selector: 'app-request-table',
  standalone: true,
  imports: [RequestDetailModal],
  templateUrl: './request-table.html',
  styleUrl: './request-table.css',
})
export class RequestTable {
  requests: ServiceRequest[] = [
    {
      id: 'REQ-001',
      citizen: 'Alex Morgan',
      category: 'Maintenance',
      description: 'Maintenance request awaiting action.',
      priority: 'Normal',
      status: 'Assigned',
      dueDate: '15 Oct 2026',
      contactPhone: '082 345 6789',
    },
    {
      id: 'REQ-002',
      citizen: 'Jamie Smith',
      category: 'IT Support',
      description: 'IT support issue currently being worked on.',
      priority: 'High',
      status: 'In Progress',
      dueDate: '14 Oct 2026',
    },
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

  statusClass(status: string): string {
    return status.toLowerCase().replace(/\s+/g, '-');
  }
}