import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../../core/auth';
import {
  RequestHistoryItem,
  RequestsService,
} from '../../../core/requests';
import { RequestDetailModal } from '../../../shared/request-detail-modal/request-detail-modal';
import {
  RequestCard,
  ServiceRequest,
} from '../../../shared/request-card/request-card';

@Component({
  selector: 'app-requester-home',
  standalone: true,
  imports: [RouterLink, RequestCard, RequestDetailModal],
  templateUrl: './requester-home.html',
  styleUrl: './requester-home.css',
})
export class RequesterHome implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly requestsService = inject(RequestsService);

  get userName(): string {
    return this.auth.userName || 'there';
  }

  readonly requests = signal<ServiceRequest[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly withdrawalMessage = signal('');
  readonly withdrawalError = signal('');
  readonly selectedRequest = signal<ServiceRequest | null>(null);

  ngOnInit(): void {
    this.loadRequests();
  }

  private loadRequests(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.requestsService.getMyRequests().subscribe({
      next: (items: RequestHistoryItem[]) => {
        this.requests.set(items.map((item) => this.toCard(item)));
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        console.error('Failed to load requests:', error);
        this.isLoading.set(false);

        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.errorMessage.set(
            'Your session may have expired. Please sign in again.',
          );
        } else if (
          error instanceof HttpErrorResponse &&
          error.status === 403
        ) {
          this.errorMessage.set(
            'Your account does not have permission to view requester history.',
          );
        } else {
          this.errorMessage.set(
            'We could not load your requests. Please try again.',
          );
        }
      },
    });
  }

  reloadRequests(): void {
    this.loadRequests();
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
    const supportedStatuses: ServiceRequest['status'][] = [
      'Submitted',
      'Assigned',
      'In Progress',
      'Resolved',
      'Closed',
      'Withdrawn',
      'Overdue',
      'Rejected',
    ];
    const status = supportedStatuses.includes(
      item.status as ServiceRequest['status'],
    )
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
