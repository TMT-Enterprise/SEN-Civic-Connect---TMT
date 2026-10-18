
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { RequestsService } from '../../../core/requests';

@Component({
  selector: 'app-new-request',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './new-request.html',
  styleUrl: './new-request.css',
})
export class NewRequest {
  private readonly requestsService = inject(RequestsService);
  private readonly router = inject(Router);

  categories = [
    { id: 1, name: 'Facility faults' },
    { id: 2, name: 'Damaged equipment' },
    { id: 3, name: 'Security concerns' },
    { id: 4, name: 'IT support' },
    { id: 5, name: 'Maintenance issues' },
    { id: 6, name: 'Lost property' },
    { id: 7, name: 'Other' },
  ];

  categoryId: number | null = null;
  description = '';
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly createdRequestId = signal<number | null>(null);

  submitRequest(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.createdRequestId.set(null);

    const trimmedDescription = this.description.trim();

    if (this.categoryId === null || !trimmedDescription) {
      this.errorMessage.set(
        'Please select a category and enter a description.',
      );
      return;
    }

    this.isSubmitting.set(true);

    this.requestsService
      .submitRequest({
        categoryId: this.categoryId,
        description: trimmedDescription,
      })
      .subscribe({
        next: (response) => {
          this.isSubmitting.set(false);
          this.successMessage.set(
            `Request REQ-${String(response.requestId).padStart(3, '0')} was submitted successfully.`,
          );
          this.createdRequestId.set(response.requestId);
          this.categoryId = null;
          this.description = '';
        },
        error: (error: unknown) => {
          this.isSubmitting.set(false);
          console.error('Request submission failed:', error);

          if (error instanceof HttpErrorResponse) {
            if (error.status === 401) {
              this.errorMessage.set(
                'Your session has expired. Please sign in again.',
              );
            } else if (error.status === 403) {
              this.errorMessage.set(
                'Only requester accounts can submit service requests.',
              );
            } else if (error.status === 422) {
              this.errorMessage.set(
                'The category or description was rejected. Please check your entries.',
              );
            } else if (error.status === 0) {
              this.errorMessage.set(
                'Could not connect to the API. Check that the backend is running.',
              );
            } else {
              this.errorMessage.set(
                `Request submission failed (HTTP ${error.status}). Please try again.`,
              );
            }
          } else {
            this.errorMessage.set(
              'An unexpected error occurred. Please try again.',
            );
          }
        },
      });
  }

  goToRequests(): void {
    void this.router.navigate(['/requester']);
  }
}
