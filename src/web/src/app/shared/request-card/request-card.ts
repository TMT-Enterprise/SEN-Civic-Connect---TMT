import { Component, EventEmitter, Input, Output } from '@angular/core';


export interface ServiceRequest {
  id: string;
  citizen: string;
  category: string;
  description: string;
  requestId?: number;
  priority: 'Critical' | 'High' | 'Medium' | 'Normal' | null;
  status:
    | 'Submitted'
    | 'Assigned'
    | 'In Progress'
    | 'Resolved'
    | 'Closed'
    | 'Withdrawn'
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
  @Input() allowWithdraw = false;

  @Output() viewDetails = new EventEmitter<ServiceRequest>();
  @Output() withdraw = new EventEmitter<ServiceRequest>();

  get statusLabel(): string {
    return this.request.status === 'Assigned'
      ? 'Pending Accept'
      : this.request.status;
  }

  onViewDetails(): void {
    this.viewDetails.emit(this.request);
  }

  onWithdraw(): void {
    this.withdraw.emit(this.request);
  }
}