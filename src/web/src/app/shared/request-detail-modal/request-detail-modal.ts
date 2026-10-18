import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ServiceRequest } from '../request-card/request-card';

@Component({
  selector: 'app-request-detail-modal',
  standalone: true,
  templateUrl: './request-detail-modal.html',
  styleUrl: './request-detail-modal.css'
})
export class RequestDetailModal {
  @Input() request: ServiceRequest | null = null;
  @Input() isOpen = false;

  @Output() closed = new EventEmitter<void>();

  close(): void {
    this.closed.emit();
  }

  get statusLabel(): string {
    if (!this.request) {
      return '';
    }

    return this.request.status === 'Assigned'
      ? 'Pending Accept'
      : this.request.status;
  }

  get formattedDueDate(): string {
    if (!this.request?.dueDate) {
      return 'Not provided';
    }

    // Preserve an ISO date's calendar day when formatting it.
    const datePart = this.request.dueDate.split('T')[0];
    const parts = datePart.split('-');

    if (parts.length === 3) {
      const [year, month, day] = parts;

      if (year && month && day) {
        const date = new Date(
          Number(year),
          Number(month) - 1,
          Number(day)
        );

        if (!Number.isNaN(date.getTime())) {
          return new Intl.DateTimeFormat('en-ZA', {
            day: '2-digit',
            month: 'short',
            year: 'numeric'
          }).format(date);
        }
      }
    }

    return this.request.dueDate;
  }
}