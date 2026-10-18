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

  private toCard(item: RequestHistoryItem): ServiceRequest {
    const supportedStatuses: ServiceRequest['status'][] = [
      'Submitted',
      'Assigned',
      'In Progress',
      'Resolved',
      'Closed',
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
