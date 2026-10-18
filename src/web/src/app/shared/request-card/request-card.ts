import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface ServiceRequest {
  id: string;
  citizen: string;
  category: string;
  description: string;
  priority: 'Critical' | 'High' | 'Medium' | 'Normal';
  status:
    | 'Assigned'
    | 'In Progress'
    | 'Resolved'
    | 'Closed'
    | 'Overdue'
    | 'Rejected';
  dueDate: string;

  // Optional details used by the modal
  contactPhone?: string;
  contactEmail?: string;
}

@Component({
  selector: 'app-request-card',
  standalone: true,
  templateUrl: './request-card.html',
  styleUrl: './request-card.css'
})
export class RequestCard {
  @Input({ required: true }) request!: ServiceRequest;

  @Output() viewDetails = new EventEmitter<ServiceRequest>();

  get statusLabel(): string {
    return this.request.status === 'Assigned'
      ? 'Pending Accept'
      : this.request.status;
  }

  onViewDetails(): void {
    this.viewDetails.emit(this.request);
  }
}