import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';

import { AuthService } from '../../../core/auth';
import {
  RequestHistoryItem,
  RequestsService,
} from '../../../core/requests';
import {
  RequestCard,
  ServiceRequest,
} from '../../../shared/request-card/request-card';
import { RequestDetailModal } from '../../../shared/request-detail-modal/request-detail-modal';

@Component({
  selector: 'app-request-history',
  standalone: true,
  imports: [RequestCard, RequestDetailModal],
  templateUrl: './request-history.html',
  styleUrl: './request-history.css',
})
export class RequestHistory implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly requestsService = inject(RequestsService);

  readonly requests = signal<ServiceRequest[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly withdrawalMessage = signal('');
  readonly withdrawalError = signal('');
  readonly selectedRequest = signal<ServiceRequest | null>(null);

  get userName(): string {
    return this.auth.userName || 'there';
  }

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.requestsService.getMyRequests().subscribe({
      next: (items) => {
        this.requests.set(items.map((item) => this.toCard(item)));
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        console.error('Failed to load request history:', error);
        this.isLoading.set(false);
        this.errorMessage.set(
          error instanceof HttpErrorResponse && error.status === 401
            ? 'Your session may have expired. Please sign in again.'
            : 'We could not load your requests. Please try again.',
        );
      },
    });
  }

  openDetails(request: ServiceRequest): void {
    this.selectedRequest.set(request);
  }

  closeDetails(): void {
    this.selectedRequest.set(null);
  }

  withdrawRequest(request: ServiceRequest): void {
    if (
      request.requestId === undefined ||
      request.status !== 'Submitted' ||
      !window.confirm(`Withdraw ${request.id}? This request will remain in your history.`)
    ) {
      return;
    }

    this.withdrawalMessage.set('');
    this.withdrawalError.set('');
    this.requestsService.withdrawRequest(request.requestId).subscribe({
      next: () => {
        this.requests.update((items) =>
          items.map((item) =>
            item.requestId === request.requestId
              ? { ...item, status: 'Withdrawn' }
              : item,
          ),
        );
        this.withdrawalMessage.set(`${request.id} was withdrawn.`);
      },
      error: (error: unknown) => {
        console.error('Failed to withdraw request:', error);
        if (error instanceof HttpErrorResponse && error.status === 409) {
          this.withdrawalError.set(
            'This request can no longer be withdrawn. Your history has been refreshed.',
          );
          this.loadRequests();
        } else {
          this.withdrawalError.set(
            'We could not withdraw this request. Please try again.',
          );
        }
      },
    });
  }

  private toCard(item: RequestHistoryItem): ServiceRequest {
    const statuses: ServiceRequest['status'][] = [
      'Submitted',
      'Assigned',
      'In Progress',
      'Resolved',
      'Closed',
      'Withdrawn',
      'Overdue',
      'Rejected',
    ];
    const status = statuses.includes(item.status as ServiceRequest['status'])
      ? (item.status as ServiceRequest['status'])
      : 'Submitted';

    return {
      id: `REQ-${String(item.requestId).padStart(3, '0')}`,
      requestId: item.requestId,
      citizen: this.userName,
      category: item.category,
      description: item.description,
      priority: null,
      status,
      dueDate: item.dueAt
        ? new Date(item.dueAt).toLocaleDateString()
        : 'Not scheduled',
    };
  }
}
