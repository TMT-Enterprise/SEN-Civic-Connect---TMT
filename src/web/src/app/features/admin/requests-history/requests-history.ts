import { Component } from '@angular/core';
import { RequestDetailModal } from '../../../shared/request-detail-modal/request-detail-modal';
import { ServiceRequest } from '../../../shared/request-card/request-card';

type HistoryFilter = 'all' | 'closed' | 'overdue' | 'rejected';

interface HistoryRow {
  request: ServiceRequest;
  assignedTo: string;
  endLabel: string;          // "Archived" | "Overdue" | "Rejected"
  endDate: string;
  rejectionReason?: string;
}

@Component({
  selector: 'app-requests-history',
  standalone: true,
  imports: [RequestDetailModal],
  templateUrl: './requests-history.html',
  styleUrl: './requests-history.css',
})
export class RequestsHistory {
  filters: { key: HistoryFilter; label: string }[] = [
    { key: 'all', label: 'All Archived' },
    { key: 'closed', label: 'Closed' },
    { key: 'overdue', label: 'Overdue' },
    { key: 'rejected', label: 'Rejected' },
  ];

  activeFilter: HistoryFilter = 'all';

  rows: HistoryRow[] = [
    {
      assignedTo: 'James Wilson', endLabel: 'Archived', endDate: '01 Oct 2026',
      request: {
        id: 'REQ-6102', citizen: 'Pam Beesly', category: 'Maintenance',
        description: 'Radiator bleeding and pressure balancing on the third floor.',
        priority: 'Normal', status: 'Closed', dueDate: '02 Oct 2026',
      },
    },
    {
      assignedTo: 'Elena Rostova', endLabel: 'Overdue', endDate: '11 Oct 2026',
      request: {
        id: 'REQ-5702', citizen: 'Taylor Morgan', category: 'Electrical',
        description: 'Streetlight repair near the community centre.',
        priority: 'High', status: 'Overdue', dueDate: '10 Oct 2026',
      },
    },
    {
      assignedTo: 'Elena Rostova', endLabel: 'Rejected', endDate: '01 Oct 2026',
      rejectionReason: 'Requires high-voltage master certification.',
      request: {
        id: 'REQ-5900', citizen: 'Robert California', category: 'Electrical',
        description: 'High-voltage breaker replacement.',
        priority: 'Critical', status: 'Rejected', dueDate: '10 Oct 2026',
      },
    },
  ];

  selectedRequest: ServiceRequest | null = null;
  isDetailsOpen = false;

  get filteredRows(): HistoryRow[] {
    if (this.activeFilter === 'all') return this.rows;
    return this.rows.filter(r => r.request.status.toLowerCase() === this.activeFilter);
  }

  setFilter(filter: HistoryFilter): void {
    this.activeFilter = filter;
  }

  openDetails(request: ServiceRequest): void {
    this.selectedRequest = request;
    this.isDetailsOpen = true;
  }

  closeDetails(): void {
    this.isDetailsOpen = false;
    this.selectedRequest = null;
  }
}