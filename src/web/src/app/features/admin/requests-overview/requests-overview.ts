import { Component } from '@angular/core';
import { RequestDetailModal } from '../../../shared/request-detail-modal/request-detail-modal';
import { ServiceRequest } from '../../../shared/request-card/request-card';
import { AssignStaffModal } from '../assign-staff-modal/assign-staff-modal';

interface OverviewRow {
  request: ServiceRequest;
  assignedTo?: string;     // shown in the table only
  displayStatus?: string;  // overrides the status label
}

@Component({
  selector: 'app-request-overview',
  standalone: true,
  imports: [RequestDetailModal, AssignStaffModal],
  templateUrl: './requests-overview.html',
  styleUrl: './requests-overview.css',
})
export class RequestsOverview {
  staffMembers: string[] = ['Marcus Vance', 'James Wilson', 'Elena Rostova'];

  rows: OverviewRow[] = [
    {
      displayStatus: 'Submitted',
      request: {
        id: 'REQ-9101', citizen: 'Sarah Connor', category: 'Electrical',
        description: 'Electrical fault reported, awaiting assignment.',
        priority: 'High', status: 'Assigned', dueDate: '25 Oct 2026, 16:00',
        contactPhone: '082 123 4567',
      },
    },
    {
      assignedTo: 'Marcus Vance',
      request: {
        id: 'REQ-9012', citizen: 'Eleanor Vance', category: 'Plumbing',
        description: 'Plumbing issue assigned to a specialist.',
        priority: 'High', status: 'Assigned', dueDate: '28 Oct 2026, 14:00',
        contactPhone: '082 234 5678',
      },
    },
    {
      assignedTo: 'Marcus Vance',
      request: {
        id: 'REQ-8492', citizen: 'Alex Morgan', category: 'Plumbing',
        description: 'Plumbing repair currently in progress.',
        priority: 'Normal', status: 'In Progress', dueDate: '27 Oct 2026, 17:00',
        contactPhone: '082 345 6789',
      },
    },
    {
      assignedTo: 'James Wilson',
      request: {
        id: 'REQ-8102', citizen: 'Carlos Rivera', category: 'Maintenance',
        description: 'Maintenance work completed and awaiting closure.',
        priority: 'High', status: 'Resolved', dueDate: '28 Oct 2026, 10:00',
        contactPhone: '082 456 7890',
      },
    },
  ];

  // Details modal
  selectedRequest: ServiceRequest | null = null;
  isDetailsOpen = false;

  // Assign modal
  assigningRow: OverviewRow | null = null;
  isAssignOpen = false;

  openDetails(request: ServiceRequest): void {
    this.selectedRequest = request;
    this.isDetailsOpen = true;
  }

  closeDetails(): void {
    this.isDetailsOpen = false;
    this.selectedRequest = null;
  }

  openAssign(row: OverviewRow): void {
    this.assigningRow = row;
    this.isAssignOpen = true;
  }

  closeAssign(): void {
    this.isAssignOpen = false;
    this.assigningRow = null;
  }

  assignStaff(name: string): void {
    const target = this.assigningRow;
    if (!target) {
      return;
    }

    this.rows = this.rows.map(row =>
      row === target
        ? { ...row, assignedTo: name, displayStatus: undefined }
        : row
    );

    this.closeAssign();
  }

  statusLabel(row: OverviewRow): string {
    return row.displayStatus ?? row.request.status;
  }

  statusClass(row: OverviewRow): string {
    return this.statusLabel(row).toLowerCase().replace(/\s+/g, '-');
  }
}