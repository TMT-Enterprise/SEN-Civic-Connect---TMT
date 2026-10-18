import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  AdminRequestItem,
  AssignableEmployee,
  RequestsService,
} from '../../../core/requests';

type QueueFilter = 'All Active' | 'Unassigned' | 'Assigned' | 'In Progress' | 'Resolved';

@Component({
  selector: 'app-requests-overview',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './requests-overview.html',
  styleUrl: './requests-overview.css',
})
export class RequestsOverview implements OnInit {
  private readonly requestsService = inject(RequestsService);

  readonly filters: QueueFilter[] = [
    'All Active',
    'Unassigned',
    'Assigned',
    'In Progress',
    'Resolved',
  ];
  readonly requests = signal<AdminRequestItem[]>([]);
  readonly employees = signal<AssignableEmployee[]>([]);
  readonly isLoading = signal(true);
  readonly isLoadingEmployees = signal(false);
  readonly isAssigning = signal(false);
  readonly errorMessage = signal('');
  readonly assignmentError = signal('');
  readonly successMessage = signal('');
  readonly filter = signal<QueueFilter>('All Active');
  readonly assignmentRequest = signal<AdminRequestItem | null>(null);
  readonly selectedEmployeeId = signal<number | null>(null);
  readonly dueAt = signal(this.localDateTimeAfterHours(48));

  readonly filteredRequests = computed(() => {
    const rows = this.requests();

    switch (this.filter()) {
      case 'Unassigned':
        return rows.filter((request) => request.status === 'Submitted');
      case 'Assigned':
        return rows.filter((request) => request.status === 'Assigned');
      case 'In Progress':
        return rows.filter((request) => request.status === 'In Progress');
      case 'Resolved':
        return rows.filter((request) => request.status === 'Resolved');
      default:
        return rows.filter((request) =>
          ['Submitted', 'Assigned', 'In Progress'].includes(request.status),
        );
    }
  });

  readonly activeCount = computed(
    () =>
      this.requests().filter((request) =>
        ['Submitted', 'Assigned', 'In Progress'].includes(request.status),
      ).length,
  );
  readonly unassignedCount = computed(
    () => this.requests().filter((request) => request.status === 'Submitted').length,
  );
  readonly assignedCount = computed(
    () => this.requests().filter((request) => request.status === 'Assigned').length,
  );
  readonly inProgressCount = computed(
    () =>
      this.requests().filter((request) => request.status === 'In Progress').length,
  );
  readonly resolvedCount = computed(
    () => this.requests().filter((request) => request.status === 'Resolved').length,
  );

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.requestsService.getAdminRequests().subscribe({
      next: (response) => {
        this.requests.set(response.requests);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        console.error('Failed to load the admin request queue:', error);
        this.isLoading.set(false);
        this.errorMessage.set(
          error instanceof HttpErrorResponse && error.status === 403
            ? 'Your account does not have permission to manage requests.'
            : 'We could not load the request queue. Please try again.',
        );
      },
    });
  }

  selectFilter(filter: QueueFilter): void {
    this.filter.set(filter);
  }

  openAssignment(request: AdminRequestItem): void {
    this.assignmentError.set('');
    this.selectedEmployeeId.set(null);
    this.dueAt.set(this.localDateTimeAfterHours(48));
    this.assignmentRequest.set(request);

    if (this.employees().length > 0) {
      return;
    }

    this.isLoadingEmployees.set(true);
    this.requestsService.getAssignableEmployees().subscribe({
      next: (employees) => {
        this.employees.set(employees);
        this.isLoadingEmployees.set(false);
      },
      error: (error: unknown) => {
        console.error('Failed to load assignable staff:', error);
        this.isLoadingEmployees.set(false);
        this.assignmentError.set(
          error instanceof HttpErrorResponse && error.status === 403
            ? 'Your account does not have permission to assign requests.'
            : 'We could not load active staff. Close this dialog and try again.',
        );
      },
    });
  }

  submitAssignment(): void {
    const request = this.assignmentRequest();
    const employeeId = this.selectedEmployeeId();
    const dueAt = new Date(this.dueAt());

    if (request === null || this.isAssigning()) {
      return;
    }

    if (employeeId === null) {
      this.assignmentError.set('Select an active staff member.');
      return;
    }

    if (
      Number.isNaN(dueAt.getTime()) ||
      dueAt.getTime() < Date.now() + 24 * 60 * 60 * 1000
    ) {
      this.assignmentError.set('Choose a due date at least 24 hours from now.');
      return;
    }

    this.assignmentError.set('');
    this.isAssigning.set(true);

    this.requestsService
      .assignRequest(request.requestId, {
        employeeId,
        dueAt: dueAt.toISOString(),
      })
      .subscribe({
        next: () => {
          this.assignmentRequest.set(null);
          this.isAssigning.set(false);
          this.successMessage.set(
            `${this.requestLabel(request.requestId)} was assigned successfully.`,
          );
          this.loadRequests();
        },
        error: (error: unknown) => {
          console.error('Request assignment failed:', error);
          this.isAssigning.set(false);
          this.assignmentError.set(this.assignmentErrorMessage(error));
        },
      });
  }

  requestLabel(requestId: number): string {
    return `REQ-${String(requestId).padStart(3, '0')}`;
  }

  formatDate(value: string | null): string {
    return value ? new Date(value).toLocaleString() : 'Not scheduled';
  }

  closeAssignment(): void {
    if (!this.isAssigning()) {
      this.assignmentRequest.set(null);
      this.assignmentError.set('');
    }
  }

  private localDateTimeAfterHours(hours: number): string {
    const date = new Date(Date.now() + hours * 60 * 60 * 1000);
    const offset = date.getTimezoneOffset() * 60_000;
    return new Date(date.getTime() - offset).toISOString().slice(0, 16);
  }

  private assignmentErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401) {
        return 'Your session has expired. Please sign in again.';
      }

      if (error.status === 403) {
        return 'Only administrator accounts can assign requests.';
      }

      if (error.status === 409) {
        this.loadRequests();
        return 'This request has already been assigned. The queue has been refreshed.';
      }

      if (error.status === 422) {
        const message = (error.error as { error?: unknown } | null)?.error;
        return typeof message === 'string'
          ? message
          : 'The assignment was rejected. Check the staff member and due date.';
      }

      return error.status === 0
        ? 'Could not connect to the API. Check that the backend is running.'
        : `Assignment failed (HTTP ${error.status}). Please try again.`;
    }

    return 'An unexpected error occurred. Please try again.';
  }
}
